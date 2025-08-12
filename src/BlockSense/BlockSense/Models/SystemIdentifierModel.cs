using BlockSense.Cryptography.Hashing;
using BlockSense.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Models
{
    public class SystemIdentifierModel
    {
        public string? DeviceId { get; private set; }
        public string? IpAddress { get; private set; }
        public string? HardwareId { get; private set; }
        public string? MacAddress { get; private set; }

        public SystemIdentifierModel()
        {
            DeviceId = Environment.MachineName;
            IpAddress = GetIpAddress();
            HardwareId = GetHardwareId();
            MacAddress = GetMacAddress();
            ConsoleLogger.Log("Hardware and Network identifiers acquired");
        }

        public static string? GetIpAddress()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    return httpClient.GetStringAsync("https://api.ipify.org").GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                ConsoleLogger.Log($"Error: {ex.Message}");
                return null;
            }
        }

        private static string GetHardwareId()
        {
            string? cpuId = GetCpuId();
            string? motherboardId = GetMotherboardId();
            string? diskId = GetDiskId();

            string combinedIds = $"{cpuId}{motherboardId}{diskId}";
            byte[] hashBytes = HashingFunctions.ComputeSha256(Encoding.UTF8.GetBytes(combinedIds));

            return Convert.ToBase64String(hashBytes);
        }

        private static string? GetMacAddress()
        {
            try
            {
                NetworkInterface[] allInterfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface eachInterface in allInterfaces)
                {
                    if (eachInterface.OperationalStatus == OperationalStatus.Up && eachInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback && !eachInterface.Description.Contains("Virtual"))
                        return eachInterface.GetPhysicalAddress().ToString();
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                ConsoleLogger.Log($"Error: {ex.Message}");
                return null;
            }
        }

        private static string? GetCpuId()
        {
            try
            {
                if (!OperatingSystem.IsWindows())
                    throw new PlatformNotSupportedException("Hardware Identifiers are only supported on Windows");

                using var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
                foreach (var mo in searcher.Get().Cast<ManagementObject>())
                {
                    return mo["ProcessorId"]?.ToString();
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                ConsoleLogger.Log("Error: " + ex.Message);
                return null;
            }
        }

        private static string? GetMotherboardId()
        {
            try
            {
                if (!OperatingSystem.IsWindows())
                    throw new PlatformNotSupportedException("Hardware Identifiers are only supported on Windows");

                using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
                foreach (var mo in searcher.Get().Cast<ManagementObject>())
                {
                    return mo["SerialNumber"]?.ToString();
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                ConsoleLogger.Log("Error: " + ex.Message);
                return null;
            }
        }

        private static string? GetDiskId()
        {
            try
            {
                if (!OperatingSystem.IsWindows())
                    throw new PlatformNotSupportedException("Hardware Identifiers are only supported on Windows");

                using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_DiskDrive WHERE Index = 0");
                foreach (var mo in searcher.Get().Cast<ManagementObject>())
                {
                    return mo["SerialNumber"]?.ToString()?.Trim();
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                ConsoleLogger.Log("Error: " + ex.Message);
                return null;
            }
        }
    }
}
