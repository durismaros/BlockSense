using BlockSense.Contracts.DTOs.Token;

namespace BlockSense.Desktop.Providers.Interfaces
{
    public interface IAccessTokenProvider
    {
        string Get();
        void Set(AccessTokenDto accessToken);
        void Clear();
    }
}
