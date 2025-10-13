namespace BlockSenseAPI.Models.Token.Configs
{
    public class AccessTokenConfig
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Secret { get; set; } = string.Empty;
        public double AccessTokenExpirationMinutes { get; set; }
    }
}
