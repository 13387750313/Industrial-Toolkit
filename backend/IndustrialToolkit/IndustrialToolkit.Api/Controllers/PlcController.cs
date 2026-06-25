using IndustrialToolkit.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO.Ports;

namespace IndustrialToolkit.Controllers
{
    [ApiController]
    [Route("api/plc")]
    public class PlcController : ControllerBase
    {
        private readonly PlcCommunicationService _communicationService;

        public PlcController(PlcCommunicationService communicationService)
        {
            _communicationService = communicationService;
        }

        [HttpGet("protocols")]
        public IActionResult GetProtocols()
        {
            var protocols = new[]
            {
                new { name = "Modbus TCP", port = 502, description = "工业标准协议" },
                new { name = "Modbus RTU", port = 0, description = "串口协议" },
                new { name = "Ethernet/IP", port = 44818, description = "罗克韦尔协议" },
                new { name = "Profinet", port = 0, description = "西门子实时以太网" },
                new { name = "FinsTCP", port = 9600, description = "欧姆龙协议" },
                new { name = "S7 Protocol", port = 102, description = "西门子S7协议" },
                new { name = "MC Protocol", port = 5006, description = "三菱协议" },
                new { name = "Omron Fins", port = 9600, description = "欧姆龙Fins" }
            };

            return Ok(protocols);
        }

        [HttpGet("domestic")]
        public IActionResult GetDomesticPlc()
        {
            var brands = new[]
            {
                new { name = "汇川技术", series = "H1U/H2U/H3U/H5U", description = "国产PLC领军品牌" },
                new { name = "信捷电气", series = "XC/XD/XL/XJ", description = "高性价比PLC" },
                new { name = "台达电子", series = "DVP/ES2/EX2/AS3", description = "台达PLC" },
                new { name = "欧姆龙(国产)", series = "CP1H/CP1L/CP2E", description = "国产化欧姆龙" },
                new { name = "西门子(国产)", series = "S7-200 SMART", description = "国产西门子" },
                new { name = "三菱电机", series = "FX3U/FX5U", description = "国产三菱" },
                new { name = "广州数控", series = "GSK PLC", description = "广数PLC" },
                new { name = "华中数控", series = "HNC PLC", description = "华中PLC" }
            };

            return Ok(brands);
        }

        [HttpGet("foreign")]
        public IActionResult GetForeignPlc()
        {
            var brands = new[]
            {
                new { name = "西门子", series = "S7-1200/S7-1500/S7-300", country = "德国" },
                new { name = "三菱", series = "Q系列/L系列/FX系列", country = "日本" },
                new { name = "罗克韦尔", series = "ControlLogix/CompactLogix/Micro800", country = "美国" },
                new { name = "欧姆龙", series = "CJ2M/CJ2H/NX系列", country = "日本" },
                new { name = "施耐德", series = "Modicon M340/M580/Quantum", country = "法国" },
                new { name = "富士电机", series = "NP1/NP2/F系列", country = "日本" },
                new { name = "AB罗克韦尔", series = "Allen-Bradley", country = "美国" },
                new { name = "倍福", series = "CX系列/BK系列", country = "德国" }
            };

            return Ok(brands);
        }

        [HttpGet("commands")]
        public IActionResult GetCommands()
        {
            var commands = new
            {
                siemens = new[]
                {
                    new { name = "LD/LDN", description = "装载/装载非" },
                    new { name = "A/AN", description = "与/与非" },
                    new { name = "O/ON", description = "或/或非" },
                    new { name = "= (OUT)", description = "输出" },
                    new { name = "S/R", description = "置位/复位" },
                    new { name = "TON/TOF", description = "接通延时/断开延时" },
                    new { name = "CTU/CTD", description = "加计数器/减计数器" },
                    new { name = "MOV", description = "数据传送" },
                    new { name = "ADD/SUB", description = "加法/减法" },
                    new { name = "MUL/DIV", description = "乘法/除法" }
                },
                mitsubishi = new[]
                {
                    new { name = "LD/LDI", description = "取/取反" },
                    new { name = "AND/ANI", description = "与/与非" },
                    new { name = "OR/ORI", description = "或/或非" },
                    new { name = "OUT", description = "输出" },
                    new { name = "SET/RST", description = "置位/复位" },
                    new { name = "TMR/TMX", description = "定时器" },
                    new { name = "CTU/CTD", description = "计数器" },
                    new { name = "MOV", description = "传送" },
                    new { name = "ADD/SUB", description = "加/减" },
                    new { name = "MUL/DIV", description = "乘/除" }
                },
                omron = new[]
                {
                    new { name = "LD/LDNOT", description = "取/取非" },
                    new { name = "AND/ANDNOT", description = "与/与非" },
                    new { name = "OR/ORNOT", description = "或/或非" },
                    new { name = "OUT", description = "输出" },
                    new { name = "SET/RSET", description = "置位/复位" },
                    new { name = "TIM/TIMH", description = "定时器" },
                    new { name = "CNT", description = "计数器" },
                    new { name = "MOV", description = "传送" },
                    new { name = "ADD/SUB", description = "加/减" },
                    new { name = "MUL/DIV", description = "乘/除" }
                }
            };

            return Ok(commands);
        }

        [HttpGet("ports")]
        public IActionResult GetAvailablePorts()
        {
            var ports = _communicationService.GetAvailablePorts();
            return Ok(new { ports });
        }

        [HttpPost("connect/serial")]
        public IActionResult ConnectSerial([FromBody] SerialConnectRequest request)
        {
            var parity = ParseParity(request.Parity);
            var stopBits = ParseStopBits(request.StopBits);

            var result = _communicationService.ConnectSerial(
                request.PortName,
                request.BaudRate,
                request.DataBits,
                parity,
                stopBits
            );

            return Ok(new { status = result.Contains("成功") ? "success" : "error", message = result });
        }

        [HttpPost("disconnect/serial")]
        public IActionResult DisconnectSerial([FromBody] PortRequest request)
        {
            var result = _communicationService.DisconnectSerial(request.PortName);
            return Ok(new { status = result.Contains("成功") ? "success" : "error", message = result });
        }

        [HttpPost("send/serial")]
        public IActionResult SendSerialData([FromBody] SerialDataRequest request)
        {
            var byteData = request.Data.Select(d => (byte)d).ToArray();
            var (status, response) = _communicationService.SendSerialData(request.PortName, byteData);
            return Ok(new { status = status.Contains("成功") ? "success" : "error", message = status, response });
        }

        [HttpPost("connect/tcp")]
        public IActionResult ConnectTcp([FromBody] TcpConnectRequest request)
        {
            var result = _communicationService.ConnectTcp(request.IpAddress, request.Port);
            return Ok(new { status = result.Contains("成功") ? "success" : "error", message = result });
        }

        [HttpPost("disconnect/tcp")]
        public IActionResult DisconnectTcp([FromBody] TcpConnectRequest request)
        {
            var result = _communicationService.DisconnectTcp(request.IpAddress, request.Port);
            return Ok(new { status = result.Contains("成功") ? "success" : "error", message = result });
        }

        [HttpPost("send/tcp")]
        public IActionResult SendTcpData([FromBody] TcpDataRequest request)
        {
            var byteData = request.Data.Select(d => (byte)d).ToArray();
            var (status, response) = _communicationService.SendTcpData(request.IpAddress, request.Port, byteData);
            return Ok(new { status = status.Contains("成功") ? "success" : "error", message = status, response });
        }

        [HttpGet("receive/tcp")]
        public IActionResult GetReceivedData([FromQuery] string ipAddress, [FromQuery] int port)
        {
            var dataList = _communicationService.GetReceivedData(ipAddress, port);
            var result = dataList.Select(d => d.Select(b => (int)b).ToArray()).ToList();
            return Ok(new { status = "success", data = result });
        }

        [HttpGet("status/tcp")]
        public IActionResult GetTcpStatus([FromQuery] string ipAddress, [FromQuery] int port)
        {
            var isConnected = _communicationService.IsTcpConnected(ipAddress, port);
            return Ok(new { status = "success", connected = isConnected });
        }

        [HttpGet("logs")]
        public IActionResult GetLogs()
        {
            var logs = _communicationService.GetLogs();
            return Ok(logs);
        }

        [HttpPost("crc")]
        public IActionResult CalculateCrc([FromBody] CrcRequest request)
        {
            var data = request.Data;
            var type = request.Type;

            uint crc = 0;

            if (type == "crc16" || type == "crc16modbus")
            {
                ushort crc16 = 0xFFFF;
                ushort poly = 0xA001;

                foreach (byte b in data)
                {
                    crc16 ^= b;
                    for (int i = 0; i < 8; i++)
                    {
                        crc16 = (ushort)((crc16 >> 1) ^ ((crc16 & 1) != 0 ? poly : 0));
                    }
                }
                crc = crc16;
            }
            else if (type == "crc16ccitt")
            {
                ushort crc16 = 0xFFFF;
                ushort poly = 0x1021;

                foreach (byte b in data)
                {
                    crc16 ^= (ushort)(b << 8);
                    for (int i = 0; i < 8; i++)
                    {
                        crc16 = (ushort)((crc16 << 1) ^ ((crc16 & 0x8000) != 0 ? poly : 0));
                    }
                }
                crc = crc16;
            }
            else if (type == "crc32")
            {
                uint crc32 = 0xFFFFFFFF;
                uint[] table = new uint[256];

                for (uint i = 0; i < 256; i++)
                {
                    uint c = i;
                    for (int j = 0; j < 8; j++)
                    {
                        c = (c & 1) != 0 ? (0xEDB88320 ^ (c >> 1)) : (c >> 1);
                    }
                    table[i] = c;
                }

                foreach (byte b in data)
                {
                    crc32 = table[(crc32 ^ b) & 0xFF] ^ (crc32 >> 8);
                }
                crc = crc32 ^ 0xFFFFFFFF;
            }

            return Ok(new { result = crc.ToString("X") });
        }

        [HttpPost("parse")]
        public IActionResult ParseMessage([FromBody] ParseRequest request)
        {
            var data = request.Data;
            var protocol = request.Protocol;

            var result = new
            {
                protocol,
                length = data.Length,
                hex = BitConverter.ToString(data).Replace("-", " "),
                ascii = System.Text.Encoding.ASCII.GetString(data),
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            return Ok(result);
        }

        private Parity ParseParity(string parity)
        {
            return parity.ToLower() switch
            {
                "odd" => Parity.Odd,
                "even" => Parity.Even,
                _ => Parity.None
            };
        }

        private StopBits ParseStopBits(string stopBits)
        {
            return stopBits switch
            {
                "1.5" => StopBits.OnePointFive,
                "2" => StopBits.Two,
                _ => StopBits.One
            };
        }

        public class SerialConnectRequest
        {
            public string PortName { get; set; } = string.Empty;
            public int BaudRate { get; set; } = 115200;
            public int DataBits { get; set; } = 8;
            public string Parity { get; set; } = "none";
            public string StopBits { get; set; } = "1";
        }

        public class PortRequest
        {
            public string PortName { get; set; } = string.Empty;
        }

        public class SerialDataRequest
        {
            public string PortName { get; set; } = string.Empty;
            public int[] Data { get; set; } = Array.Empty<int>();
        }

        public class TcpConnectRequest
        {
            public string IpAddress { get; set; } = string.Empty;
            public int Port { get; set; } = 502;
        }

        public class TcpDataRequest
        {
            public string IpAddress { get; set; } = string.Empty;
            public int Port { get; set; } = 502;
            public int[] Data { get; set; } = Array.Empty<int>();
        }

        public class CrcRequest
        {
            public byte[] Data { get; set; } = Array.Empty<byte>();
            public string Type { get; set; } = "crc16";
        }

        public class ParseRequest
        {
            public byte[] Data { get; set; } = Array.Empty<byte>();
            public string Protocol { get; set; } = "modbus";
        }
    }
}