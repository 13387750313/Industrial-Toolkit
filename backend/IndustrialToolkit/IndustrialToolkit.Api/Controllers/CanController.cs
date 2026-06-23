using IndustrialToolkit.Services;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialToolkit.Api.Controllers
{
    [ApiController]
    [Route("api/can")]
    public class CanController : ControllerBase
    {
        private readonly CanCommunicationService _canService;

        public CanController(CanCommunicationService canService)
        {
            _canService = canService;
        }

        [HttpGet("channels")]
        public IActionResult GetAvailableChannels()
        {
            try
            {
                var channels = _canService.GetAvailableChannels();
                return Ok(new { channels });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("open")]
        public IActionResult OpenChannel([FromBody] CanOpenRequest request)
        {
            try
            {
                var result = _canService.OpenChannel(request.ChannelName, request.BaudRate);
                return Ok(new { success = result == "成功", message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("close")]
        public IActionResult CloseChannel([FromBody] CanCloseRequest request)
        {
            try
            {
                var result = _canService.CloseChannel(request.ChannelName);
                return Ok(new { success = result == "关闭成功", message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("send")]
        public IActionResult SendMessage([FromBody] CanSendRequest request)
        {
            try
            {
                byte[] data;
                if (request.DataHex != null)
                {
                    data = HexStringToBytes(request.DataHex);
                }
                else if (request.Data != null)
                {
                    data = request.Data;
                }
                else
                {
                    data = Array.Empty<byte>();
                }

                var (status, frame) = _canService.SendCanMessage(request.ChannelName, request.Id, data, request.IsExtendedId);
                
                return Ok(new
                {
                    success = status == "成功",
                    message = status,
                    frame = frame != null ? new
                    {
                        id = frame.Id,
                        isExtendedId = frame.IsExtendedId,
                        length = frame.Length,
                        data = BitConverter.ToString(frame.Data).Replace("-", " ")
                    } : null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("receive")]
        public IActionResult ReceiveMessage([FromBody] CanReceiveRequest request)
        {
            try
            {
                var (status, frame) = _canService.ReceiveCanMessage(request.ChannelName);

                return Ok(new
                {
                    success = status == "接收成功" || status == "无数据",
                    message = status,
                    frame = frame != null ? new
                    {
                        id = frame.Id,
                        isExtendedId = frame.IsExtendedId,
                        length = frame.Length,
                        data = BitConverter.ToString(frame.Data).Replace("-", " ")
                    } : null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("status/{channelName}")]
        public IActionResult GetChannelStatus(string channelName)
        {
            try
            {
                var isOpen = _canService.IsChannelOpen(channelName);
                return Ok(new { channelName, isOpen });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("logs")]
        public IActionResult GetLogs()
        {
            try
            {
                var logs = _canService.GetLogs();
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
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

    public class CanOpenRequest
    {
        public string ChannelName { get; set; } = string.Empty;
        public int BaudRate { get; set; } = 250000;
    }

    public class CanCloseRequest
    {
        public string ChannelName { get; set; } = string.Empty;
    }

    public class CanSendRequest
    {
        public string ChannelName { get; set; } = string.Empty;
        public int Id { get; set; }
        public byte[]? Data { get; set; }
        public string? DataHex { get; set; }
        public bool IsExtendedId { get; set; } = false;
    }

    public class CanReceiveRequest
    {
        public string ChannelName { get; set; } = string.Empty;
    }
}
