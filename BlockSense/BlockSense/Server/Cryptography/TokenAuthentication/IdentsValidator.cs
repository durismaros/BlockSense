using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MaxMind.GeoIP2;
using System.Threading.Tasks;
using BlockSense.Server.Utilities;

namespace BlockSense.Server.Cryptography.TokenAuthentication
{
    class IdentsValidator
    {
        // Current user credentials
        public string CurrentHwid { get; set; }
        public string CurrentMacAddress { get; set; }
        public string CurrentIpAddress { get; set; }

        // Stored DB credentials
        public string StoredHwid { get; set; }
        public string StoredMacAddress { get; set; }
        public string StoredIpAddress { get; set; }

        public bool GetResult()
        {
            // HWID changed, Immediate logout
            if (!CheckHardwareIdentifiers())
                return false;

            // Ip or Geo change
            if (!CheckNetworkIdentifiers())
                return false;

            return true;
        }

        private bool CheckHardwareIdentifiers()
        {
            if (CurrentHwid != StoredHwid)
                return false;

            if (CurrentMacAddress != StoredMacAddress)
                return false;

            return true;
        }

        private bool CheckNetworkIdentifiers()
        {
            // IP changes are allowed but checked for anomalies
            if (CurrentIpAddress == StoredIpAddress)
                return true;

            // Geolocation check (e.g., same country)
            var currentCountry = IpGeoLookup.GetCountry(CurrentIpAddress);
            var storedCountry = IpGeoLookup.GetCountry(StoredIpAddress);

            return currentCountry == storedCountry;
        }
    }
}
