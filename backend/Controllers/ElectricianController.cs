using Microsoft.AspNetCore.Mvc;

namespace IndustrialToolkit.Controllers
{
    [ApiController]
    [Route("api/electrician")]
    public class ElectricianController : ControllerBase
    {
        [HttpPost("resistor")]
        public IActionResult CalculateResistor([FromBody] ResistorRequest request)
        {
            var bands = request.Bands;

            int value = bands[0] * 100 + bands[1] * 10 + bands[2];
            int multiplier = (int)Math.Pow(10, bands[3]);
            double resistance = value * multiplier;

            string unit = "Ω";
            double displayValue = resistance;

            if (resistance >= 1000000)
            {
                displayValue = resistance / 1000000;
                unit = "MΩ";
            }
            else if (resistance >= 1000)
            {
                displayValue = resistance / 1000;
                unit = "kΩ";
            }

            var result = new
            {
                bands,
                resistance,
                displayValue,
                unit,
                tolerance = 5,
                minValue = resistance * 0.95,
                maxValue = resistance * 1.05,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            return Ok(result);
        }

        [HttpPost("subnet")]
        public IActionResult CalculateSubnet([FromBody] SubnetRequest request)
        {
            var ipParts = request.IpAddress.Split('.').Select(int.Parse).ToArray();
            var maskParts = request.SubnetMask.Split('.').Select(int.Parse).ToArray();

            var networkParts = ipParts.Zip(maskParts, (i, m) => i & m).ToArray();
            var broadcastParts = ipParts.Zip(maskParts, (i, m) => i | (255 - m)).ToArray();

            var usableStart = networkParts.ToArray();
            usableStart[3]++;
            var usableEnd = broadcastParts.ToArray();
            usableEnd[3]--;

            var cidr = maskParts.Sum(m => Convert.ToString(m, 2).Count(c => c == '1'));
            var hostBits = 32 - cidr;
            var hostCount = (int)Math.Pow(2, hostBits) - 2;

            var result = new
            {
                ipAddress = request.IpAddress,
                subnetMask = request.SubnetMask,
                networkAddress = string.Join(".", networkParts),
                broadcastAddress = string.Join(".", broadcastParts),
                usableRange = $"{string.Join(".", usableStart)} - {string.Join(".", usableEnd)}",
                hostCount,
                cidr,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            return Ok(result);
        }

        [HttpPost("power")]
        public IActionResult CalculatePower([FromBody] PowerRequest request)
        {
            var voltage = request.Voltage;
            var current = request.Current;
            var powerFactor = request.PowerFactor;
            var phase = request.Phase;

            double power, reactivePower, apparentPower;

            if (phase == 3)
            {
                power = Math.Sqrt(3) * voltage * current * powerFactor;
                reactivePower = Math.Sqrt(3) * voltage * current * Math.Sqrt(1 - powerFactor * powerFactor);
                apparentPower = Math.Sqrt(3) * voltage * current;
            }
            else
            {
                power = voltage * current * powerFactor;
                reactivePower = voltage * current * Math.Sqrt(1 - powerFactor * powerFactor);
                apparentPower = voltage * current;
            }

            var result = new
            {
                voltage,
                current,
                powerFactor,
                phase,
                activePower = power / 1000,
                reactivePower = reactivePower / 1000,
                apparentPower = apparentPower / 1000,
                resistance = voltage / current,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            return Ok(result);
        }

        [HttpPost("cable")]
        public IActionResult SelectCable([FromBody] CableRequest request)
        {
            var current = request.Current;
            var type = request.Type;

            var cables = new[]
            {
                new { size = "1.5 mm²", copperCurrent = 16, aluminumCurrent = 12 },
                new { size = "2.5 mm²", copperCurrent = 22, aluminumCurrent = 17 },
                new { size = "4 mm²", copperCurrent = 28, aluminumCurrent = 22 },
                new { size = "6 mm²", copperCurrent = 36, aluminumCurrent = 28 },
                new { size = "10 mm²", copperCurrent = 50, aluminumCurrent = 39 },
                new { size = "16 mm²", copperCurrent = 68, aluminumCurrent = 52 },
                new { size = "25 mm²", copperCurrent = 90, aluminumCurrent = 69 },
                new { size = "35 mm²", copperCurrent = 110, aluminumCurrent = 85 }
            };

            var recommended = cables.FirstOrDefault(c => type == "copper" ? c.copperCurrent >= current : c.aluminumCurrent >= current) ?? cables.Last();

            var result = new
            {
                current,
                type,
                recommendedCable = recommended,
                allCables = cables,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            return Ok(result);
        }

        [HttpPost("relay")]
        public IActionResult SelectRelay([FromBody] RelayRequest request)
        {
            var voltage = request.Voltage;
            var contact = request.ContactType;
            var current = request.Current;

            var relays = new[]
            {
                new { model = "HH52P/MY2N", voltage = "DC 12V/24V", contactType = "SPDT", current = 5 },
                new { model = "HH54P/MY4N", voltage = "DC 12V/24V", contactType = "4PDT", current = 5 },
                new { model = "JQX-13F", voltage = "AC 220V", contactType = "DPDT", current = 10 },
                new { model = "SSR-25DA", voltage = "DC 3-32V", contactType = "SPST", current = 25 },
                new { model = "G2R-1", voltage = "DC 12V/24V", contactType = "SPDT", current = 10 },
                new { model = "LY2N", voltage = "AC 110V/220V", contactType = "DPDT", current = 10 }
            };

            return Ok(new { relays, timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
        }

        public class ResistorRequest
        {
            public int[] Bands { get; set; } = new[] { 0, 0, 0, 1 };
        }

        public class SubnetRequest
        {
            public string IpAddress { get; set; } = "192.168.1.100";
            public string SubnetMask { get; set; } = "255.255.255.0";
        }

        public class PowerRequest
        {
            public double Voltage { get; set; } = 380;
            public double Current { get; set; } = 10;
            public double PowerFactor { get; set; } = 0.85;
            public int Phase { get; set; } = 3;
        }

        public class CableRequest
        {
            public double Current { get; set; } = 50;
            public string Type { get; set; } = "copper";
        }

        public class RelayRequest
        {
            public string Voltage { get; set; } = "24";
            public string ContactType { get; set; } = "SPDT";
            public int Current { get; set; } = 5;
        }
    }
}