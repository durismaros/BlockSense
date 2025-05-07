using BlockSense.Client.Cryptography.Hashing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Client
{
    public class SystemIdentifiers
    {
        public string DeviceId { get; }
        public string IpAddress { get; private set; }
        public string HardwareId { get; }
        public string MacAddress { get; }

        public SystemIdentifiers()
        {
            DeviceId = Environment.MachineName;
            GetIpAddress();
            HardwareId = GetHardwareId();
            MacAddress = GetMacAddress();
        }

        public async void GetIpAddress()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    IpAddress = await httpClient.GetStringAsync("https://api.ipify.org");
                }
                ConsoleHelper.Log("IP address acquired");
            }
            catch (Exception ex)
            {
                ConsoleHelper.Log($"Error: {ex.Message}");
            }
        }

        private static string GetHardwareId()
        {
            string cpuId = GetCpuId();
            string motherboardId = GetMotherboardId();
            string diskId = GetDiskId();

            string combinedIds = $"{cpuId}{motherboardId}{diskId}";
            byte[] hashBytes = HashUtils.ComputeSha256(Encoding.UTF8.GetBytes(combinedIds));

            return Convert.ToBase64String(hashBytes);
        }

        private static string GetMacAddress()
        {
            try
            {
                NetworkInterface[] allInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                NetworkInterface nic = null;

                foreach (NetworkInterface eachInterface in allInterfaces)
                {
                    if (eachInterface.OperationalStatus == OperationalStatus.Up &&
                        eachInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                        !eachInterface.Description.Contains("Virtual"))
                    {
                        nic = eachInterface;
                        break;
                    }
                }

                return nic?.GetPhysicalAddress().ToString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string GetCpuId()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
                foreach (var mo in searcher.Get().Cast<ManagementObject>())
                {
                    return mo["ProcessorId"]?.ToString() ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.Log("Error: " + ex.Message);
            }
            return string.Empty;
        }

        private static string GetMotherboardId()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
                foreach (var mo in searcher.Get().Cast<ManagementObject>())
                {
                    return mo["SerialNumber"]?.ToString() ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.Log("Error: " + ex.Message);
            }
            return string.Empty;
        }

        private static string GetDiskId()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_DiskDrive WHERE Index = 0");
                foreach (var mo in searcher.Get().Cast<ManagementObject>())
                {
                    return mo["SerialNumber"]?.ToString()?.Trim() ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.Log("Error: " + ex.Message);
            }
            return string.Empty;
        }
    }
}
