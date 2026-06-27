namespace IndustrialToolkit.Services
{
    public class DebugToolInfoService
    {
        public List<DebugToolCategory> GetDebugTools()
        {
            return new List<DebugToolCategory>
            {
                new DebugToolCategory
                {
                    Name = "以太网协议调试工具",
                    Icon = "🌐",
                    Tools = new List<DebugToolInfo>
                    {
                        new DebugToolInfo
                        {
                            Name = "Modbus Poll",
                            Protocol = "Modbus TCP/RTU",
                            Description = "最经典的Modbus主站调试工具，支持TCP和RTU",
                            Manufacturer = "Witte Software",
                            DownloadUrl = "https://www.modbustools.com/modbus_poll.html",
                            Price = "付费（有试用版）",
                            Icon = "📊"
                        },
                        new DebugToolInfo
                        {
                            Name = "Modbus Slave",
                            Protocol = "Modbus TCP/RTU",
                            Description = "Modbus从站模拟器，用于测试主站设备",
                            Manufacturer = "Witte Software",
                            DownloadUrl = "https://www.modbustools.com/modbus_slave.html",
                            Price = "付费（有试用版）",
                            Icon = "🔧"
                        },
                        new DebugToolInfo
                        {
                            Name = "EIPScanner",
                            Protocol = "Ethernet/IP",
                            Description = "开源Ethernet/IP扫描和调试工具",
                            Manufacturer = "开源社区",
                            DownloadUrl = "https://github.com/EIPScanner/EIPScanner",
                            Price = "免费开源",
                            Icon = "💎"
                        },
                        new DebugToolInfo
                        {
                            Name = "RSLinx Classic",
                            Protocol = "Ethernet/IP",
                            Description = "罗克韦尔官方通讯配置和诊断工具",
                            Manufacturer = "Rockwell Automation",
                            DownloadUrl = "https://www.rockwellautomation.com/en-us/products/software/rslinx.html",
                            Price = "付费",
                            Icon = "🏭"
                        },
                        new DebugToolInfo
                        {
                            Name = "PRONETA",
                            Protocol = "Profinet",
                            Description = "西门子Profinet网络诊断和配置工具",
                            Manufacturer = "Siemens",
                            DownloadUrl = "https://support.industry.siemens.com/cs/document/67460624/proneta",
                            Price = "免费",
                            Icon = "🔍"
                        },
                        new DebugToolInfo
                        {
                            Name = "FinsGateway",
                            Protocol = "FinsTCP (Omron)",
                            Description = "欧姆龙Fins协议通讯中间件",
                            Manufacturer = "Omron",
                            DownloadUrl = "https://www.omron.com/global/en/",
                            Price = "免费",
                            Icon = "📡"
                        }
                    }
                },
                new DebugToolCategory
                {
                    Name = "现场总线调试工具",
                    Icon = "🔗",
                    Tools = new List<DebugToolInfo>
                    {
                        new DebugToolInfo
                        {
                            Name = "SIMATIC Step 7",
                            Protocol = "Profibus DP",
                            Description = "西门子PLC编程和Profibus DP组态工具",
                            Manufacturer = "Siemens",
                            DownloadUrl = "https://support.industry.siemens.com/cs/document/109794442/step-7-v5-6",
                            Price = "付费",
                            Icon = "⚙️"
                        },
                        new DebugToolInfo
                        {
                            Name = "RSNetWorx for DeviceNet",
                            Protocol = "DeviceNet",
                            Description = "罗克韦尔DeviceNet网络配置和诊断工具",
                            Manufacturer = "Rockwell Automation",
                            DownloadUrl = "https://www.rockwellautomation.com/en-us/products/software/rsnetworx.html",
                            Price = "付费",
                            Icon = "🔧"
                        },
                        new DebugToolInfo
                        {
                            Name = "GX Works3",
                            Protocol = "CC-Link",
                            Description = "三菱PLC编程和CC-Link配置工具",
                            Manufacturer = "Mitsubishi Electric",
                            DownloadUrl = "https://www.mitsubishielectric.com/fa/products/cnt/plc/software/gx_works/index.html",
                            Price = "付费",
                            Icon = "🎛️"
                        },
                        new DebugToolInfo
                        {
                            Name = "AS-i Control Tools",
                            Protocol = "AS-i",
                            Description = "Bihl+Wiedemann AS-i 配置和诊断工具",
                            Manufacturer = "Bihl+Wiedemann",
                            DownloadUrl = "https://www.bihl-wiedemann.com/en/products/software/as-i-control-tools/",
                            Price = "免费",
                            Icon = "📋"
                        }
                    }
                },
                new DebugToolCategory
                {
                    Name = "通用网络调试工具",
                    Icon = "🛠️",
                    Tools = new List<DebugToolInfo>
                    {
                        new DebugToolInfo
                        {
                            Name = "网络调试助手 NetAssist",
                            Protocol = "TCP/UDP/串口",
                            Description = "国产经典网络调试工具，支持TCP/UDP/串口",
                            Manufacturer = "国内开发者",
                            DownloadUrl = "https://www.cmsoft.cn/",
                            Price = "免费",
                            Icon = "🌐"
                        },
                        new DebugToolInfo
                        {
                            Name = "UaExpert",
                            Protocol = "OPC UA",
                            Description = "功能强大的OPC UA客户端测试工具",
                            Manufacturer = "Unified Automation",
                            DownloadUrl = "https://www.unified-automation.com/products/development-tools/uaexpert.html",
                            Price = "免费",
                            Icon = "🔐"
                        },
                        new DebugToolInfo
                        {
                            Name = "Wireshark",
                            Protocol = "网络抓包",
                            Description = "全球最流行的网络协议分析器，支持所有工业协议",
                            Manufacturer = "Wireshark Foundation",
                            DownloadUrl = "https://www.wireshark.org/",
                            Price = "免费开源",
                            Icon = "🦈"
                        },
                        new DebugToolInfo
                        {
                            Name = "XCOM (串口调试助手)",
                            Protocol = "串口",
                            Description = "格西烽火串口调试工具，功能强大",
                            Manufacturer = "格西烽火",
                            DownloadUrl = "http://www.geshe.com/",
                            Price = "免费",
                            Icon = "📟"
                        },
                        new DebugToolInfo
                        {
                            Name = "CANoe",
                            Protocol = "CAN/CANopen",
                            Description = "Vector公司CAN总线分析和仿真工具",
                            Manufacturer = "Vector Informatik",
                            DownloadUrl = "https://www.vector.com/cn/zh/products/products-a-z/software/canoe/",
                            Price = "付费",
                            Icon = "🚗"
                        },
                        new DebugToolInfo
                        {
                            Name = "CANtest",
                            Protocol = "CAN/CANopen",
                            Description = "周立功CAN总线测试工具",
                            Manufacturer = "周立功 (ZLG)",
                            DownloadUrl = "https://www.zlg.cn/",
                            Price = "免费",
                            Icon = "🔌"
                        }
                    }
                }
            };
        }
    }

    public class DebugToolCategory
    {
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = "🔧";
        public List<DebugToolInfo> Tools { get; set; } = new List<DebugToolInfo>();
    }

    public class DebugToolInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Protocol { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
        public string Icon { get; set; } = "🔧";
    }
}
