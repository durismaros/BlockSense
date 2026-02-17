using BlockSense.Contracts.DTOs.Token;
using System;

namespace BlockSense.Desktop.Providers.Interfaces
{
    public interface IAccessTokenProvider
    {
        string Get();
        void Set(AccessTokenDto accessToken);
        void Clear();
    }
}
