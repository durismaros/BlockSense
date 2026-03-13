using System.Net;
using System.Net.Sockets;

namespace BlockSense.Backend.Utilities
{
    /// <summary>
    /// Provides utility methods for masking IP addresses to protect user privacy in logs and responses.
    /// </summary>
    public static class IpAddressMasker
    {
        /// <summary>
        /// Masks the last octet (IPv4) or segment (IPv6) of an IP address string.
        /// Returns the original string if it cannot be parsed as a valid IP address.
        /// </summary>
        /// <param name="ipString">The IP address string to mask.</param>
        /// <returns>A masked representation of the IP address.</returns>
        public static string Mask(string ipString)
        {
            if (!IPAddress.TryParse(ipString, out var ip))
                return ipString;

            if (ip.IsIPv4MappedToIPv6)
                ip = ip.MapToIPv4();

            return ip.AddressFamily switch
            {
                AddressFamily.InterNetwork => MaskIpv4(ip),
                AddressFamily.InterNetworkV6 => MaskIpv6(ip),
                _ => ipString
            };
        }

        private static string MaskIpv4(IPAddress ip)
        {
            var bytes = ip.GetAddressBytes();
            return $"{bytes[0]}.{bytes[1]}.{bytes[2]}.*";
        }

        private static string MaskIpv6(IPAddress ip)
        {
            var bytes = ip.GetAddressBytes();
            return $"{bytes[0]}:{bytes[1]}:{bytes[2]}:{bytes[3]}:*";
        }
    }
}