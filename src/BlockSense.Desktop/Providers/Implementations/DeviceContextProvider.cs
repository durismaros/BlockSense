using BlockSense.Contracts.Cryptography.Hashing;
using BlockSense.Desktop.Providers.Interfaces;
using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;

namespace BlockSense.Desktop.Providers.Implementations
{
    public sealed class DeviceContextProvider : IDeviceContextProvider
    {
        public string DeviceIdentifier
        {
            get;
            init;
        }

        public string DeviceOs
        {
            get;
            init;
        }

        public string HardwareFingerprint
        {
            get;
            init;
        }

        public string NetworkFingerprint
        {
            get;
            init;
        }

        public DeviceContextProvider()
        {
            DeviceIdentifier = Environment.MachineName;
            DeviceOs = GetDeviceOs();
            HardwareFingerprint = GetHardwareFingerprint();
            NetworkFingerprint = GetNetworkFingerprint();
        }

        private string GetDeviceOs()
        {
            var os =
                OperatingSystem.IsWindows() ? "Windows" :
                OperatingSystem.IsMacOS() ? "macOS" :
                "Unknown";

            var version = Environment.OSVersion.Version;
            var description = RuntimeInformation.OSDescription;

            return $"{os} {version.Major}.{version.Minor}.{version.Build} ({description})";
        }

        private string GetHardwareFingerprint()
        {
            var components = new[]
            {
                Environment.ProcessorCount.ToString(),
                GetHardwareIds()
            };

            var combined = string.Join('|', components);

            return Sha256Hasher.ComputeBase64(Encoding.UTF8.GetBytes(combined));
        }

        private string GetNetworkFingerprint()
        {
            var nic = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up &&
                            n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                            n.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                            n.Description.ToLower().Contains("virtual") == false &&
                            n.Name.ToLower().Contains("virtual") == false)
                .OrderByDescending(n => n.Speed)
                .FirstOrDefault();

            if (nic is null)
                return "Unknown";

            return string.Join(":", nic.GetPhysicalAddress()
                                       .GetAddressBytes()
                                       .Select(b => b.ToString("X2")));
        }

        private static string GetHardwareIds()
        {
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    // CPU ID
                    using var cpuSearcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
                    string? cpuId = cpuSearcher.Get().Cast<ManagementObject>()
                                            .Select(mo => mo["ProcessorId"]?.ToString())
                                            .FirstOrDefault();

                    // Motherboard Serial
                    using var boardSearcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
                    string? motherboardId = boardSearcher.Get().Cast<ManagementObject>()
                                            .Select(mo => mo["SerialNumber"]?.ToString())
                                            .FirstOrDefault();

                    // Disk Serial
                    using var diskSearcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_DiskDrive WHERE Index = 0");
                    string? diskId = diskSearcher.Get().Cast<ManagementObject>()
                                            .Select(mo => mo["SerialNumber"]?.ToString()?.Trim())
                                            .FirstOrDefault();

                    return string.Join('|', cpuId, motherboardId, diskId);
                }
                catch
                {
                    return string.Empty;
                }
            }

            else if (OperatingSystem.IsMacOS())
            {
                try
                {
                    string? cpuBrand = RunProcess("/usr/sbin/sysctl", "-n machdep.cpu.brand_string")?.Trim();
                    string? platformUuid = RunProcess("/usr/sbin/ioreg", "-rd1 -c IOPlatformExpertDevice | awk '/IOPlatformUUID/ { print $3 }'")?.Trim('\"');
                    string? diskUuid = RunProcess("/usr/sbin/diskutil", "info / | awk '/Volume UUID/ { print $3 }'")?.Trim();

                    return string.Join('|', cpuBrand, platformUuid, diskUuid);
                }
                catch
                {
                    return string.Empty;
                }
            }

            throw new PlatformNotSupportedException();
        }

        private static string? RunProcess(string fileName, string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                return process?.StandardOutput.ReadToEnd();
            }
            catch
            {
                return null;
            }
        }
    }
}