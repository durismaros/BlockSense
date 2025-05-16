using BlockSenseAPI.Models;

namespace BlockSenseAPI.Services.UserServices
{
    public class ValidatorService
    {
        private readonly SystemIdentifierModel _clientIdentifiers;
        private readonly SystemIdentifierModel _validIdentifiers;
        private readonly GeoLookupService _geoLookupService;

        public ValidatorService(SystemIdentifierModel clientIdents, SystemIdentifierModel fetchedIdents, GeoLookupService geoLookupService)
        {
            _clientIdentifiers = clientIdents;
            _validIdentifiers = fetchedIdents;
            _geoLookupService = geoLookupService;
        }
        public bool GetResult()
        {
            // Hwid or Mac address changed, Immediate logout
            if (!CheckHardwareIdentifiers())
                return false;

            // Ip or Geo change
            if (!CheckNetworkIdentifiers())
                return false;

            return true;
        }

        private bool CheckHardwareIdentifiers()
        {
            if (string.IsNullOrEmpty(_clientIdentifiers.HardwareId) || string.IsNullOrEmpty(_validIdentifiers.HardwareId) ||
                string.IsNullOrEmpty(_clientIdentifiers.MacAddress) || string.IsNullOrEmpty(_validIdentifiers.MacAddress))
                return false;

            if (_clientIdentifiers.HardwareId != _validIdentifiers.HardwareId)
                return false;

            if (_clientIdentifiers.MacAddress != _validIdentifiers.MacAddress)
                return false;

            return true;
        }

        private bool CheckNetworkIdentifiers()
        {
            if (string.IsNullOrEmpty(_clientIdentifiers.IpAddress) || string.IsNullOrEmpty(_validIdentifiers.IpAddress))
                return false;

            // IP changes are allowed
            if (_clientIdentifiers.IpAddress == _validIdentifiers.IpAddress)
                return true;

            // Geolocation check
            var currentCountry = _geoLookupService.GetCountry(_clientIdentifiers.IpAddress);
            var storedCountry = _geoLookupService.GetCountry(_validIdentifiers.IpAddress);

            return currentCountry == storedCountry;
        }
    }
}
