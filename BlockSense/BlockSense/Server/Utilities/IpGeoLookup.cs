using MaxMind.GeoIP2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Server.Utilities
{
    static class IpGeoLookup
    {
        private static readonly DatabaseReader _reader = new DatabaseReader(@"C:\Users\d3str\Desktop\School\BlockSense\BlockSense\BlockSense\Assets\GeoLite2-Country.mmdb"); // Download from MaxMind

        public static string GetCountry(string ipAddress)
        {
            try
            {
                var location = _reader.Country(ipAddress);
                return location.Country.IsoCode;
            }
            catch
            {
                return null; // Unknown/private IP
            }
        }
    }
}
