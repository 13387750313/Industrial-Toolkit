using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace IndustrialToolkit.Services
{
    public class CanCommunicationService
    {
        private readonly ILogger<CanCommunicationService> _logger;
        private readonly ConcurrentDictionary<string, CanChannel> _canChannels = new ConcurrentDictionary<string, CanChannel>();
        private readonly ConcurrentQueue<LogEntry> _logEntries = new ConcurrentQueue<LogEntry>();
        private const int MaxLogEntries = 1000;

        public CanCommunicationService(ILogger<CanCommunicationService> logger)
        {
            _logger = logger;
        }

        public string OpenChannel(string channelName, int baudRate = 250000)
        {
            var key = $"can_{channelName}";

            if (_canChannels.ContainsKey(key))
            {
                return "通道已打开";
            }

            try
            {
                var channel = new CanChannel(channelName, baudRate);
                var result = channel.Open();
                
                if (result == "成功")
                {
                    _canChannels.TryAdd(key, channel);
                    Log(LogLevel.Information, $"CAN通道打开成功: {channelName}, 波特率: {baudRate}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, $"CAN通道打开失败: {channelName}, 错误: {ex.Message}");
                return $"打开失败: {ex.Message}";
            }
        }

        public string CloseChannel(string channelName)
        {
            var key = $"can_{channelName}";

            if (_canChannels.TryRemove(key, out var channel))
            {
                try
                {
                    channel.Close();
                    Log(LogLevel.Information, $"CAN通道关闭: {channelName}");
                    return "关闭成功";
                }
                catch (Exception ex)
                {
                    Log(LogLevel.Error, $"CAN通道关闭失败: {channelName}, 错误: {ex.Message}");
                    return $"关闭失败: {ex.Message}";
                }
            }

            return "未找到通道";
        }

        public (string, CanFrame?) SendCanMessage(string channelName, int id, byte[] data, bool isExtendedId = false)
        {
            var key = $"can_{channelName}";

            if (!_canChannels.TryGetValue(key, out var channel))
            {
                return ("通道未打开", null);
            }

            try
            {
                var frame = new CanFrame
                {
                    Id = id,
                    IsExtendedId = isExtendedId,
                    Data = data,
                    Length = (byte)data.Length
                };

                var result = channel.SendFrame(frame);
                
                if (result == "成功")
                {
                    Log(LogLevel.Information, $"CAN发送: {channelName}, ID: 0x{id:X}, 数据: {BitConverter.ToString(data).Replace("-", " ")}");
                }

                return (result, frame);
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, $"CAN发送失败: {channelName}, ID: 0x{id:X}, 错误: {ex.Message}");
                return ($"发送失败: {ex.Message}", null);
            }
        }

        public (string, CanFrame?) ReceiveCanMessage(string channelName)
        {
            var key = $"can_{channelName}";

            if (!_canChannels.TryGetValue(key, out var channel))
            {
                return ("通道未打开", null);
            }

            try
            {
                var frame = channel.ReceiveFrame();
                
                if (frame != null)
                {
                    Log(LogLevel.Information, $"CAN接收: {channelName}, ID: 0x{frame.Id:X}, 数据: {BitConverter.ToString(frame.Data).Replace("-", " ")}");
                    return ("接收成功", frame);
                }

                return ("无数据", null);
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, $"CAN接收失败: {channelName}, 错误: {ex.Message}");
                return ($"接收失败: {ex.Message}", null);
            }
        }

        public List<string> GetAvailableChannels()
        {
            var channels = new List<string>();
            
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    var canInterfaces = System.IO.Directory.GetFiles("/sys/class/net/", "can*");
                    foreach (var iface in canInterfaces)
                    {
                        channels.Add(System.IO.Path.GetFileName(iface));
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    channels.AddRange(new[] { "PCAN_USBBUS1", "PCAN_USBBUS2", "PCAN_PCI1", "VirtualCAN1" });
                }
            }
            catch { }

            return channels;
        }

        public bool IsChannelOpen(string channelName)
        {
            return _canChannels.ContainsKey($"can_{channelName}");
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
                Source = "CAN",
                Message = message
            });

            while (_logEntries.Count > MaxLogEntries)
            {
                _logEntries.TryDequeue(out _);
            }
        }
    }

    public class CanChannel
    {
        public string Name { get; }
        public int BaudRate { get; }
        private Socket? _socket;
        private bool _isOpen;

        public CanChannel(string name, int baudRate)
        {
            Name = name;
            BaudRate = baudRate;
        }

        public string Open()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    _socket = new Socket(AddressFamily.Unix, SocketType.Raw, ProtocolType.IP);
                    _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);
                    _isOpen = true;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    _isOpen = true;
                }
                
                return "成功";
            }
            catch (Exception ex)
            {
                return $"失败: {ex.Message}";
            }
        }

        public void Close()
        {
            _socket?.Close();
            _socket?.Dispose();
            _isOpen = false;
        }

        public string SendFrame(CanFrame frame)
        {
            try
            {
                if (!_isOpen) return "通道未打开";
                
                if (_socket != null)
                {
                    var data = frame.ToBytes();
                    _socket.Send(data);
                }
                
                return "成功";
            }
            catch (Exception ex)
            {
                return $"失败: {ex.Message}";
            }
        }

        public CanFrame? ReceiveFrame()
        {
            try
            {
                if (!_isOpen || _socket == null) return null;

                var buffer = new byte[16];
                var bytesRead = _socket.Receive(buffer);
                
                if (bytesRead > 0)
                {
                    return CanFrame.FromBytes(buffer);
                }

                return null;
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
            {
                return null;
            }
            catch
            {
                return null;
            }
        }
    }

    public class CanFrame
    {
        public int Id { get; set; }
        public bool IsExtendedId { get; set; }
        public byte Length { get; set; }
        public byte[] Data { get; set; } = Array.Empty<byte>();

        public byte[] ToBytes()
        {
            var result = new List<byte>();
            
            if (IsExtendedId)
            {
                result.AddRange(BitConverter.GetBytes((uint)Id).Reverse());
            }
            else
            {
                var idBytes = BitConverter.GetBytes((ushort)Id).Reverse().ToArray();
                result.AddRange(idBytes);
            }
            
            result.Add(Length);
            result.AddRange(Data.Take(8));
            
            return result.ToArray();
        }

        public static CanFrame? FromBytes(byte[] data)
        {
            if (data.Length < 5) return null;

            return new CanFrame
            {
                Id = BitConverter.ToInt32(data.Take(4).Reverse().ToArray(), 0),
                IsExtendedId = (data[3] & 0x80) != 0,
                Length = data[4],
                Data = data.Skip(5).Take(data[4]).ToArray()
            };
        }
    }
}
