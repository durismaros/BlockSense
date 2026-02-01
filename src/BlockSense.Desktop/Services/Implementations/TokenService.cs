using BlockSense.Contracts.DTOs.Session;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Verification;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Implementations
{
    public sealed class TokenService : ITokenService
    {
        private readonly IApiClient _apiClient;
        private readonly ICurrentUserProvider _currentUserProvider;

        public TokenService(IApiClient apiClient)
        {
            _apiClient = apiClient
                ?? throw new ArgumentNullException(nameof(apiClient));
        }

        public async Task<bool> RevokeAsync(SessionRevokeRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _apiClient
                .AddBearerToken()
                .DeleteAsync<SessionRevokeRequest, object>(
                    requestUri: "/api/users/me/sessions",
                    request: request,
                    cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> RevokeAllAsync(TwoFactorVerificationRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _apiClient
                .AddBearerToken()
                .DeleteAsync<TwoFactorVerificationRequest, object>(
                    requestUri: "/api/users/me/sessions",
                    request: request,
                    cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return true;
            }

            return false;
        }
    }
}
