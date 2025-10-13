namespace BlockSense.Models.TwoFactorAuth.Setup
{
    public class TwoFactorSetupResponse
    {
        public string? SetupKey { get; set; } 
        public byte[]? QRCodeData { get; set; }
    }
}
