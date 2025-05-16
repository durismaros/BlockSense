using QRCoder;

namespace BlockSenseAPI.Models.TwoFactorAuth
{
    public class TwoFactorSetupResponseModel
    {
        public string? SetupKey { get; set; } 
        public byte[]? QRCodeData { get; set; }
    }
}
