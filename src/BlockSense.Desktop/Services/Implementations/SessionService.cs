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
    public sealed class SessionService : ISessionService, IDisposable
    {
        private readonly ILogger<SessionService> _logger;
        private readonly IApiClient _apiClient;
        private readonly IUserService _userService;
        private readonly IRefreshTokenProvider _refreshTokenProvider;
        private readonly IAccessTokenProvider _accessTokenProvider;
        private readonly NavigationManager _navigationManager;

        private Timer? _timer;

        public SessionService(
            ILogger<SessionService> logger,
            IApiClient apiClient,
            IUserService userService,
            IRefreshTokenProvider refreshTokenProvider,
            IAccessTokenProvider accessTokenProvider,
            NavigationManager navigationManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _refreshTokenProvider = refreshTokenProvider ?? throw new ArgumentNullException(nameof(refreshTokenProvider));
            _accessTokenProvider = accessTokenProvider ?? throw new ArgumentNullException(nameof(accessTokenProvider));
            _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
        }

        /// <inheritdoc/>
        public async Task InitializeSessionAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Initializing user session");

            if (!_refreshTokenProvider.Exists())
            {
                _logger.LogInformation("No refresh token on disk [Navigating back to WelcomeView]");
                await _navigationManager.NavigateToAsync<WelcomeView>();
                return;
            }

            _logger.LogInformation("Refresh token found [attempting to establish a new session]");

            var refreshed = await RefreshAccessTokenAsync(cancellationToken);

            if (!refreshed)
            {
                _logger.LogWarning("Attempt to establish a new session failed [Navigating back to AuthenticationView]");

                ClearSessionAsync();
                await _navigationManager.NavigateToAsync<AuthenticationView>();

                MainWindow.Instance.ShowNotification(
                    "Session Expired",
                    "Your session has expired. Please sign in again.");

                return;
            }

            await _userService.LoadCurrentUserAsync(cancellationToken);

            _logger.LogInformation("Session established");
            await _navigationManager.NavigateToAsync<HomeView>();
        }

        /// <inheritdoc/>
        public async Task EstablishSessionAsync(AuthResponse tokens, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Establishing user session");

            await _refreshTokenProvider.SaveAsync(tokens.RefreshToken, cancellationToken);
            _accessTokenProvider.Set(tokens.AccessToken);

            await _userService.LoadCurrentUserAsync(cancellationToken);
            ScheduleTokenRefresh(tokens.AccessToken.ExpiresAt);

            MainWindow.Instance.ShowNotification("Welcome Back", "Signed in successfully.");
            _logger.LogInformation("Session established");

            await Task.Delay(1000, cancellationToken);
            await _navigationManager.NavigateToAsync<HomeView>();
        }

        /// <inheritdoc/>
        public async Task<bool> RefreshAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Refreshing access token");

            string refreshToken;
            try
            {
                refreshToken = await _refreshTokenProvider.GetAsync(cancellationToken);
            }
            catch (AuthenticationRequiredException)
            {
                _logger.LogWarning("Refresh token is missing or expired");
                return false;
            }

            var result = await _apiClient
                .AddDeviceHeaders()
                .PostAsync<AuthRefreshRequest, AuthRefreshResponse>(
                    requestUri: "/api/auth/refresh",
                    request: new AuthRefreshRequest { RefreshToken = refreshToken },
                    cancellationToken: cancellationToken);

            if (result is ApiResult<AuthRefreshResponse>.Success success)
            {
                _accessTokenProvider.Set(success.Data.AccessToken);
                ScheduleTokenRefresh(success.Data.AccessToken.ExpiresAt);

                _logger.LogInformation("Access token refreshed successfully");

                return true;
            }

            Dispose();
            _logger.LogWarning("Access token refresh was rejected by the server");
            return false;
        }

        /// <inheritdoc/>
        public async Task SignOutAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("User signing out");

            ClearSessionAsync();
            await _navigationManager.NavigateToAsync<WelcomeView>();

            MainWindow.Instance.ShowNotification("Signed Out", "You have been signed out.");
        }

        private void ScheduleTokenRefresh(DateTime scheduledTime)
        {
            Dispose();

            var delay = scheduledTime - DateTime.UtcNow;

            if (delay < TimeSpan.Zero)
            {
                _logger.LogWarning("Token already expired — scheduling immediate refresh");
                delay = TimeSpan.FromSeconds(1);
            }

            _timer = new Timer(
                callback: async _ => await OnTokenExpiredAsync(),
                state: null,
                dueTime: (int)delay.TotalMilliseconds,
                period: Timeout.Infinite);

            _logger.LogInformation("Token refresh scheduled for {ExpiresAt:O} (in {Delay})", scheduledTime, delay);
        }

        private async Task OnTokenExpiredAsync()
        {
            _logger.LogInformation("Access token expired — attempting automatic refresh");

            var success = await RefreshAccessTokenAsync();

            if (!success)
            {
                _logger.LogWarning("Automatic refresh failed — signing out user");
                await SignOutAsync();
            }
        }

        private void ClearSessionAsync()
        {
            _accessTokenProvider.Clear();
            _refreshTokenProvider.Clear();
            Dispose();
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _timer = null;
        }
    }
}
