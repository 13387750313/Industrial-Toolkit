using Microsoft.AspNetCore.Mvc;

namespace IndustrialToolkit.Controllers
{
    [ApiController]
    [Route("api/structure")]
    public class StructureController : ControllerBase
    {
        [HttpPost("bolt")]
        public IActionResult CalculateBolt([FromBody] BoltRequest request)
        {
            var size = request.Size;
            var material = request.Material;
            var load = request.WorkingLoad;

            var diameters = new Dictionary<string, double>
            {
                { "M6", 6 }, { "M8", 8 }, { "M10", 10 }, { "M12", 12 },
                { "M16", 16 }, { "M20", 20 }, { "M24", 24 }
            };

            var strengths = new Dictionary<string, double>
            {
                { "Q235", 235 }, { "45", 355 }, { "35CrMo", 835 }, { "20CrMnTi", 835 }
            };

            var d = diameters.TryGetValue(size, out var diameter) ? diameter : 12;
            var sigma = strengths.TryGetValue(material, out var strength) ? strength : 355;

            var area = Math.PI * Math.Pow(d / 2, 2);
            var preload = 0.7 * sigma * area;
            var totalLoad = preload + load;
            var stress = totalLoad / area;
            var safety = sigma / stress;

            var result = new
            {
                size,
                material,
                diameter,
                area,
                yieldStrength = sigma,
                preload,
                workingLoad = load,
                totalLoad,
                stress,
                safety,
                isSafe = safety >= 1.5,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            return Ok(result);
        }

        [HttpPost("gear")]
        public IActionResult CalculateGear([FromBody] GearRequest request)
        {
            var m = request.Module;
            var z1 = request.Teeth1;
            var z2 = request.Teeth2;
            var power = request.Power;
            var speed = request.Speed;

            var ratio = (double)z2 / z1;
            var d1 = m * z1;
            var d2 = m * z2;
            var centerDistance = (d1 + d2) / 2;
            var torque = (power * 9550) / speed;

            var result = new
            {
                module = m,
                teeth1 = z1,
                teeth2 = z2,
                ratio,
                pitchDiameter1 = d1,
                pitchDiameter2 = d2,
                centerDistance,
                power,
                speed,
                torque,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            return Ok(result);
        }

        [HttpGet("materials")]
        public IActionResult GetMaterials()
        {
            var materials = new[]
            {
                new { name = "Q235", type = "碳素结构钢", tensileStrength = "375-460 MPa", yieldStrength = "235 MPa", modulus = "206 GPa", density = "7.85 g/cm³" },
                new { name = "45号钢", type = "优质碳素结构钢", tensileStrength = "600 MPa", yieldStrength = "355 MPa", modulus = "206 GPa", density = "7.85 g/cm³" },
                new { name = "35CrMo", type = "合金结构钢", tensileStrength = "980 MPa", yieldStrength = "835 MPa", modulus = "210 GPa", density = "7.85 g/cm³" },
                new { name = "20CrMnTi", type = "渗碳钢", tensileStrength = "1080 MPa", yieldStrength = "835 MPa", modulus = "210 GPa", density = "7.85 g/cm³" },
                new { name = "HT250", type = "灰铸铁", tensileStrength = "250 MPa", yieldStrength = "-", modulus = "130 GPa", density = "7.2 g/cm³" },
                new { name = "LY12", type = "硬铝", tensileStrength = "420 MPa", yieldStrength = "275 MPa", modulus = "71 GPa", density = "2.7 g/cm³" }
            };

            return Ok(materials);
        }

        [HttpPost("bearing")]
        public IActionResult SelectBearing([FromBody] BearingRequest request)
        {
            var inner = request.InnerDiameter;
            var outer = request.OuterDiameter;
            var load = request.Load;

            var bearings = new[]
            {
                new { model = "6206", inner = 30, outer = 62, width = 16, dynamicLoad = 19.5, staticLoad = 11.3 },
                new { model = "6207", inner = 35, outer = 72, width = 17, dynamicLoad = 25.5, staticLoad = 15.2 },
                new { model = "6208", inner = 40, outer = 80, width = 18, dynamicLoad = 32.0, staticLoad = 19.4 },
                new { model = "6306", inner = 30, outer = 72, width = 19, dynamicLoad = 30.5, staticLoad = 17.0 },
                new { model = "6307", inner = 35, outer = 80, width = 21, dynamicLoad = 36.5, staticLoad = 21.2 }
            };

            return Ok(new { bearings, timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
        }

        [HttpGet("tolerance")]
        public IActionResult GetTolerance()
        {
            var tolerances = new[]
            {
                new { basicSize = "3-6", hole = "H7", holeTolerance = "+0.012", shaft = "h6", shaftTolerance = "-0.008" },
                new { basicSize = "6-10", hole = "H7", holeTolerance = "+0.015", shaft = "h6", shaftTolerance = "-0.009" },
                new { basicSize = "10-18", hole = "H7", holeTolerance = "+0.018", shaft = "h6", shaftTolerance = "-0.011" },
                new { basicSize = "18-30", hole = "H7", holeTolerance = "+0.021", shaft = "h6", shaftTolerance = "-0.013" },
                new { basicSize = "30-50", hole = "H7", holeTolerance = "+0.025", shaft = "h6", shaftTolerance = "-0.016" },
                new { basicSize = "50-80", hole = "H7", holeTolerance = "+0.030", shaft = "h6", shaftTolerance = "-0.019" }
            };

            return Ok(tolerances);
        }

        public class BoltRequest
        {
            public string Size { get; set; } = "M12";
            public string Material { get; set; } = "45";
            public double WorkingLoad { get; set; } = 10000;
        }

        public class GearRequest
        {
            public double Module { get; set; } = 2;
            public int Teeth1 { get; set; } = 20;
            public int Teeth2 { get; set; } = 40;
            public double Power { get; set; } = 5;
            public double Speed { get; set; } = 1450;
        }

        public class BearingRequest
        {
            public double InnerDiameter { get; set; } = 30;
            public double OuterDiameter { get; set; } = 62;
            public double Load { get; set; } = 20;
        }
    }
}