namespace BlockSenseAPI.Models.TwoFactorAuth
{
    public class TwoFactorSetupRequestModel
    {
        public string? SecretKey { get; set; }
        public string? Code { get; set; }
    }
}
