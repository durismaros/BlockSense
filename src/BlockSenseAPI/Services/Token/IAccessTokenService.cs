using BlockSenseAPI.Models.Token;

namespace BlockSenseAPI.Services.Token
{
    public interface IAccessTokenService
    {
        AccessToken GenerateAccessToken(int userId);
    }
}
