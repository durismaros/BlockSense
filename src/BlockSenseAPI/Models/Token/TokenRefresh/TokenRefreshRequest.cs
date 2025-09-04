namespace BlockSenseAPI.Models.Token.DTOs
{
    public class TokenRefreshRequest
    {
        public RefreshToken? RefreshToken { get; set; }
        public SystemIdentifier? Identifiers { get; set; }
    }
}
