using BlockSense.Contracts.Definitions;
using BlockSense.Contracts.DTOs.Session;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Verification;
using BlockSense.Desktop.Models.Api;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Implementations
{
    public sealed class TokenService : ITokenService
    {
        private readonly IApiClient _apiClient;
        private readonly ISessionService _sessionService;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly TwoFactorSlidingPanel _twoFactorSlidingPanel;

        public TokenService(IApiClient apiClient, ISessionService sessionService, ICurrentUserProvider currentUserProvider)
        {
            _apiClient = apiClient
                ?? throw new ArgumentNullException(nameof(apiClient));

            _sessionService = sessionService
                ?? throw new ArgumentNullException(nameof(sessionService));

            _currentUserProvider = currentUserProvider
                ?? throw new ArgumentNullException(nameof(currentUserProvider));

            _twoFactorSlidingPanel = MainWindow.Instance.TwoFactorSlidingPanel
                ?? throw new ArgumentNullException(nameof(MainWindow.Instance.TwoFactorSlidingPanel));
        }

        public async Task RevokeAsync(SessionRevokeRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _apiClient
                .AddBearerToken()
                .DeleteAsync<SessionRevokeRequest, object>(
                    requestUri: "/api/users/me/sessions",
                    request: request,
                    cancellationToken: cancellationToken);

            switch (response)
            {
                case ApiResult<object>.Success:
                    _twoFactorSlidingPanel.HidePanel();

                    _currentUserProvider.SetActiveDevices(_currentUserProvider.ActiveDevices
                        .Where(d => d.TokenHash != request.TokenHash)
                        .ToList());

                    break;

                case ApiResult.Failure error:
                    await HandleRevokeFailureAsync(error, request, cancellationToken);
                    break;
            }
        }

        public async Task RevokeAllAsync(RevokeAllSessionsRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _apiClient
                .AddBearerToken()
                .DeleteAsync<RevokeAllSessionsRequest, object>(
                    requestUri: "/api/users/me/sessions/all",
                    request: request,
                    cancellationToken: cancellationToken);

            switch (response)
            {
                case ApiResult<object>.Success:
                    _twoFactorSlidingPanel.HidePanel();
                    await _sessionService.SignOutAsync();
                    break;

                case ApiResult.Failure error:
                    await HandleRevokeAllFailureAsync(error, cancellationToken);
                    break;
            }
        }

        private async Task HandleRevokeAllFailureAsync(ApiResult.Failure error, CancellationToken cancellationToken)
        {
            switch (error.ProblemDetails.Type)
            {
                case StandardizedCodes.Authentication.TwoFactorRequired:
                    _twoFactorSlidingPanel.ShowPanel(async code =>
                    {
                        await RevokeAllAsync(new RevokeAllSessionsRequest { TwoFactorCode = code }, cancellationToken);
                    });
                    break;

                case StandardizedCodes.TwoFactorAuthentication.Invalid:
                    await _twoFactorSlidingPanel.ShowErrorState();
                    break;

                default:
                    MainWindow.Instance.ShowNotification(
                        error.ProblemDetails.Title,
                        error.ProblemDetails.Detail);
                    break;
            }
        }

        private async Task HandleRevokeFailureAsync(
            ApiResult.Failure error,
            SessionRevokeRequest originalRequest,
            CancellationToken cancellationToken)
        {
            switch (error.ProblemDetails.Type)
            {
                case StandardizedCodes.Authentication.TwoFactorRequired:
                    _twoFactorSlidingPanel.ShowPanel(async code =>
                    {
                        await RevokeAsync(originalRequest with { TwoFactorCode = code }, cancellationToken);
                    });
                    break;

                case StandardizedCodes.TwoFactorAuthentication.Invalid:
                    await _twoFactorSlidingPanel.ShowErrorState();
                    break;

                default:
                    MainWindow.Instance.ShowNotification(
                        error.ProblemDetails.Title,
                        error.ProblemDetails.Detail);
                    break;
            }
        }
    }
}
