using BlockSense.Contracts.DTOs.Authentication;
using BlockSense.Contracts.DTOs.Token;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Providers.Implementations
{
    public sealed class AccessTokenProvider : IAccessTokenProvider
    {
        private readonly IAuthService _authService;
        private readonly IRefreshTokenProvider _refreshTokenProvider;

        private string _accessToken;
        private DateTime _expiresAt;

        public AccessTokenProvider(IAuthService authService, IRefreshTokenProvider refreshTokenProvider)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _refreshTokenProvider = refreshTokenProvider ?? throw new ArgumentNullException(nameof(refreshTokenProvider));

            _accessToken = string.Empty;
            _expiresAt = DateTime.MinValue;
        }

        public async Task<string> GetAsync(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(_accessToken) && _expiresAt > DateTime.UtcNow)
            {
                return _accessToken;
            }

            var refreshToken = await _refreshTokenProvider.GetAsync(cancellationToken);

            var request = new AuthRefreshRequest()
            {
                RefreshToken = refreshToken
            };

            var response = await _authService.AuthRefreshAsync(request, cancellationToken);

            _accessToken = response.AccessToken.Token;
            _expiresAt = response.AccessToken.ExpiresAt;

            await _refreshTokenProvider.SaveAsync(response.RefreshToken);

            return _accessToken;
        }

        public void Set(AccessTokenDto accessToken)
        {
            _accessToken = accessToken.Token;
            _expiresAt = accessToken.ExpiresAt;
        }
    }
}
