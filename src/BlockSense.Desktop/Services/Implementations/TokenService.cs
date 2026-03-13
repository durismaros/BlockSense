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
    /// <summary>
    /// Implements <see cref="ITokenService"/> to manage session token revocation
    /// for the current user, including single and bulk revocation with two-factor authentication support.
    /// </summary>
    public sealed class TokenService : ITokenService
    {
        private readonly IApiClient _apiClient;
        private readonly ISessionService _sessionService;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly TwoFactorSlidingPanel _twoFactorSlidingPanel;

        /// <summary>
        /// Initializes a new instance of <see cref="TokenService"/>.
        /// </summary>
        /// <param name="apiClient">The API client used to communicate with the backend.</param>
        /// <param name="sessionService">The session service used to sign the user out after all sessions are revoked.</param>
        /// <param name="currentUserProvider">The provider for accessing and updating the current user's state.</param>
        /// <exception cref="ArgumentNullException">Thrown if any required dependency is null.</exception>
        public TokenService(
            IApiClient apiClient,
            ISessionService sessionService,
            ICurrentUserProvider currentUserProvider)
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

        /// <inheritdoc/>
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
                    HandleRevokeSuccess(request);
                    break;

                case ApiResult.Failure failure:
                    await HandleRevokeFailureAsync(failure, request, cancellationToken);
                    break;
            }
        }

        /// <inheritdoc/>
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
                    await _sessionService.SignOutAsync(cancellationToken);
                    break;

                case ApiResult.Failure failure:
                    await HandleRevokeAllFailureAsync(failure, cancellationToken);
                    break;
            }
        }

        private void HandleRevokeSuccess(SessionRevokeRequest request)
        {
            _twoFactorSlidingPanel.HidePanel();

            var updatedDevices = _currentUserProvider.ActiveDevices
                .Where(device => device.TokenHash != request.TokenHash)
                .ToList();

            _currentUserProvider.SetActiveDevices(updatedDevices);
        }

        private async Task HandleRevokeFailureAsync(
            ApiResult.Failure failure,
            SessionRevokeRequest originalRequest,
            CancellationToken cancellationToken)
        {
            switch (failure.ProblemDetails.Type)
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
                    ShowErrorNotification(failure);
                    break;
            }
        }

        private async Task HandleRevokeAllFailureAsync(ApiResult.Failure failure, CancellationToken cancellationToken)
        {
            switch (failure.ProblemDetails.Type)
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
                    ShowErrorNotification(failure);
                    break;
            }
        }

        private static void ShowErrorNotification(ApiResult.Failure failure)
        {
            MainWindow.Instance.ShowNotification(
                failure.ProblemDetails.Title,
                failure.ProblemDetails.Detail);
        }
    }
}