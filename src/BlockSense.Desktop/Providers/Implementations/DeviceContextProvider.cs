using BlockSense.Contracts.Cryptography.Hashing;
using BlockSense.Desktop.Providers.Interfaces;
using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
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
            DeviceOs = BuildOsDescription();
            HardwareFingerprint = BuildHardwareFingerprint();
            NetworkFingerprint = BuildNetworkFingerprint();
        }

        private string BuildOsDescription()
        {
            var os =
                OperatingSystem.IsWindows() ? "Windows" :
                OperatingSystem.IsMacOS() ? "MacOS" :
                "Unknown";

            var version = Environment.OSVersion.Version;
            var description = RuntimeInformation.OSDescription;

            return $"{os} {version.Major}.{version.Minor}.{version.Build}";
        }

        private static string BuildHardwareFingerprint()
        {
            var raw = string.Join('|',
                Environment.ProcessorCount,
                GetHardwareIds());

            return Sha256Hasher.ComputeBase64(
                Encoding.UTF8.GetBytes(raw));
        }

        private static string GetHardwareIds()
        {
            if (OperatingSystem.IsWindows())
                return GetWindowsHardwareIds();

            if (OperatingSystem.IsMacOS())
                return GetMacHardwareIds();

            return string.Empty;
        }

        [SupportedOSPlatform("windows")]
        private static string GetWindowsHardwareIds()
        {
            try
            {
                string? cpu = QueryWmi("SELECT ProcessorId FROM Win32_Processor", "ProcessorId");
                string? board = QueryWmi("SELECT SerialNumber FROM Win32_BaseBoard", "SerialNumber");
                string? disk = QueryWmi("SELECT SerialNumber FROM Win32_DiskDrive WHERE Index = 0", "SerialNumber");

                return string.Join('|', cpu, board, disk);
            }
            catch
            {
                return string.Empty;
            }
        }

        [SupportedOSPlatform("windows")]
        private static string? QueryWmi(string query, string property)
        {
            using var searcher = new ManagementObjectSearcher(query);
            return searcher.Get()
                           .Cast<ManagementObject>()
                           .Select(mo => mo[property]?.ToString()?.Trim())
                           .FirstOrDefault();
        }

        [SupportedOSPlatform("macos")]
        private static string GetMacHardwareIds()
        {
            try
            {
                string? cpu = RunProcess("/usr/sbin/sysctl", "-n machdep.cpu.brand_string")?.Trim();

                string? platform = RunProcess("/usr/sbin/ioreg",
                    "-rd1 -c IOPlatformExpertDevice")?
                    .Split('\n')
                    .FirstOrDefault(l => l.Contains("IOPlatformUUID"))?
                    .Split('"')
                    .ElementAtOrDefault(3);

                string? disk = RunProcess("/usr/sbin/diskutil", "info /")?
                    .Split('\n')
                    .FirstOrDefault(l => l.Contains("Volume UUID"))?
                    .Split(':')
                    .ElementAtOrDefault(1)?
                    .Trim();

                return string.Join('|', cpu, platform, disk);
            }
            catch
            {
                return string.Empty;
            }
        }

        [SupportedOSPlatform("macos")]
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

        private static string BuildNetworkFingerprint()
        {
            var nic = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n =>
                    n.OperationalStatus == OperationalStatus.Up &&
                    n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    n.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                    !n.Description.Contains("virtual", StringComparison.OrdinalIgnoreCase) &&
                    !n.Name.Contains("virtual", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(n => n.Speed)
                .FirstOrDefault();

            if (nic == null)
            {
                return "Unknown";
            }

            return string.Join(":",
                nic.GetPhysicalAddress()
                   .GetAddressBytes()
                   .Select(b => b.ToString("X2")));
        }
    }
}