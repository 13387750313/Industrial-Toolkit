using IndustrialToolkit.Services;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialToolkit.Api.Controllers
{
    [ApiController]
    [Route("api/network")]
    public class NetworkController : ControllerBase
    {
        private readonly NetworkToolService _networkService;

        public NetworkController(NetworkToolService networkService)
        {
            _networkService = networkService;
        }

        [HttpGet("lan/scan")]
        public async Task<IActionResult> ScanLan(string ipRange = "192.168.1.")
        {
            try
            {
                var devices = await _networkService.ScanLan(ipRange);
                return Ok(new { status = "success", data = devices, count = devices.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("port/scan")]
        public async Task<IActionResult> ScanPorts(string ipAddress, int startPort = 1, int endPort = 100)
        {
            try
            {
                var ports = await _networkService.ScanPorts(ipAddress, startPort, endPort);
                var occupied = ports.Where(p => p.Status == "占用").ToList();
                return Ok(new { status = "success", data = ports, occupiedCount = occupied.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("ping")]
        public async Task<IActionResult> PingHost(string host, int count = 4)
        {
            try
            {
                var result = await _networkService.PingHost(host, count);
                return Ok(new { status = "success", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("ip/query")]
        public async Task<IActionResult> QueryIP(string ipOrDomain)
        {
            try
            {
                var result = await _networkService.QueryIP(ipOrDomain);
                return Ok(new { status = string.IsNullOrEmpty(result.Error) ? "success" : "error", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("whois")]
        public async Task<IActionResult> QueryWhois(string domain)
        {
            try
            {
                var result = await _networkService.QueryWhois(domain);
                return Ok(new { status = "success", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }
    }
}
