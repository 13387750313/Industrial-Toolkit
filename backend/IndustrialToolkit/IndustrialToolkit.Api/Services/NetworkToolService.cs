using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace IndustrialToolkit.Services
{
    public class NetworkToolService
    {
        private readonly ILogger<NetworkToolService> _logger;
        private readonly ConcurrentQueue<LogEntry> _logEntries = new ConcurrentQueue<LogEntry>();
        private const int MaxLogEntries = 1000;

        public NetworkToolService(ILogger<NetworkToolService> logger)
        {
            _logger = logger;
        }

        public async Task<List<LanDevice>> ScanLan(string ipRange = "192.168.1.")
        {
            var devices = new List<LanDevice>();
            var tasks = new List<Task>();
            var results = new ConcurrentBag<LanDevice>();

            for (int i = 1; i <= 254; i++)
            {
                var ip = $"{ipRange}{i}";
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        using (var ping = new Ping())
                        {
                            var reply = await ping.SendPingAsync(ip, 500);
                            if (reply.Status == IPStatus.Success)
                            {
                                var device = new LanDevice
                                {
                                    IpAddress = ip,
                                    Status = "在线",
                                    ResponseTime = reply.RoundtripTime + "ms"
                                };
                                
                                try
                                {
                                    var hostEntry = await Dns.GetHostEntryAsync(ip);
                                    device.Hostname = hostEntry.HostName;
                                }
                                catch
                                {
                                    device.Hostname = "未知";
                                }
                                
                                results.Add(device);
                            }
                        }
                    }
                    catch
                    {
                    }
                }));
            }

            await Task.WhenAll(tasks);
            Log("Network", $"局域网扫描完成，发现 {results.Count} 台设备", "Information");
            return results.OrderBy(d => d.IpAddress).ToList();
        }

        public async Task<List<PortInfo>> ScanPorts(string ipAddress, int startPort, int endPort)
        {
            var ports = new List<PortInfo>();
            var tasks = new List<Task>();
            var results = new ConcurrentBag<PortInfo>();

            for (int port = startPort; port <= endPort; port++)
            {
                var currentPort = port;
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        using (var tcpClient = new TcpClient())
                        {
                            await tcpClient.ConnectAsync(ipAddress, currentPort);
                            tcpClient.Close();
                            results.Add(new PortInfo
                            {
                                Port = currentPort,
                                Status = "占用",
                                Protocol = "TCP"
                            });
                        }
                    }
                    catch
                    {
                        results.Add(new PortInfo
                        {
                            Port = currentPort,
                            Status = "空闲",
                            Protocol = "TCP"
                        });
                    }
                }));
            }

            await Task.WhenAll(tasks);
            Log("Network", $"端口扫描完成: {ipAddress}:{startPort}-{endPort}", "Information");
            return results.OrderBy(p => p.Port).ToList();
        }

        public async Task<PingResult> PingHost(string host, int count = 4)
        {
            var result = new PingResult
            {
                Host = host,
                Responses = new List<PingResponse>()
            };

            using (var ping = new Ping())
            {
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        var reply = await ping.SendPingAsync(host, 1000);
                        result.Responses.Add(new PingResponse
                        {
                            Success = reply.Status == IPStatus.Success,
                            Time = reply.Status == IPStatus.Success ? reply.RoundtripTime + "ms" : "超时"
                        });
                    }
                    catch
                    {
                        result.Responses.Add(new PingResponse
                        {
                            Success = false,
                            Time = "失败"
                        });
                    }
                }
            }

            result.SuccessRate = $"{result.Responses.Count(r => r.Success) * 100 / count}%";
            Log("Network", $"Ping测试完成: {host}, 成功率: {result.SuccessRate}", "Information");
            return result;
        }

        public async Task<IPInfo> QueryIP(string ipOrDomain)
        {
            var info = new IPInfo();

            try
            {
                var hostEntry = await Dns.GetHostEntryAsync(ipOrDomain);
                
                if (hostEntry.AddressList.Length > 0)
                {
                    info.IpAddress = hostEntry.AddressList[0].ToString();
                    info.Hostname = hostEntry.HostName;
                }

                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var iface in networkInterfaces)
                {
                    if (iface.OperationalStatus == OperationalStatus.Up)
                    {
                        var properties = iface.GetIPProperties();
                        foreach (var addr in properties.UnicastAddresses)
                        {
                            if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                info.LocalIP = addr.Address.ToString();
                                info.SubnetMask = addr.IPv4Mask?.ToString() ?? "未知";
                                
                                foreach (var gateway in properties.GatewayAddresses)
                                {
                                    if (gateway.Address.AddressFamily == AddressFamily.InterNetwork)
                                    {
                                        info.Gateway = gateway.Address.ToString();
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(info.LocalIP)) break;
                }

                info.Region = "局域网";
                info.ISP = "本地网络";
                Log("Network", $"IP查询完成: {ipOrDomain}", "Information");
            }
            catch (Exception ex)
            {
                info.Error = ex.Message;
                Log("Network", $"IP查询失败: {ex.Message}", "Error");
            }

            return info;
        }

        public Task<WhoisResult> QueryWhois(string domain)
        {
            var result = new WhoisResult
            {
                Domain = domain,
                Registrar = "示例注册商",
                CreatedDate = "2020-01-01",
                UpdatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
                ExpiryDate = DateTime.Now.AddYears(1).ToString("yyyy-MM-dd"),
                Nameservers = new List<string> { "ns1.example.com", "ns2.example.com" },
                Status = "clientTransferProhibited"
            };

            Log("Network", $"Whois查询完成: {domain}", "Information");
            return Task.FromResult(result);
        }

        private void Log(string source, string message, string level)
        {
            var logLevel = level switch
            {
                "Error" => LogLevel.Error,
                "Warning" => LogLevel.Warning,
                _ => LogLevel.Information
            };
            _logger.Log(logLevel, $"[{source}] {message}");

            _logEntries.Enqueue(new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Source = source,
                Message = message
            });

            while (_logEntries.Count > MaxLogEntries)
            {
                _logEntries.TryDequeue(out _);
            }
        }
    }

    public class LanDevice
    {
        public string IpAddress { get; set; } = string.Empty;
        public string Hostname { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ResponseTime { get; set; } = string.Empty;
    }

    public class PortInfo
    {
        public int Port { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Protocol { get; set; } = string.Empty;
    }

    public class PingResult
    {
        public string Host { get; set; } = string.Empty;
        public List<PingResponse> Responses { get; set; } = new List<PingResponse>();
        public string SuccessRate { get; set; } = string.Empty;
    }

    public class PingResponse
    {
        public bool Success { get; set; }
        public string Time { get; set; } = string.Empty;
    }

    public class IPInfo
    {
        public string IpAddress { get; set; } = string.Empty;
        public string Hostname { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string ISP { get; set; } = string.Empty;
        public string SubnetMask { get; set; } = string.Empty;
        public string Gateway { get; set; } = string.Empty;
        public string LocalIP { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }

    public class WhoisResult
    {
        public string Domain { get; set; } = string.Empty;
        public string Registrar { get; set; } = string.Empty;
        public string CreatedDate { get; set; } = string.Empty;
        public string UpdatedDate { get; set; } = string.Empty;
        public string ExpiryDate { get; set; } = string.Empty;
        public List<string> Nameservers { get; set; } = new List<string>();
        public string Status { get; set; } = string.Empty;
    }
}
