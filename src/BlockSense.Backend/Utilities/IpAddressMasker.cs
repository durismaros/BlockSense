using System.Net;
using System.Net.Sockets;

namespace BlockSense.Backend.Utilities
{
    public static class IpAddressMasker
    {
        public static string Mask(string ipString)
        {
            if (IPAddress.TryParse(ipString, out var ip) == false)
            {
                return ipString;
            }

            if (ip.IsIPv4MappedToIPv6)
            {
                ip = ip.MapToIPv4();
            }

            switch (ip.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    return MaskIpv4(ip);

                case AddressFamily.InterNetworkV6:
                    return MaskIpv6(ip);

                default:
                    return ipString;
            }
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
