using BlockSense.Contracts.DTOs.Token;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Utilities.ApiHandling;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Providers.Implementations
{
    public sealed class AccessTokenProvider : IAccessTokenProvider
    {
        private string _accessToken;
        private DateTime _expiresAt;

        public event AccessTokenRefreshRequestedAsync? RefreshRequested;

        public AccessTokenProvider()
        {
            _accessToken = string.Empty;
            _expiresAt = DateTime.MinValue;
        }

        public async Task<string> GetAsync(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(_accessToken) && _expiresAt > DateTime.UtcNow)
            {
                return _accessToken;
            }

            if (RefreshRequested is null)
            {
                throw new AuthenticationRequiredException();
            }

            await RefreshRequested.Invoke(cancellationToken);
            
            return _accessToken;
        }

        public void Set(AccessTokenDto accessToken)
        {
            _accessToken = accessToken.Token;
            _expiresAt = accessToken.ExpiresAt;
        }
    }

    public delegate Task AccessTokenRefreshRequestedAsync(CancellationToken cancellationToken);
}
