using BlockSenseAPI.Models;
using BlockSenseAPI.Models.Token.DTOs;

namespace BlockSenseAPI.Services.SystemValidation
{
    public class SystemValidationService
    {
        private readonly SystemIdentifier _clientIdentifiers;
        private readonly SystemIdentifier _validIdentifiers;

        public SystemValidationService(SystemIdentifier clientIdents, SystemIdentifier fetchedIdents)
        {
            _clientIdentifiers = clientIdents;
            _validIdentifiers = fetchedIdents;
        }

        public TokenRefreshResponse GetResult()
        {
            // Hwid or Mac address changed, Immediate logout
            if (!CheckHardwareIdentifiers())
                return new TokenRefreshResponse
                {
                    Success = false,
                    Message = "Identifiers do not match"
                };

            // Ip or Geo change
            if (!CheckNetworkIdentifiers())
                // 2Fa implementation later ...
                return new TokenRefreshResponse
                {
                    Success = true,
                    Message = "IP change approved"
                };

            return new TokenRefreshResponse
            {
                Success = true,
                Message = "System indentifiers verified"
            };
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

            return false;
        }
    }
}
