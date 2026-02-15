using BlockSense.Contracts.Definitions;
using BlockSense.Contracts.DTOs.Authentication;
using BlockSense.Desktop.Models.Api;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.ApiHandling;
using BlockSense.Desktop.Utilities.UIComponents;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Implementations
{
    /// <summary>
    /// Implements <see cref="IAuthService"/> to handle user authentication by communicating with the backend API via <see cref="IApiClient"/>.
    /// </summary>
    public sealed class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly IApiClient _apiClient;
        private readonly NavigationManager _navigationManager;
        private readonly IAccessTokenProvider _accessTokenProvider;
        private readonly IRefreshTokenProvider _refreshTokenProvider;
        private readonly TwoFactorSlidingPanel _twoFactorSlidingPanel;

        /// <summary>
        /// Initializes a new instance of <see cref="AuthService"/>.
        /// </summary>
        /// <param name="logger">Logger for capturing authentication-related events.</param>
        /// <param name="apiClient">The API client used to send authentication requests.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="apiClient"/> or <paramref name="logger"/> is null.</exception>
        public AuthService(
            ILogger<AuthService> logger,
            IApiClient apiClient,
            NavigationManager navigationManager,
            IAccessTokenProvider accessTokenProvider,
            IRefreshTokenProvider refreshTokenProvider)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _apiClient = apiClient
                ?? throw new ArgumentNullException(nameof(apiClient));
            _navigationManager = navigationManager
                ?? throw new ArgumentNullException(nameof(navigationManager));
            _accessTokenProvider = accessTokenProvider
                ?? throw new ArgumentNullException(nameof(accessTokenProvider));
            _refreshTokenProvider = refreshTokenProvider
                ?? throw new ArgumentNullException(nameof(refreshTokenProvider));
            _twoFactorSlidingPanel = MainWindow.Instance.TwoFactorSlidingPanel
                ?? throw new ArgumentNullException(nameof(MainWindow.Instance.TwoFactorSlidingPanel));

            _accessTokenProvider.RefreshRequested += AuthRefreshAsync;
        }

        /// <inheritdoc/>
        public async Task AuthAsync(AuthRequest request, CancellationToken cancellationToken = default)
        {
            var delayTask = Task.Delay(1000, cancellationToken);
            var authTask = _apiClient
                .AddDeviceHeaders()
                .PostAsync<AuthRequest, AuthResponse>(
                    requestUri: "/api/auth",
                    request: request,
                    cancellationToken: cancellationToken);

            // Wait for whichever finishes first
            var completedTask = await Task.WhenAny(authTask, delayTask);

            if (completedTask == delayTask)
            {
                MainWindow.Instance.ShowNotification(
                    "Authentication",
                    "Your request is being processed. Please wait.");
            }

            var response = await authTask;

            switch (response)
            {
                case ApiResult<AuthResponse>.Success success:

                    // Save tokens
                    await _refreshTokenProvider.SaveAsync(success.Data.RefreshToken);
                    _accessTokenProvider.Set(success.Data.AccessToken);

                    // Hide 2FA panel if visible
                    _twoFactorSlidingPanel.HidePanel();

                    // Notify success
                    MainWindow.Instance.ShowNotification(
                        "Authentication",
                        "You've been successfully authenticated.");

                    await Task.Delay(2000);

                    // Navigate to home
                    await _navigationManager.NavigateToAsync<HomeView>();
                    break;

                case ApiResult.Failure failure:

                    // Handle specific 2FA scenarios
                    switch (failure.ProblemDetails.Type)
                    {
                        case StandardizedCodes.Authentication.TwoFactorRequired:
                            _twoFactorSlidingPanel.ShowPanel(async code =>
                            {
                                await AuthAsync(request with { TwoFactorCode = code }, cancellationToken);
                            });
                            break;

                        case StandardizedCodes.TwoFactorAuthentication.Invalid:
                            await _twoFactorSlidingPanel.ShowErrorState();
                            break;

                        default:
                            MainWindow.Instance.ShowNotification(
                                failure.ProblemDetails.Title,
                                failure.ProblemDetails.Detail);
                            break;
                    }
                    break;
            }
        }

        public async Task AuthRefreshAsync(CancellationToken cancellationToken = default)
        {
            var refreshToken = await _refreshTokenProvider.GetAsync(cancellationToken);

            var request = new AuthRefreshRequest
            {
                RefreshToken = refreshToken
            };

            var response = await _apiClient
                .AddDeviceHeaders()
                .PostAsync<AuthRefreshRequest, AuthRefreshResponse>(
                    requestUri: "/api/auth/refresh",
                    request: request,
                    cancellationToken: cancellationToken);

            if (response is ApiResult<AuthRefreshResponse>.Success success)
            {
                _accessTokenProvider.Set(success.Data.AccessToken);

                MainWindow.Instance.ShowNotification(
                    "Authentication Extended",
                    "Your access token has been successfully refreshed.");
                return;
            }

            throw new AuthenticationRequiredException();
        }
    }
}
