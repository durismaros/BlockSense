namespace BlockSenseAPI.Models.TwoFactorAuth
{
    public class TwoFactorConfig
    {
        public string Issuer { get; set; } = string.Empty;
        public string MasterKey { get; set; } = string.Empty;
    }
}
