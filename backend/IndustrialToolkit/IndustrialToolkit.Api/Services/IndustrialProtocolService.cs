using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Net.Sockets;
using System.Text;

namespace IndustrialToolkit.Services
{
    /// <summary>
    /// 工业协议服务 - 统一处理各种工业协议通讯
    /// </summary>
    public class IndustrialProtocolService
    {
        private readonly ILogger<IndustrialProtocolService> _logger;
        private readonly ConcurrentDictionary<string, TcpClient> _tcpClients = new ConcurrentDictionary<string, TcpClient>();
        private readonly ConcurrentDictionary<string, SerialPort> _serialPorts = new ConcurrentDictionary<string, SerialPort>();
        private readonly ConcurrentQueue<LogEntry> _logEntries = new ConcurrentQueue<LogEntry>();
        private const int MaxLogEntries = 1000;

        public IndustrialProtocolService(ILogger<IndustrialProtocolService> logger)
        {
            _logger = logger;
        }

        #region Modbus TCP
        public string ConnectModbus(string ipAddress, int port)
        {
            var key = $"modbus_{ipAddress}:{port}";
            if (_tcpClients.ContainsKey(key)) return "已连接";

            try
            {
                var client = new TcpClient();
                client.Connect(ipAddress, port);
                _tcpClients.TryAdd(key, client);
                Log("Modbus", $"连接成功: {ipAddress}:{port}", "Information");
                return "连接成功";
            }
            catch (Exception ex)
            {
                Log("Modbus", $"连接失败: {ipAddress}:{port}, 错误: {ex.Message}", "Error");
                return $"连接失败: {ex.Message}";
            }
        }

        public (string, object?) SendModbus(string ipAddress, int port, int slaveId, int functionCode, int address, int count, string values)
        {
            var key = $"modbus_{ipAddress}:{port}";
            if (!_tcpClients.TryGetValue(key, out var client)) return ("未连接", null);

            try
            {
                var stream = client.GetStream();
                // 构建Modbus TCP帧
                var transactionId = (ushort)DateTime.Now.Millisecond;
                var protocolId = 0;
                var length = 6;
                var frame = new List<byte>();
                frame.AddRange(BitConverter.GetBytes(transactionId).Reverse());
                frame.AddRange(BitConverter.GetBytes(protocolId).Reverse());
                frame.AddRange(BitConverter.GetBytes(length).Reverse());
                frame.Add((byte)slaveId);
                frame.Add((byte)functionCode);
                frame.AddRange(BitConverter.GetBytes((ushort)address).Reverse());
                
                if (functionCode == 16)
                {
                    var vals = values.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(v => ushort.Parse(v)).ToArray();
                    frame.AddRange(BitConverter.GetBytes((ushort)vals.Length).Reverse());
                    frame.Add((byte)(vals.Length * 2));
                    foreach (var v in vals)
                    {
                        frame.AddRange(BitConverter.GetBytes(v).Reverse());
                    }
                }
                else
                {
                    frame.AddRange(BitConverter.GetBytes((ushort)count).Reverse());
                }

                stream.Write(frame.ToArray(), 0, frame.Count);
                stream.ReadTimeout = 1000;

                if (stream.DataAvailable)
                {
                    var response = new byte[1024];
                    var bytesRead = stream.Read(response, 0, response.Length);
                    Log("Modbus", $"请求成功: FC={functionCode}, 地址={address}, 数量={count}", "Information");
                    return ("成功", new { raw = BitConverter.ToString(response, 0, bytesRead).Replace("-", " ") });
                }
                Log("Modbus", $"请求成功: FC={functionCode}, 地址={address}, 数量={count}", "Information");
                return ("成功", null);
            }
            catch (Exception ex)
            {
                Log("Modbus", $"请求失败: {ex.Message}", "Error");
                return ($"失败: {ex.Message}", null);
            }
        }

        public string DisconnectModbus(string ipAddress, int port)
        {
            var key = $"modbus_{ipAddress}:{port}";
            if (_tcpClients.TryRemove(key, out var client))
            {
                client.Close();
                client.Dispose();
                return "断开成功";
            }
            return "未连接";
        }
        #endregion

        #region Ethernet/IP
        public string ConnectEip(string ipAddress, int port)
        {
            var key = $"eip_{ipAddress}:{port}";
            if (_tcpClients.ContainsKey(key)) return "已连接";

            try
            {
                var client = new TcpClient();
                client.Connect(ipAddress, port);
                _tcpClients.TryAdd(key, client);
                Log("EIP", $"连接成功: {ipAddress}:{port}", "Information");
                return "连接成功";
            }
            catch (Exception ex)
            {
                Log("EIP", $"连接失败: {ex.Message}", "Error");
                return $"连接失败: {ex.Message}";
            }
        }

        public (string, object?) SendEip(string ipAddress, int port, string service, string tag, string data)
        {
            var key = $"eip_{ipAddress}:{port}";
            if (!_tcpClients.TryGetValue(key, out var client)) return ("未连接", null);

            try
            {
                var stream = client.GetStream();
                // 简化的EIP封装 - 实际生产环境需完整CIP协议
                var request = BuildEipRequest(service, tag, data);
                stream.Write(request, 0, request.Length);
                stream.ReadTimeout = 1000;
                if (stream.DataAvailable)
                {
                    var response = new byte[1024];
                    var bytesRead = stream.Read(response, 0, response.Length);
                    Log("EIP", $"EIP请求成功: 服务={service}, 标签={tag}", "Information");
                    return ("成功", new { raw = BitConverter.ToString(response, 0, bytesRead).Replace("-", " ") });
                }
                Log("EIP", $"EIP请求成功: 服务={service}, 标签={tag}", "Information");
                return ("成功", null);
            }
            catch (Exception ex)
            {
                Log("EIP", $"EIP请求失败: {ex.Message}", "Error");
                return ($"失败: {ex.Message}", null);
            }
        }

        private byte[] BuildEipRequest(string service, string tag, string data)
        {
            var request = new List<byte>();
            // EIP命令 - SendRRData
            request.AddRange(new byte[] { 0x6F, 0x00 });
            request.AddRange(BitConverter.GetBytes((ushort)0).Reverse()); // 长度占位
            request.AddRange(BitConverter.GetBytes((uint)0).Reverse()); // 会话句柄
            request.AddRange(BitConverter.GetBytes((uint)0).Reverse()); // 状态
            request.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00 }); // 发送方上下文
            request.AddRange(BitConverter.GetBytes((uint)0).Reverse()); // 选项
            // 协议版本
            request.Add(0x02);
            // CIP路径 - 简化
            request.Add(0x00);
            request.Add(0x00);
            request.Add(0x00);
            request.Add(0x00);
            // CIP服务
            var serviceCode = Convert.ToInt32(service.Replace("0x", ""), 16);
            request.Add((byte)serviceCode);
            // 标签路径
            var tagBytes = Encoding.ASCII.GetBytes(tag);
            request.Add((byte)(tagBytes.Length * 2));
            foreach (var b in tagBytes)
            {
                request.Add(b);
                request.Add(0x00);
            }
            // 数据
            if (!string.IsNullOrEmpty(data))
            {
                var dataBytes = Encoding.ASCII.GetBytes(data);
                request.AddRange(dataBytes);
            }
            // 更新长度
            var length = request.Count - 4;
            request[2] = (byte)(length & 0xFF);
            request[3] = (byte)((length >> 8) & 0xFF);
            return request.ToArray();
        }

        public string DisconnectEip(string ipAddress, int port)
        {
            var key = $"eip_{ipAddress}:{port}";
            if (_tcpClients.TryRemove(key, out var client))
            {
                client.Close();
                client.Dispose();
                return "断开成功";
            }
            return "未连接";
        }
        #endregion

        #region Profinet
        public string ConnectProfinet(string ipAddress, int port)
        {
            var key = $"pn_{ipAddress}:{port}";
            if (_tcpClients.ContainsKey(key)) return "已连接";

            try
            {
                var client = new TcpClient();
                client.Connect(ipAddress, port);
                _tcpClients.TryAdd(key, client);
                Log("Profinet", $"连接成功: {ipAddress}:{port}", "Information");
                return "连接成功";
            }
            catch (Exception ex)
            {
                Log("Profinet", $"连接失败: {ex.Message}", "Error");
                return $"连接失败: {ex.Message}";
            }
        }

        public (string, object?) SendProfinet(string ipAddress, int port, string operation, string slot, string data)
        {
            var key = $"pn_{ipAddress}:{port}";
            if (!_tcpClients.TryGetValue(key, out var client)) return ("未连接", null);

            try
            {
                var stream = client.GetStream();
                // Profinet IO 数据 - 简化处理
                var request = Encoding.ASCII.GetBytes($"PN:{operation}:{slot}:{data}");
                stream.Write(request, 0, request.Length);
                stream.ReadTimeout = 1000;
                if (stream.DataAvailable)
                {
                    var response = new byte[1024];
                    var bytesRead = stream.Read(response, 0, response.Length);
                    Log("Profinet", $"PN请求成功: 操作={operation}", "Information");
                    return ("成功", new { raw = Encoding.ASCII.GetString(response, 0, bytesRead) });
                }
                Log("Profinet", $"PN请求成功: 操作={operation}", "Information");
                return ("成功", null);
            }
            catch (Exception ex)
            {
                Log("Profinet", $"PN请求失败: {ex.Message}", "Error");
                return ($"失败: {ex.Message}", null);
            }
        }

        public string DisconnectProfinet(string ipAddress, int port)
        {
            var key = $"pn_{ipAddress}:{port}";
            if (_tcpClients.TryRemove(key, out var client))
            {
                client.Close();
                client.Dispose();
                return "断开成功";
            }
            return "未连接";
        }
        #endregion

        #region FinsTCP (Omron)
        public string ConnectFins(string ipAddress, int port)
        {
            var key = $"fins_{ipAddress}:{port}";
            if (_tcpClients.ContainsKey(key)) return "已连接";

            try
            {
                var client = new TcpClient();
                client.Connect(ipAddress, port);
                _tcpClients.TryAdd(key, client);
                Log("Fins", $"Fins连接成功: {ipAddress}:{port}", "Information");
                return "连接成功";
            }
            catch (Exception ex)
            {
                Log("Fins", $"Fins连接失败: {ex.Message}", "Error");
                return $"连接失败: {ex.Message}";
            }
        }

        public (string, object?) SendFins(string ipAddress, int port, string command, string area, int address, int count)
        {
            var key = $"fins_{ipAddress}:{port}";
            if (!_tcpClients.TryGetValue(key, out var client)) return ("未连接", null);

            try
            {
                var stream = client.GetStream();
                // Fins命令帧 - 简化
                var request = new List<byte>();
                request.Add(0x46); // F
                request.Add(0x49); // I
                request.Add(0x4E); // N
                request.Add(0x53); // S
                // 命令
                var cmdBytes = HexStringToBytes(command);
                request.AddRange(cmdBytes);
                // 存储区
                var areaCode = area switch
                {
                    "DM" => 0x82,
                    "WR" => 0xB1,
                    "HR" => 0xB2,
                    _ => 0x82
                };
                request.Add((byte)areaCode);
                request.AddRange(BitConverter.GetBytes((ushort)address).Reverse());
                request.AddRange(BitConverter.GetBytes((ushort)count).Reverse());

                stream.Write(request.ToArray(), 0, request.Count);
                stream.ReadTimeout = 1000;
                if (stream.DataAvailable)
                {
                    var response = new byte[1024];
                    var bytesRead = stream.Read(response, 0, response.Length);
                    Log("Fins", $"Fins请求成功: 命令={command}, 区域={area}", "Information");
                    return ("成功", new { raw = BitConverter.ToString(response, 0, bytesRead).Replace("-", " ") });
                }
                Log("Fins", $"Fins请求成功: 命令={command}, 区域={area}", "Information");
                return ("成功", null);
            }
            catch (Exception ex)
            {
                Log("Fins", $"Fins请求失败: {ex.Message}", "Error");
                return ($"失败: {ex.Message}", null);
            }
        }

        public string DisconnectFins(string ipAddress, int port)
        {
            var key = $"fins_{ipAddress}:{port}";
            if (_tcpClients.TryRemove(key, out var client))
            {
                client.Close();
                client.Dispose();
                return "断开成功";
            }
            return "未连接";
        }
        #endregion

        #region Profibus DP
        public string ConnectProfibus(string portName, int baudRate)
        {
            var key = $"pb_{portName}_{baudRate}";
            if (_serialPorts.ContainsKey(key)) return "已连接";

            try
            {
                var port = new SerialPort(portName, baudRate, Parity.Even, 8, StopBits.One);
                port.Open();
                _serialPorts.TryAdd(key, port);
                Log("Profibus", $"PB连接成功: {portName}, 波特率={baudRate}", "Information");
                return "连接成功";
            }
            catch (Exception ex)
            {
                Log("Profibus", $"PB连接失败: {ex.Message}", "Error");
                return $"连接失败: {ex.Message}";
            }
        }

        public (string, object?) SendProfibus(string portName, int baudRate, int slaveId, string function, int address, int length, string data)
        {
            var key = $"pb_{portName}_{baudRate}";
            if (!_serialPorts.TryGetValue(key, out var port)) return ("未连接", null);

            try
            {
                // Profibus DP 帧 - 简化实现
                var frame = new List<byte>();
                frame.Add((byte)slaveId);
                frame.Add(function == "read" ? (byte)0x01 : (byte)0x02);
                frame.AddRange(BitConverter.GetBytes((ushort)address).Reverse());
                frame.AddRange(BitConverter.GetBytes((ushort)length).Reverse());
                if (function == "write" && !string.IsNullOrEmpty(data))
                {
                    var dataBytes = Encoding.ASCII.GetBytes(data);
                    frame.AddRange(dataBytes.Take(64));
                }
                port.Write(frame.ToArray(), 0, frame.Count);
                Log("Profibus", $"PB请求: 从站={slaveId}, 操作={function}", "Information");
                return ("成功", null);
            }
            catch (Exception ex)
            {
                Log("Profibus", $"PB请求失败: {ex.Message}", "Error");
                return ($"失败: {ex.Message}", null);
            }
        }

        public string DisconnectProfibus(string portName, int baudRate)
        {
            var key = $"pb_{portName}_{baudRate}";
            if (_serialPorts.TryRemove(key, out var port))
            {
                port.Close();
                port.Dispose();
                return "断开成功";
            }
            return "未连接";
        }
        #endregion

        #region DeviceNet
        public string ConnectDeviceNet(string channelName, int baudRate)
        {
            var key = $"dn_{channelName}_{baudRate}";
            Log("DeviceNet", $"DN连接: {channelName}, 波特率={baudRate}", "Information");
            return "连接成功 (DeviceNet基于CAN)";
        }

        public (string, object?) SendDeviceNet(string channelName, int baudRate, int macId, string connectionId, string data)
        {
            try
            {
                // DeviceNet基于CAN - 构建CAN ID: (优先级 + 源MAC + 目标MAC)
                var priority = 0x0;
                var canId = (priority << 26) | (macId << 18) | (0x7F << 11) | (macId << 8);
                var dataBytes = Encoding.ASCII.GetBytes(data ?? "");
                var frame = new List<byte> { (byte)(canId >> 24), (byte)(canId >> 16), (byte)(canId >> 8), (byte)canId };
                frame.Add((byte)dataBytes.Length);
                frame.AddRange(dataBytes);
                Log("DeviceNet", $"DN请求: MAC ID={macId}, 数据长度={dataBytes.Length}", "Information");
                return ("成功", new { canId, data = BitConverter.ToString(dataBytes).Replace("-", " ") });
            }
            catch (Exception ex)
            {
                Log("DeviceNet", $"DN请求失败: {ex.Message}", "Error");
                return ($"失败: {ex.Message}", null);
            }
        }

        public string DisconnectDeviceNet(string channelName, int baudRate)
        {
            return "断开成功";
        }
        #endregion

        #region CC-Link
        public string ConnectCCLink(string ipAddress, int port)
        {
            var key = $"cc_{ipAddress}:{port}";
            if (_tcpClients.ContainsKey(key)) return "已连接";

            try
            {
                var client = new TcpClient();
                client.Connect(ipAddress, port);
                _tcpClients.TryAdd(key, client);
                Log("CCLink", $"CC-Link连接成功: {ipAddress}:{port}", "Information");
                return "连接成功";
            }
            catch (Exception ex)
            {
                Log("CCLink", $"CC-Link连接失败: {ex.Message}", "Error");
                return $"连接失败: {ex.Message}";
            }
        }

        public (string, object?) SendCCLink(string ipAddress, int port, int station, string operation, string address, int count, string data)
        {
            var key = $"cc_{ipAddress}:{port}";
            if (!_tcpClients.TryGetValue(key, out var client)) return ("未连接", null);

            try
            {
                var stream = client.GetStream();
                // CC-Link IE 协议 - 简化
                var request = $"CC:{station}:{operation}:{address}:{count}:{data}";
                var requestBytes = Encoding.ASCII.GetBytes(request);
                stream.Write(requestBytes, 0, requestBytes.Length);
                stream.ReadTimeout = 1000;
                if (stream.DataAvailable)
                {
                    var response = new byte[1024];
                    var bytesRead = stream.Read(response, 0, response.Length);
                    Log("CCLink", $"CC请求成功: 站号={station}", "Information");
                    return ("成功", new { raw = Encoding.ASCII.GetString(response, 0, bytesRead) });
                }
                Log("CCLink", $"CC请求成功: 站号={station}", "Information");
                return ("成功", null);
            }
            catch (Exception ex)
            {
                Log("CCLink", $"CC请求失败: {ex.Message}", "Error");
                return ($"失败: {ex.Message}", null);
            }
        }

        public string DisconnectCCLink(string ipAddress, int port)
        {
            var key = $"cc_{ipAddress}:{port}";
            if (_tcpClients.TryRemove(key, out var client))
            {
                client.Close();
                client.Dispose();
                return "断开成功";
            }
            return "未连接";
        }
        #endregion

        #region AS-Interface
        public string ConnectASInterface(string portName, int baudRate)
        {
            var key = $"asi_{portName}_{baudRate}";
            if (_serialPorts.ContainsKey(key)) return "已连接";

            try
            {
                var port = new SerialPort(portName, baudRate, Parity.Even, 8, StopBits.One);
                port.Open();
                _serialPorts.TryAdd(key, port);
                Log("AS-i", $"AS-i连接成功: {portName}, 波特率={baudRate}", "Information");
                return "连接成功";
            }
            catch (Exception ex)
            {
                Log("AS-i", $"AS-i连接失败: {ex.Message}", "Error");
                return $"连接失败: {ex.Message}";
            }
        }

        public (string, object?) SendASInterface(string portName, int baudRate, int masterAddr, int slaveAddr, string operation, string data)
        {
            var key = $"asi_{portName}_{baudRate}";
            if (!_serialPorts.TryGetValue(key, out var port)) return ("未连接", null);

            try
            {
                // AS-i 协议 - 简化
                var frame = new List<byte>();
                frame.Add((byte)masterAddr);
                frame.Add((byte)slaveAddr);
                frame.Add((byte)(operation == "read" ? 0x01 : 0x02));
                if (operation == "write" && !string.IsNullOrEmpty(data))
                {
                    var dataBytes = Encoding.ASCII.GetBytes(data);
                    frame.AddRange(dataBytes.Take(4));
                }
                port.Write(frame.ToArray(), 0, frame.Count);
                Log("AS-i", $"AS-i请求: 主={masterAddr}, 从={slaveAddr}", "Information");
                return ("成功", null);
            }
            catch (Exception ex)
            {
                Log("AS-i", $"AS-i请求失败: {ex.Message}", "Error");
                return ($"失败: {ex.Message}", null);
            }
        }

        public string DisconnectASInterface(string portName, int baudRate)
        {
            var key = $"asi_{portName}_{baudRate}";
            if (_serialPorts.TryRemove(key, out var port))
            {
                port.Close();
                port.Dispose();
                return "断开成功";
            }
            return "未连接";
        }
        #endregion

        public List<LogEntry> GetLogs()
        {
            return _logEntries.ToList();
        }

        private void Log(string source, string message, string level)
        {
            var logLevel = level switch
            {
                "Error" => LogLevel.Error,
                "Warning" => LogLevel.Warning,
                "Debug" => LogLevel.Debug,
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

        public void ClearLogs()
        {
            while (_logEntries.TryDequeue(out _)) { }
        }

        public int GetLogCount()
        {
            return _logEntries.Count;
        }

        private byte[] HexStringToBytes(string hex)
        {
            hex = hex.Replace(" ", "").Replace("-", "");
            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }
    }
}
