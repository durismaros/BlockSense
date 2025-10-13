namespace BlockSenseAPI.Models.Token.DTOs
{
    public class TokenRefreshResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public AccessToken? AccessToken { get; set; }
    }
}
