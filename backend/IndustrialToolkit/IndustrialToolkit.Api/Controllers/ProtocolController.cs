using IndustrialToolkit.Services;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialToolkit.Api.Controllers
{
    /// <summary>
    /// 工业协议API控制器 - Modbus, Ethernet/IP, Profinet, FinsTCP, Profibus, DeviceNet, CC-Link, AS-i
    /// </summary>
    [ApiController]
    [Route("api/protocol")]
    public class ProtocolController : ControllerBase
    {
        private readonly IndustrialProtocolService _protocolService;

        public ProtocolController(IndustrialProtocolService protocolService)
        {
            _protocolService = protocolService;
        }

        #region Modbus TCP
        [HttpPost("modbus/connect")]
        public IActionResult ModbusConnect([FromBody] ProtocolConnectRequest request)
        {
            var result = _protocolService.ConnectModbus(request.IpAddress, request.Port);
            return Ok(new { status = result.Contains("成功") ? "success" : "error", message = result });
        }

        [HttpPost("modbus/disconnect")]
        public IActionResult ModbusDisconnect([FromBody] ProtocolConnectRequest request)
        {
            var result = _protocolService.DisconnectModbus(request.IpAddress, request.Port);
            return Ok(new { status = result.Contains("成功") ? "success" : "error", message = result });
        }

        [HttpPost("modbus/send")]
        public IActionResult ModbusSend([FromBody] ModbusRequest request)
        {
            var (status, response) = _protocolService.SendModbus(
                request.IpAddress, request.Port, request.SlaveId,
                request.FunctionCode, request.Address, request.Count, request.Values);
            return Ok(new { status = status.Contains("成功") ? "success" : "error", message = status, response });
        }
        #endregion

        #region Ethernet/IP
        [HttpPost("ethernetip/connect")]
        public IActionResult EipConnect([FromBody] ProtocolConnectRequest request)
        {
            var result = _protocolService.ConnectEip(request.IpAddress, request.Port);
            return Ok(new { status = result.Contains("成功") ? "success" : "error", message = result });
        }

        [HttpPost("ethernetip/disconnect")]
        public IActionResult EipDisconnect([FromBody] ProtocolConnectRequest request)
        {
            var result = _protocolService.DisconnectEip(request.IpAddress, request.Port);
            return Ok(new { status = result.Contains("成功") ? "success" : "error", message = result });
        }

        [HttpPost("ethernetip/send")]
        public IActionResult EipSend([FromBody] EipRequest request)
        {
            var (status, response) = _protocolService.SendEip(
                request.IpAddress, request.Port, request.Service, request.Tag, request.Data);
            return Ok(new { status = status.Contains("成功") ? "success" : "error", message = status, response });
        }
        #endregion

        #region Profinet
        [HttpPost("profinet/connect")]
        public IActionResult ProfinetConnect([FromBody] ProtocolConnectRequest request)
        {
            var result = _protocolService.ConnectProfinet(request.IpAddress, request.Port);
            return Ok(new { status = result.Contains("成功") ? "success" : "error", message = result });
        }

        [HttpPost("profinet/disconnect")]
        public IActionResult ProfinetDisconnect([FromBody] ProtocolConnectRequest request)
        {
            var result = _protocolService.DisconnectProfinet(request.IpAddress, request.Port);
            return Ok(new { status = result.Contains("成功") ? "success" : "error", message = result });
        }

        [HttpPost("profinet/send")]
        public IActionResult ProfinetSend([FromBody] ProfinetRequest request)
        {
            var (status, response) = _protocolService.SendProfinet(
                request.IpAddress, request.Port, request.Operation, request.Slot, request.Data);
            return Ok(new { status = status.Contains("成功") ? "success" : "error", message = status, response });
        }
        #endregion

        #region FinsTCP
        [HttpPost("fins/connect")]
        public IActionResult FinsConnect([FromBody] ProtocolConnectRequest request)
        {
            var result = _protocolService.ConnectFins(request.IpAddress, request.Port);
            return Ok(new { status = result.Contains("成功") ? "success" : "error", message = result });
        }

        [HttpPost("fins/disconnect")]
        public IActionResult FinsDisconnect([FromBody] ProtocolConnectRequest request)
        {
            var result = _protocolService.DisconnectFins(request.IpAddress, request.Port);
            return Ok(new { status = result.Contains("成功") ? "success" : "error", message = result });
        }

        [HttpPost("fins/send")]
        public IActionResult FinsSend([FromBody] FinsRequest request)
        {
            var (status, response) = _protocolService.SendFins(
                request.IpAddress, request.Port, request.Command, request.Area, request.Address, request.Count);
            return Ok(new { status = status.Contains("成功") ? "success" : "error", message = status, response });
        }
        #endregion

        #region Profibus DP
        [HttpPost("profibus/connect")]
        public IActionResult ProfibusConnect([FromBody] ProfibusConnectRequest request)
        {
            var result = _protocolService.ConnectProfibus(request.PortName, request.BaudRate);
            return Ok(new { status = result.Contains("成功") ? "success" : "error", message = result });
        }

        [HttpPost("profibus/disconnect")]
        public IActionResult ProfibusDisconnect([FromBody] ProfibusConnectRequest request)
        {
            var result = _protocolService.DisconnectProfibus(request.PortName, request.BaudRate);
            return Ok(new { status = result.Contains("成功") ? "success" : "error", message = result });
        }

        [HttpPost("profibus/send")]
        public IActionResult ProfibusSend([FromBody] ProfibusRequest request)
        {
            var (status, response) = _protocolService.SendProfibus(
                request.PortName, request.BaudRate, request.SlaveId, request.Function,
                request.Address, request.Length, request.Data);
            return Ok(new { status = status.Contains("成功") ? "success" : "error", message = status, response });
        }
        #endregion

        #region DeviceNet
        [HttpPost("devicenet/connect")]
        public IActionResult DeviceNetConnect([FromBody] DeviceNetConnectRequest request)
        {
            var result = _protocolService.ConnectDeviceNet(request.ChannelName, request.BaudRate);
            return Ok(new { status = result.Contains("成功") ? "success" : "error", message = result });
        }

        [HttpPost("devicenet/disconnect")]
        public IActionResult DeviceNetDisconnect([FromBody] DeviceNetConnectRequest request)
        {
            var result = _protocolService.DisconnectDeviceNet(request.ChannelName, request.BaudRate);
            return Ok(new { status = result.Contains("成功") ? "success" : "error", message = result });
        }

        [HttpPost("devicenet/send")]
        public IActionResult DeviceNetSend([FromBody] DeviceNetRequest request)
        {
            var (status, response) = _protocolService.SendDeviceNet(
                request.ChannelName, request.BaudRate, request.MacId, request.ConnectionId, request.Data);
            return Ok(new { status = status.Contains("成功") ? "success" : "error", message = status, response });
        }
        #endregion

        #region CC-Link
        [HttpPost("cclink/connect")]
        public IActionResult CCLinkConnect([FromBody] ProtocolConnectRequest request)
        {
            var result = _protocolService.ConnectCCLink(request.IpAddress, request.Port);
            return Ok(new { status = result.Contains("成功") ? "success" : "error", message = result });
        }

        [HttpPost("cclink/disconnect")]
        public IActionResult CCLinkDisconnect([FromBody] ProtocolConnectRequest request)
        {
            var result = _protocolService.DisconnectCCLink(request.IpAddress, request.Port);
            return Ok(new { status = result.Contains("成功") ? "success" : "error", message = result });
        }

        [HttpPost("cclink/send")]
        public IActionResult CCLinkSend([FromBody] CCLinkRequest request)
        {
            var (status, response) = _protocolService.SendCCLink(
                request.IpAddress, request.Port, request.Station, request.Operation,
                request.Address, request.Count, request.Data);
            return Ok(new { status = status.Contains("成功") ? "success" : "error", message = status, response });
        }
        #endregion

        #region AS-Interface
        [HttpPost("asinterface/connect")]
        public IActionResult ASInterfaceConnect([FromBody] ASInterfaceConnectRequest request)
        {
            var result = _protocolService.ConnectASInterface(request.PortName, request.BaudRate);
            return Ok(new { status = result.Contains("成功") ? "success" : "error", message = result });
        }

        [HttpPost("asinterface/disconnect")]
        public IActionResult ASInterfaceDisconnect([FromBody] ASInterfaceConnectRequest request)
        {
            var result = _protocolService.DisconnectASInterface(request.PortName, request.BaudRate);
            return Ok(new { status = result.Contains("成功") ? "success" : "error", message = result });
        }

        [HttpPost("asinterface/send")]
        public IActionResult ASInterfaceSend([FromBody] ASInterfaceRequest request)
        {
            var (status, response) = _protocolService.SendASInterface(
                request.PortName, request.BaudRate, request.MasterAddr, request.SlaveAddr,
                request.Operation, request.Data);
            return Ok(new { status = status.Contains("成功") ? "success" : "error", message = status, response });
        }
        #endregion

        [HttpGet("logs")]
        public IActionResult GetLogs()
        {
            var logs = _protocolService.GetLogs();
            return Ok(new { status = "success", data = logs, count = logs.Count });
        }

        [HttpDelete("logs")]
        public IActionResult ClearLogs()
        {
            _protocolService.ClearLogs();
            return Ok(new { status = "success", message = "日志已清空" });
        }

        [HttpGet("logs/count")]
        public IActionResult GetLogCount()
        {
            var count = _protocolService.GetLogCount();
            return Ok(new { status = "success", count });
        }
    }

    #region Request Models
    public class ProtocolConnectRequest
    {
        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; }
    }

    public class ModbusRequest
    {
        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; } = 502;
        public int SlaveId { get; set; } = 1;
        public int FunctionCode { get; set; } = 3;
        public int Address { get; set; }
        public int Count { get; set; } = 1;
        public string Values { get; set; } = string.Empty;
    }

    public class EipRequest
    {
        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; } = 44818;
        public string Service { get; set; } = "0x4C";
        public string Tag { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
    }

    public class ProfinetRequest
    {
        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; } = 34964;
        public string Operation { get; set; } = "read";
        public string Slot { get; set; } = "0/0";
        public string Data { get; set; } = string.Empty;
    }

    public class FinsRequest
    {
        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; } = 9600;
        public string Command { get; set; } = "0101";
        public string Area { get; set; } = "DM";
        public int Address { get; set; }
        public int Count { get; set; } = 1;
    }

    public class ProfibusConnectRequest
    {
        public string PortName { get; set; } = string.Empty;
        public int BaudRate { get; set; } = 9600;
    }

    public class ProfibusRequest
    {
        public string PortName { get; set; } = string.Empty;
        public int BaudRate { get; set; } = 9600;
        public int SlaveId { get; set; } = 3;
        public string Function { get; set; } = "read";
        public int Address { get; set; }
        public int Length { get; set; } = 1;
        public string Data { get; set; } = string.Empty;
    }

    public class DeviceNetConnectRequest
    {
        public string ChannelName { get; set; } = string.Empty;
        public int BaudRate { get; set; } = 125000;
    }

    public class DeviceNetRequest
    {
        public string ChannelName { get; set; } = string.Empty;
        public int BaudRate { get; set; } = 125000;
        public int MacId { get; set; }
        public string ConnectionId { get; set; } = "0x01";
        public string Data { get; set; } = string.Empty;
    }

    public class CCLinkRequest
    {
        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; } = 61451;
        public int Station { get; set; }
        public string Operation { get; set; } = "read";
        public string Address { get; set; } = string.Empty;
        public int Count { get; set; } = 1;
        public string Data { get; set; } = string.Empty;
    }

    public class ASInterfaceConnectRequest
    {
        public string PortName { get; set; } = string.Empty;
        public int BaudRate { get; set; } = 19200;
    }

    public class ASInterfaceRequest
    {
        public string PortName { get; set; } = string.Empty;
        public int BaudRate { get; set; } = 19200;
        public int MasterAddr { get; set; }
        public int SlaveAddr { get; set; } = 1;
        public string Operation { get; set; } = "read";
        public string Data { get; set; } = string.Empty;
    }
    #endregion
}
