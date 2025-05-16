using BlockSenseAPI.Models.Token;

namespace BlockSenseAPI.Models.Requests
{
    public class TokenRefreshRequestModel
    {
        public RefreshTokenModel? RefreshToken { get; set; }
        public SystemIdentifierModel? Identifiers { get; set; }
    }
}
