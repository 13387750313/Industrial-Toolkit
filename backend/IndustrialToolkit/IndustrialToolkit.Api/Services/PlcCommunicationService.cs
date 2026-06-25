using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Net.Sockets;

namespace IndustrialToolkit.Services
{
    public class PlcCommunicationService
    {
        private readonly ILogger<PlcCommunicationService> _logger;
        private readonly ConcurrentDictionary<string, SerialPort> _serialPorts = new ConcurrentDictionary<string, SerialPort>();
        private readonly ConcurrentDictionary<string, TcpConnectionInfo> _tcpClients = new ConcurrentDictionary<string, TcpConnectionInfo>();
        private readonly ConcurrentQueue<LogEntry> _logEntries = new ConcurrentQueue<LogEntry>();
        private const int MaxLogEntries = 1000;

        public PlcCommunicationService(ILogger<PlcCommunicationService> logger)
        {
            _logger = logger;
        }

        public string ConnectSerial(string portName, int baudRate, int dataBits, Parity parity, StopBits stopBits)
        {
            var key = $"serial_{portName}";

            if (_serialPorts.ContainsKey(key))
            {
                return "端口已连接";
            }

            try
            {
                var serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits)
                {
                    ReadTimeout = 5000,
                    WriteTimeout = 5000,
                    DtrEnable = true,
                    RtsEnable = true
                };

                serialPort.Open();
                _serialPorts.TryAdd(key, serialPort);

                Log(LogLevel.Information, $"PLC串口连接成功: {portName}, 波特率: {baudRate}");
                return "连接成功";
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, $"PLC串口连接失败: {portName}, 错误: {ex.Message}");
                return $"连接失败: {ex.Message}";
            }
        }

        public string DisconnectSerial(string portName)
        {
            var key = $"serial_{portName}";

            if (_serialPorts.TryRemove(key, out var serialPort))
            {
                try
                {
                    serialPort.Close();
                    serialPort.Dispose();
                    Log(LogLevel.Information, $"PLC串口断开连接: {portName}");
                    return "断开成功";
                }
                catch (Exception ex)
                {
                    Log(LogLevel.Error, $"PLC串口断开失败: {portName}, 错误: {ex.Message}");
                    return $"断开失败: {ex.Message}";
                }
            }

            return "未找到连接";
        }

        public (string, byte[]) SendSerialData(string portName, byte[] data)
        {
            var key = $"serial_{portName}";

            if (!_serialPorts.TryGetValue(key, out var serialPort))
            {
                return ("未连接", Array.Empty<byte>());
            }

            try
            {
                serialPort.Write(data, 0, data.Length);

                var hexData = BitConverter.ToString(data).Replace("-", " ");
                Log(LogLevel.Information, $"PLC串口发送: {portName}, 数据: {hexData}");

                var response = new List<byte>();
                var startTime = DateTime.Now;

                while (serialPort.BytesToRead > 0 && (DateTime.Now - startTime).TotalMilliseconds < 5000)
                {
                    response.Add((byte)serialPort.ReadByte());
                    Thread.Sleep(10);
                }

                if (response.Count > 0)
                {
                    var hexResponse = BitConverter.ToString(response.ToArray()).Replace("-", " ");
                    Log(LogLevel.Information, $"PLC串口接收: {portName}, 数据: {hexResponse}");
                }

                return ("发送成功", response.ToArray());
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, $"PLC串口发送失败: {portName}, 错误: {ex.Message}");
                return ($"发送失败: {ex.Message}", Array.Empty<byte>());
            }
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

                var connInfo = new TcpConnectionInfo
                {
                    Client = tcpClient,
                    IpAddress = ipAddress,
                    Port = port,
                    IsConnected = true,
                    ConnectTime = DateTime.Now
                };

                _tcpClients.TryAdd(key, connInfo);

                var receiveThread = new Thread(() => ReceiveLoop(key))
                {
                    IsBackground = true,
                    Name = $"TCP_Receive_{ipAddress}:{port}"
                };
                receiveThread.Start();

                Log(LogLevel.Information, $"PLC TCP连接成功: {ipAddress}:{port}");
                return "连接成功";
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, $"PLC TCP连接失败: {ipAddress}:{port}, 错误: {ex.Message}");
                return $"连接失败: {ex.Message}";
            }
        }

        public string DisconnectTcp(string ipAddress, int port)
        {
            var key = $"tcp_{ipAddress}:{port}";

            if (_tcpClients.TryRemove(key, out var connInfo))
            {
                try
                {
                    connInfo.IsConnected = false;
                    connInfo.Client.Close();
                    connInfo.Client.Dispose();
                    Log(LogLevel.Information, $"PLC TCP断开连接: {ipAddress}:{port}");
                    return "断开成功";
                }
                catch (Exception ex)
                {
                    Log(LogLevel.Error, $"PLC TCP断开失败: {ipAddress}:{port}, 错误: {ex.Message}");
                    return $"断开失败: {ex.Message}";
                }
            }

            return "未找到连接";
        }

        private void ReceiveLoop(string key)
        {
            if (!_tcpClients.TryGetValue(key, out var connInfo))
                return;

            var ipEndPoint = $"{connInfo.IpAddress}:{connInfo.Port}";
            Log(LogLevel.Debug, $"TCP接收线程启动: {ipEndPoint}");

            try
            {
                var stream = connInfo.Client.GetStream();
                var buffer = new byte[4096];

                while (connInfo.IsConnected)
                {
                    try
                    {
                        if (stream.DataAvailable)
                        {
                            var bytesRead = stream.Read(buffer, 0, buffer.Length);
                            if (bytesRead > 0)
                            {
                                var data = new byte[bytesRead];
                                Array.Copy(buffer, data, bytesRead);

                                connInfo.ReceivedData.Enqueue(data);

                                var hexData = BitConverter.ToString(data).Replace("-", " ");
                                Log(LogLevel.Information, $"PLC TCP接收: {ipEndPoint}, 数据: {hexData}");
                            }
                        }
                        else
                        {
                            Thread.Sleep(10);
                        }
                    }
                    catch (IOException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Log(LogLevel.Error, $"TCP接收异常: {ipEndPoint}, 错误: {ex.Message}");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, $"TCP接收线程异常: {ipEndPoint}, 错误: {ex.Message}");
            }
            finally
            {
                connInfo.IsConnected = false;
                Log(LogLevel.Information, $"TCP连接已断开: {ipEndPoint}");
            }
        }

        public (string, byte[]) SendTcpData(string ipAddress, int port, byte[] data)
        {
            var key = $"tcp_{ipAddress}:{port}";

            if (!_tcpClients.TryGetValue(key, out var connInfo) || !connInfo.IsConnected)
            {
                return ("未连接", Array.Empty<byte>());
            }

            try
            {
                var stream = connInfo.Client.GetStream();
                stream.Write(data, 0, data.Length);

                var hexData = BitConverter.ToString(data).Replace("-", " ");
                Log(LogLevel.Information, $"PLC TCP发送: {ipAddress}:{port}, 数据: {hexData}");

                return ("发送成功", Array.Empty<byte>());
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, $"PLC TCP发送失败: {ipAddress}:{port}, 错误: {ex.Message}");
                connInfo.IsConnected = false;
                return ($"发送失败: {ex.Message}", Array.Empty<byte>());
            }
        }

        public List<byte[]> GetReceivedData(string ipAddress, int port)
        {
            var key = $"tcp_{ipAddress}:{port}";
            if (!_tcpClients.TryGetValue(key, out var connInfo))
                return new List<byte[]>();

            var result = new List<byte[]>();
            while (connInfo.ReceivedData.TryDequeue(out var data))
            {
                result.Add(data);
            }
            return result;
        }

        public bool IsTcpConnected(string ipAddress, int port)
        {
            var key = $"tcp_{ipAddress}:{port}";
            if (!_tcpClients.TryGetValue(key, out var connInfo))
                return false;

            if (!connInfo.IsConnected)
                return false;

            try
            {
                var client = connInfo.Client;
                if (client.Client == null)
                {
                    connInfo.IsConnected = false;
                    return false;
                }

                if (client.Client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] buff = new byte[1];
                    if (client.Client.Receive(buff, SocketFlags.Peek) == 0)
                    {
                        connInfo.IsConnected = false;
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                connInfo.IsConnected = false;
                return false;
            }
        }

        public List<string> GetAvailablePorts()
        {
            return SerialPort.GetPortNames().ToList();
        }

        public bool IsConnected(string connectionId)
        {
            return _serialPorts.ContainsKey($"serial_{connectionId}") || _tcpClients.ContainsKey($"tcp_{connectionId}");
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
                Source = "PLC",
                Message = message
            });

            while (_logEntries.Count > MaxLogEntries)
            {
                _logEntries.TryDequeue(out _);
            }
        }
    }

    public class TcpConnectionInfo
    {
        public TcpClient Client { get; set; } = new TcpClient();
        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; }
        public bool IsConnected { get; set; }
        public DateTime ConnectTime { get; set; }
        public ConcurrentQueue<byte[]> ReceivedData { get; set; } = new ConcurrentQueue<byte[]>();
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
