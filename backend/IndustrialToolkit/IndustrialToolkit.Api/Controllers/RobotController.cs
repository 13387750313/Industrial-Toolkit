using IndustrialToolkit.Services;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialToolkit.Controllers
{
    [ApiController]
    [Route("api/robot")]
    public class RobotController : ControllerBase
    {
        private readonly RobotCommunicationService _communicationService;

        public RobotController(RobotCommunicationService communicationService)
        {
            _communicationService = communicationService;
        }

        [HttpGet("domestic")]
        public IActionResult GetDomesticRobots()
        {
            var robots = new[]
            {
                new { name = "埃斯顿", englishName = "ESTUN", series = "ER系列", load = "3-500kg", description = "国产工业机器人领军品牌" },
                new { name = "新时达", englishName = "STEP", series = "SD系列", load = "3-210kg", description = "高性能工业机器人" },
                new { name = "汇川技术", englishName = "INOVANCE", series = "IR系列", load = "6-200kg", description = "汇川工业机器人" },
                new { name = "华数机器人", englishName = "HUST", series = "HS系列", load = "3-50kg", description = "华数工业机器人" },
                new { name = "广州数控", englishName = "GSK", series = "GR系列", load = "6-100kg", description = "广数工业机器人" },
                new { name = "大族机器人", englishName = "Han's Robot", series = "EL系列", load = "3-12kg", description = "协作机器人" },
                new { name = "遨博智能", englishName = "AUBO", series = "i系列", load = "3-20kg", description = "协作机器人专家" },
                new { name = "节卡机器人", englishName = "JAKA", series = "Zu系列", load = "3-12kg", description = "轻量协作机器人" }
            };

            return Ok(robots);
        }

        [HttpGet("foreign")]
        public IActionResult GetForeignRobots()
        {
            var robots = new[]
            {
                new { name = "FANUC", chineseName = "发那科", country = "日本", series = "R系列", load = "3-2300kg" },
                new { name = "ABB", chineseName = "ABB", country = "瑞士", series = "IRB系列", load = "3-800kg" },
                new { name = "KUKA", chineseName = "库卡", country = "德国", series = "KR系列", load = "6-1300kg" },
                new { name = "YASKAWA", chineseName = "安川", country = "日本", series = "MOTOMAN系列", load = "3-700kg" },
                new { name = "Universal Robots", chineseName = "优傲", country = "丹麦", series = "UR系列", load = "3-16kg" },
                new { name = "DENSO", chineseName = "电装", country = "日本", series = "VS系列", load = "0.5-5kg" },
                new { name = "EPSON", chineseName = "爱普生", country = "日本", series = "LS系列", load = "1-20kg" },
                new { name = "STAUBLI", chineseName = "史陶比尔", country = "瑞士", series = "TX系列", load = "6-90kg" }
            };

            return Ok(robots);
        }

        [HttpGet("protocols")]
        public IActionResult GetProtocols()
        {
            var protocols = new[]
            {
                new { name = "TCP/IP", port = 0, description = "标准以太网通讯" },
                new { name = "UDP", port = 0, description = "无连接高速通讯" },
                new { name = "FANUC Focas", port = 8193, description = "FANUC专用协议" },
                new { name = "ABB PC Interface", port = 0, description = "ABB机器人协议" },
                new { name = "KUKA KRL", port = 0, description = "KUKA机器人语言" },
                new { name = "YASKAWA MotoPlus", port = 0, description = "安川机器人接口" },
                new { name = "Modbus TCP", port = 502, description = "工业标准协议" },
                new { name = "Socket", port = 0, description = "通用Socket通讯" }
            };

            return Ok(protocols);
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

        [HttpPost("connect/udp")]
        public IActionResult ConnectUdp([FromBody] UdpConnectRequest request)
        {
            var result = _communicationService.ConnectUdp(request.LocalPort);
            return Ok(new { status = result.Contains("成功") ? "success" : "error", message = result });
        }

        [HttpPost("disconnect/udp")]
        public IActionResult DisconnectUdp([FromBody] UdpConnectRequest request)
        {
            var result = _communicationService.DisconnectUdp(request.LocalPort);
            return Ok(new { status = result.Contains("成功") ? "success" : "error", message = result });
        }

        [HttpPost("send/udp")]
        public IActionResult SendUdpData([FromBody] UdpDataRequest request)
        {
            var byteData = request.Data.Select(d => (byte)d).ToArray();
            var (status, response) = _communicationService.SendUdpData(request.LocalPort, request.RemoteIp, request.RemotePort, byteData);
            return Ok(new { status = status.Contains("成功") ? "success" : "error", message = status, response });
        }

        [HttpPost("send/fanuc")]
        public IActionResult SendFanucCommand([FromBody] FanucRequest request)
        {
            var (status, response) = _communicationService.SendFanucCommand(request.IpAddress, request.Port, request.Command);
            return Ok(new { status = status.Contains("成功") ? "success" : "error", message = status, response });
        }

        [HttpGet("logs")]
        public IActionResult GetLogs()
        {
            var logs = _communicationService.GetLogs();
            return Ok(logs);
        }

        [HttpPost("kinematics")]
        public IActionResult CalculateKinematics([FromBody] KinematicsRequest request)
        {
            var joints = request.Joints;

            var result = new
            {
                inputJoints = joints,
                position = new
                {
                    x = joints[0] * 0.5 + joints[1] * 0.3,
                    y = joints[2] * 0.4 + joints[3] * 0.2,
                    z = joints[4] * 0.3 + joints[5] * 0.1
                },
                orientation = new
                {
                    a = joints[3],
                    b = joints[4],
                    c = joints[5]
                },
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            return Ok(result);
        }

        public class TcpConnectRequest
        {
            public string IpAddress { get; set; } = string.Empty;
            public int Port { get; set; } = 8080;
        }

        public class TcpDataRequest
        {
            public string IpAddress { get; set; } = string.Empty;
            public int Port { get; set; } = 8080;
            public int[] Data { get; set; } = Array.Empty<int>();
        }

        public class UdpConnectRequest
        {
            public int LocalPort { get; set; } = 8080;
        }

        public class UdpDataRequest
        {
            public int LocalPort { get; set; } = 8080;
            public string RemoteIp { get; set; } = string.Empty;
            public int RemotePort { get; set; } = 8080;
            public int[] Data { get; set; } = Array.Empty<int>();
        }

        public class FanucRequest
        {
            public string IpAddress { get; set; } = string.Empty;
            public int Port { get; set; } = 8193;
            public string Command { get; set; } = string.Empty;
        }

        public class KinematicsRequest
        {
            public double[] Joints { get; set; } = new double[6];
        }
    }
}