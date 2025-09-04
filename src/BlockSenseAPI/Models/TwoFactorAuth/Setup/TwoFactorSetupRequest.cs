namespace BlockSenseAPI.Models.TwoFactorAuth.Setup
{
    public class TwoFactorSetupRequest
    {
        public string? SecretKey { get; set; }
        public string? Code { get; set; }
    }
}
