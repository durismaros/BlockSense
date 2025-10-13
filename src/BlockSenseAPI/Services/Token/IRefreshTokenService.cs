using BlockSenseAPI.Models.Token;
using BlockSenseAPI.Models.Token.DTOs;

namespace BlockSenseAPI.Services.Token
{
    public interface IRefreshTokenService
    {
        RefreshToken GenerateRefreshToken(int userId);
        Task StoreRefreshToken(TokenRefreshRequest request);
        Task<TokenRefreshResponse?> RefreshAccessToken(TokenRefreshRequest request);
        Task RevokeRefreshToken(Guid tokenId);
    }
}
