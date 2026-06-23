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
        private readonly ConcurrentDictionary<string, TcpClient> _tcpClients = new ConcurrentDictionary<string, TcpClient>();
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
                _tcpClients.TryAdd(key, tcpClient);

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

            if (_tcpClients.TryRemove(key, out var tcpClient))
            {
                try
                {
                    tcpClient.Close();
                    tcpClient.Dispose();
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
                Log(LogLevel.Information, $"PLC TCP发送: {ipAddress}:{port}, 数据: {hexData}");

                stream.ReadTimeout = 1000;

                if (stream.DataAvailable)
                {
                    var response = new byte[1024];
                    var bytesRead = stream.Read(response, 0, response.Length);

                    if (bytesRead > 0)
                    {
                        var responseData = new byte[bytesRead];
                        Array.Copy(response, responseData, bytesRead);
                        var hexResponse = BitConverter.ToString(responseData).Replace("-", " ");
                        Log(LogLevel.Information, $"PLC TCP接收: {ipAddress}:{port}, 数据: {hexResponse}");
                        return ("发送成功", responseData);
                    }
                }

                return ("发送成功", Array.Empty<byte>());
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, $"PLC TCP发送失败: {ipAddress}:{port}, 错误: {ex.Message}");
                return ($"发送失败: {ex.Message}", Array.Empty<byte>());
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

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}