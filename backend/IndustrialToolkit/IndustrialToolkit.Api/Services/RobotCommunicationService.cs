using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace IndustrialToolkit.Services
{
    public class RobotCommunicationService
    {
        private readonly ILogger<RobotCommunicationService> _logger;
        private readonly ConcurrentDictionary<string, TcpClient> _tcpClients = new ConcurrentDictionary<string, TcpClient>();
        private readonly ConcurrentDictionary<string, UdpClient> _udpClients = new ConcurrentDictionary<string, UdpClient>();
        private readonly ConcurrentQueue<LogEntry> _logEntries = new ConcurrentQueue<LogEntry>();
        private const int MaxLogEntries = 1000;

        public RobotCommunicationService(ILogger<RobotCommunicationService> logger)
        {
            _logger = logger;
        }

        public string ConnectTcp(string ipAddress, int port)
        {
            var key = $"tcp_{ipAddress}:{port}";

            if (_tcpClients.ContainsKey(key))
            {
                return "连接已存在";
            }

            try
            {
                var tcpClient = new TcpClient();
                tcpClient.Connect(ipAddress, port);
                _tcpClients.TryAdd(key, tcpClient);

                Log(LogLevel.Information, $"机器人TCP连接成功: {ipAddress}:{port}");
                return "连接成功";
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, $"机器人TCP连接失败: {ipAddress}:{port}, 错误: {ex.Message}");
                return $"连接失败: {ex.Message}";
            }
        }

        public string DisconnectTcp(string ipAddress, int port)
        {
            var key = $"tcp_{ipAddress}:{port}";

            if (_tcpClients.TryRemove(key, out var tcpClient))
            {
                try
                {
                    tcpClient.Close();
                    tcpClient.Dispose();
                    Log(LogLevel.Information, $"机器人TCP断开连接: {ipAddress}:{port}");
                    return "断开成功";
                }
                catch (Exception ex)
                {
                    Log(LogLevel.Error, $"机器人TCP断开失败: {ipAddress}:{port}, 错误: {ex.Message}");
                    return $"断开失败: {ex.Message}";
                }
            }

            return "未找到连接";
        }

        public (string, byte[]) SendTcpData(string ipAddress, int port, byte[] data)
        {
            var key = $"tcp_{ipAddress}:{port}";

            if (!_tcpClients.TryGetValue(key, out var tcpClient))
            {
                return ("未连接", Array.Empty<byte>());
            }

            try
            {
                var stream = tcpClient.GetStream();
                stream.Write(data, 0, data.Length);

                var hexData = BitConverter.ToString(data).Replace("-", " ");
                Log(LogLevel.Information, $"机器人TCP发送: {ipAddress}:{port}, 数据: {hexData}");

                stream.ReadTimeout = 1000;

                if (stream.DataAvailable)
                {
                    var response = new byte[4096];
                    var bytesRead = stream.Read(response, 0, response.Length);

                    if (bytesRead > 0)
                    {
                        var responseData = new byte[bytesRead];
                        Array.Copy(response, responseData, bytesRead);
                        var hexResponse = BitConverter.ToString(responseData).Replace("-", " ");
                        Log(LogLevel.Information, $"机器人TCP接收: {ipAddress}:{port}, 数据: {hexResponse}");
                        return ("发送成功", responseData);
                    }
                }

                return ("发送成功", Array.Empty<byte>());
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, $"机器人TCP发送失败: {ipAddress}:{port}, 错误: {ex.Message}");
                return ($"发送失败: {ex.Message}", Array.Empty<byte>());
            }
        }

        public string ConnectUdp(int localPort)
        {
            var key = $"udp_{localPort}";

            if (_udpClients.ContainsKey(key))
            {
                return "UDP端口已绑定";
            }

            try
            {
                var udpClient = new UdpClient(localPort);
                _udpClients.TryAdd(key, udpClient);

                Log(LogLevel.Information, $"机器人UDP绑定成功: 端口 {localPort}");
                return "绑定成功";
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, $"机器人UDP绑定失败: 端口 {localPort}, 错误: {ex.Message}");
                return $"绑定失败: {ex.Message}";
            }
        }

        public string DisconnectUdp(int localPort)
        {
            var key = $"udp_{localPort}";

            if (_udpClients.TryRemove(key, out var udpClient))
            {
                try
                {
                    udpClient.Close();
                    udpClient.Dispose();
                    Log(LogLevel.Information, $"机器人UDP断开: 端口 {localPort}");
                    return "断开成功";
                }
                catch (Exception ex)
                {
                    Log(LogLevel.Error, $"机器人UDP断开失败: 端口 {localPort}, 错误: {ex.Message}");
                    return $"断开失败: {ex.Message}";
                }
            }

            return "未找到连接";
        }

        public (string, byte[]) SendUdpData(int localPort, string remoteIp, int remotePort, byte[] data)
        {
            var key = $"udp_{localPort}";

            if (!_udpClients.TryGetValue(key, out var udpClient))
            {
                return ("未绑定", Array.Empty<byte>());
            }

            try
            {
                var remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIp), remotePort);
                udpClient.Send(data, data.Length, remoteEndPoint);

                var hexData = BitConverter.ToString(data).Replace("-", " ");
                Log(LogLevel.Information, $"机器人UDP发送: {remoteIp}:{remotePort}, 数据: {hexData}");

                var receiveEndPoint = new IPEndPoint(IPAddress.Any, 0);
                var response = udpClient.Receive(ref receiveEndPoint);

                if (response != null && response.Length > 0)
                {
                    var hexResponse = BitConverter.ToString(response).Replace("-", " ");
                    Log(LogLevel.Information, $"机器人UDP接收: {receiveEndPoint.Address}:{receiveEndPoint.Port}, 数据: {hexResponse}");
                    return ("发送成功", response);
                }

                return ("发送成功", Array.Empty<byte>());
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, $"机器人UDP发送失败: {remoteIp}:{remotePort}, 错误: {ex.Message}");
                return ($"发送失败: {ex.Message}", Array.Empty<byte>());
            }
        }

        public (string, byte[]) SendFanucCommand(string ipAddress, int port, string command)
        {
            var key = $"tcp_{ipAddress}:{port}";

            if (!_tcpClients.TryGetValue(key, out var tcpClient))
            {
                return ("未连接", Array.Empty<byte>());
            }

            try
            {
                var data = System.Text.Encoding.ASCII.GetBytes(command + "\r\n");
                var stream = tcpClient.GetStream();
                stream.Write(data, 0, data.Length);

                Log(LogLevel.Information, $"FANUC命令发送: {ipAddress}:{port}, 命令: {command}");

                var response = new byte[4096];
                var bytesRead = stream.Read(response, 0, response.Length);

                if (bytesRead > 0)
                {
                    var responseData = new byte[bytesRead];
                    Array.Copy(response, responseData, bytesRead);
                    Log(LogLevel.Information, $"FANUC响应接收: {ipAddress}:{port}, 长度: {bytesRead}");
                    return ("发送成功", responseData);
                }

                return ("发送成功", Array.Empty<byte>());
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, $"FANUC命令发送失败: {ipAddress}:{port}, 错误: {ex.Message}");
                return ($"发送失败: {ex.Message}", Array.Empty<byte>());
            }
        }

        public bool IsConnected(string connectionId)
        {
            return _tcpClients.ContainsKey(connectionId) || _udpClients.ContainsKey(connectionId);
        }

        public List<LogEntry> GetLogs()
        {
            return _logEntries.ToList();
        }

        private void Log(LogLevel level, string message)
        {
            _logger.Log(level, message);

            _logEntries.Enqueue(new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level.ToString(),
                Source = "Robot",
                Message = message
            });

            while (_logEntries.Count > MaxLogEntries)
            {
                _logEntries.TryDequeue(out _);
            }
        }
    }
}