using BlockSense.Contracts.Definitions;
using BlockSense.Contracts.DTOs.Authentication;
using BlockSense.Desktop.Models.Api;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.ApiHandling.Exceptions;
using BlockSense.Desktop.Utilities.UIComponents;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Implementations
{
    /// <summary>
    /// Implements <see cref="ISessionService"/> to manage the user's authenticated session lifecycle,
    /// including initialization, establishment, token refresh, and sign-out.
    /// Schedules automatic access token refresh before expiry.
    /// </summary>
    public sealed class SessionService : ISessionService, IDisposable
    {
        private readonly ILogger<SessionService> _logger;
        private readonly IApiClient _apiClient;
        private readonly IUserService _userService;
        private readonly IRefreshTokenProvider _refreshTokenProvider;
        private readonly IAccessTokenProvider _accessTokenProvider;
        private readonly NavigationManager _navigationManager;

        private Timer? _tokenRefreshTimer;

        /// <summary>
        /// Initializes a new instance of <see cref="SessionService"/>.
        /// </summary>
        /// <param name="logger">The logger used to record session lifecycle events.</param>
        /// <param name="apiClient">The API client used to send token refresh requests.</param>
        /// <param name="userService">The user service used to load the current user's data after session establishment.</param>
        /// <param name="refreshTokenProvider">The provider used to persist and retrieve the refresh token.</param>
        /// <param name="accessTokenProvider">The provider used to store the in-memory access token.</param>
        /// <param name="navigationManager">The navigation manager used to redirect the user between views.</param>
        /// <exception cref="ArgumentNullException">Thrown if any required dependency is null.</exception>
        public SessionService(
            ILogger<SessionService> logger,
            IApiClient apiClient,
            IUserService userService,
            IRefreshTokenProvider refreshTokenProvider,
            IAccessTokenProvider accessTokenProvider,
            NavigationManager navigationManager)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));

            _apiClient = apiClient
                ?? throw new ArgumentNullException(nameof(apiClient));

            _userService = userService
                ?? throw new ArgumentNullException(nameof(userService));

            _refreshTokenProvider = refreshTokenProvider
                ?? throw new ArgumentNullException(nameof(refreshTokenProvider));

            _accessTokenProvider = accessTokenProvider
                ?? throw new ArgumentNullException(nameof(accessTokenProvider));

            _navigationManager = navigationManager
                ?? throw new ArgumentNullException(nameof(navigationManager));
        }

        /// <inheritdoc/>
        public async Task InitializeSessionAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Initializing user session");

            if (!_refreshTokenProvider.Exists())
            {
                _logger.LogInformation("No refresh token found on disk — navigating to WelcomeView");
                await _navigationManager.NavigateToAsync<WelcomeView>();
                return;
            }

            _logger.LogInformation("Refresh token found — attempting to restore session");

            var refreshed = await RefreshAccessTokenAsync(cancellationToken);

            if (refreshed)
            {
                await _userService.LoadCurrentUserAsync(cancellationToken);

                _logger.LogInformation("Session restored successfully — navigating to HomeView");
                await _navigationManager.NavigateToAsync<HomeView>();
                return;
            }

            _logger.LogWarning("Session restoration failed — navigating to AuthenticationView");
            await _navigationManager.NavigateToAsync<AuthenticationView>();
        }

        /// <inheritdoc/>
        public async Task EstablishSessionAsync(AuthResponse response, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Establishing new user session");

            await _refreshTokenProvider.SaveAsync(response.RefreshToken, cancellationToken);
            _accessTokenProvider.Set(response.AccessToken);

            await _userService.LoadCurrentUserAsync(cancellationToken);
            ScheduleTokenRefresh(response.AccessToken.ExpiresAt);

            MainWindow.Instance.ShowNotification("Welcome Back", "Signed in successfully.");
            _logger.LogInformation("User session established successfully");

            await Task.Delay(1000, cancellationToken);
            await _navigationManager.NavigateToAsync<HomeView>();
        }

        /// <inheritdoc/>
        public async Task<bool> RefreshAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to refresh access token");

            string refreshToken;

            try
            {
                refreshToken = await _refreshTokenProvider.GetAsync(cancellationToken);
            }
            catch (AuthenticationRequiredException)
            {
                _logger.LogWarning("Refresh token is missing or has expired");
                return false;
            }

            var response = await _apiClient
                .AddDeviceHeaders()
                .PostAsync<AuthRefreshRequest, AuthRefreshResponse>(
                    requestUri: "/api/auth/refresh",
                    request: new AuthRefreshRequest { RefreshToken = refreshToken },
                    cancellationToken: cancellationToken);

            switch (response)
            {
                case ApiResult<AuthRefreshResponse>.Success success:
                    HandleTokenRefreshSuccess(success.Data, cancellationToken);
                    return true;

                case ApiResult.Failure failure:
                    HandleTokenRefreshFailure(failure);
                    return false;
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task SignOutAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Signing out current user");

            ClearSession();
            await _navigationManager.NavigateToAsync<WelcomeView>();

            MainWindow.Instance.ShowNotification("Signed Out", "You have been signed out.");
        }

        private void ScheduleTokenRefresh(DateTime expiresAt)
        {
            Dispose();

            var delay = expiresAt - DateTime.UtcNow;

            if (delay < TimeSpan.Zero)
            {
                _logger.LogWarning("Access token is already expired — scheduling immediate refresh");
                delay = TimeSpan.FromSeconds(1);
            }

            _tokenRefreshTimer = new Timer(
                callback: async _ => await OnTokenExpiredAsync(),
                state: null,
                dueTime: (int)delay.TotalMilliseconds,
                period: Timeout.Infinite);

            _logger.LogInformation(
                "Token refresh scheduled for {ExpiresAt:O} (in {Delay})",
                expiresAt, delay);
        }

        private async Task OnTokenExpiredAsync()
        {
            _logger.LogInformation("Access token expired — attempting automatic refresh");

            var refreshed = await RefreshAccessTokenAsync();

            if (refreshed)
            {
                return;
            }

            _logger.LogWarning("Automatic token refresh failed — signing user out");
            await SignOutAsync();
        }

        private void HandleTokenRefreshSuccess(AuthRefreshResponse response, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Access token refreshed successfully");

            _accessTokenProvider.Set(response.AccessToken);
            ScheduleTokenRefresh(response.AccessToken.ExpiresAt);
        }

        private void HandleTokenRefreshFailure(ApiResult.Failure failure)
        {
            _logger.LogWarning(
                "Access token refresh rejected by server — Type: {Type}",
                failure.ProblemDetails.Type);

            switch (failure.ProblemDetails.Type)
            {
                case StandardizedCodes.Authentication.AuthenticationRequired:
                case StandardizedCodes.Authentication.InvalidClientContext:
                    ClearSession();
                    break;
            }

            Dispose();
        }

        private void ClearSession()
        {
            _accessTokenProvider.Clear();
            _refreshTokenProvider.Clear();
            Dispose();
        }

        /// <summary>
        /// Disposes the token refresh timer if it is currently active.
        /// </summary>
        public void Dispose()
        {
            _tokenRefreshTimer?.Dispose();
            _tokenRefreshTimer = null;
        }
    }
}