using Microsoft.Extensions.Logging;
using System.IO.Ports;
using System.Management;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace IndustrialToolkit.Services
{
    public class SerialPortDetectorService
    {
        private readonly ILogger<SerialPortDetectorService> _logger;

        public SerialPortDetectorService(ILogger<SerialPortDetectorService> logger)
        {
            _logger = logger;
        }

        public SerialPortInfo GetSerialPortInfo()
        {
            var result = new SerialPortInfo
            {
                Ports = new List<string>(),
                Drivers = new List<SerialDriverInfo>()
            };

            try
            {
                result.Ports = SerialPort.GetPortNames().ToList();
                result.HasSerialPorts = result.Ports.Count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"获取串口列表失败: {ex.Message}");
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    result.Drivers = GetInstalledSerialDrivers();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"获取串口驱动信息失败: {ex.Message}");
                }
            }

            result.CommonDrivers = GetCommonDrivers();

            return result;
        }

        private List<SerialDriverInfo> GetInstalledSerialDrivers()
        {
            var drivers = new List<SerialDriverInfo>();

            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT * FROM Win32_SerialPort");
                foreach (var obj in searcher.Get())
                {
                    var driver = new SerialDriverInfo
                    {
                        Name = obj["Name"]?.ToString() ?? string.Empty,
                        Description = obj["Description"]?.ToString() ?? string.Empty,
                        DeviceId = obj["DeviceID"]?.ToString() ?? string.Empty,
                        Provider = obj["ProviderName"]?.ToString() ?? string.Empty,
                        IsInstalled = true
                    };
                    drivers.Add(driver);
                }

                using var searcher2 = new ManagementObjectSearcher(
                    "SELECT * FROM Win32_PnPEntity WHERE PNPDeviceID LIKE '%USB%' AND Name LIKE '%COM%'");
                foreach (var obj in searcher2.Get())
                {
                    var name = obj["Name"]?.ToString() ?? string.Empty;
                    var pnpDeviceId = obj["PNPDeviceID"]?.ToString() ?? string.Empty;

                    if (!drivers.Any(d => d.Name == name))
                    {
                        var driver = new SerialDriverInfo
                        {
                            Name = name,
                            Description = name,
                            DeviceId = pnpDeviceId,
                            Provider = "USB Serial",
                            IsInstalled = true
                        };
                        drivers.Add(driver);
                    }
                }
            }
            catch
            {
            }

            return drivers;
        }

        private List<CommonDriverInfo> GetCommonDrivers()
        {
            return new List<CommonDriverInfo>
            {
                new CommonDriverInfo
                {
                    ChipName = "CH340 / CH341",
                    Manufacturer = "南京沁恒",
                    Description = "最常用的USB转串口芯片，价格低廉，国产芯片",
                    DownloadUrl = "http://www.wch-ic.com/downloads/CH341SER_ZIP.html",
                    SupportedOs = "Windows XP/7/8/10/11",
                    Icon = "🔌"
                },
                new CommonDriverInfo
                {
                    ChipName = "CP2102 / CP210x",
                    Manufacturer = "Silicon Labs",
                    Description = "芯科科技的USB转UART芯片，稳定性好",
                    DownloadUrl = "https://www.silabs.com/developers/usb-to-uart-bridge-vcp-drivers",
                    SupportedOs = "Windows XP/7/8/10/11, macOS, Linux",
                    Icon = "💎"
                },
                new CommonDriverInfo
                {
                    ChipName = "PL2303",
                    Manufacturer = "Prolific (旺玖)",
                    Description = "台湾旺玖的USB转串口芯片，老设备常用",
                    DownloadUrl = "http://www.prolific.com.tw/US/ShowProduct.aspx?p_id=225&pcid=41",
                    SupportedOs = "Windows XP/7/8/10/11",
                    Icon = "🔗"
                },
                new CommonDriverInfo
                {
                    ChipName = "FT232RL / FTDI",
                    Manufacturer = "FTDI (Future Technology Devices)",
                    Description = "英国FTDI公司，业界公认最稳定的USB转串口方案",
                    DownloadUrl = "https://ftdichip.com/drivers/vcp-drivers/",
                    SupportedOs = "Windows, macOS, Linux, Android",
                    Icon = "🏆"
                },
                new CommonDriverInfo
                {
                    ChipName = "CH9102 / CH9101",
                    Manufacturer = "南京沁恒",
                    Description = "沁恒新一代USB转串口芯片，低功耗",
                    DownloadUrl = "http://www.wch-ic.com/downloads/CH9102_WIN_ZIP.html",
                    SupportedOs = "Windows 7/8/10/11",
                    Icon = "⚡"
                },
                new CommonDriverInfo
                {
                    ChipName = "虚拟串口 (VSPD)",
                    Manufacturer = "Eltima",
                    Description = "软件虚拟串口，用于调试串口程序",
                    DownloadUrl = "https://www.eltima.com/products/vspdxp/",
                    SupportedOs = "Windows XP/7/8/10/11",
                    Icon = "💻"
                }
            };
        }
    }

    public class SerialPortInfo
    {
        public bool HasSerialPorts { get; set; }
        public List<string> Ports { get; set; } = new List<string>();
        public List<SerialDriverInfo> Drivers { get; set; } = new List<SerialDriverInfo>();
        public List<CommonDriverInfo> CommonDrivers { get; set; } = new List<CommonDriverInfo>();
    }

    public class SerialDriverInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public bool IsInstalled { get; set; }
    }

    public class CommonDriverInfo
    {
        public string ChipName { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public string SupportedOs { get; set; } = string.Empty;
        public string Icon { get; set; } = "🔌";
    }
}
