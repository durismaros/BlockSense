using BlockSense.Contracts.Definitions;
using BlockSense.Contracts.DTOs.Authentication;
using BlockSense.Desktop.Models.Services;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.ApiHandling;
using BlockSense.Desktop.Utilities.UIComponents;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IAccessTokenProvider _accessTokenProvider;
        private readonly IRefreshTokenProvider _refreshTokenProvider;
        private readonly NavigationManager _navigationManager;

        /// <summary>
        /// Initializes a new instance of <see cref="AuthService"/>.
        /// </summary>
        /// <param name="logger">Logger for capturing authentication-related events.</param>
        /// <param name="apiClient">The API client used to send authentication requests.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="apiClient"/> or <paramref name="logger"/> is null.</exception>
        public AuthService(ILogger<AuthService> logger, IApiClient apiClient, IAccessTokenProvider accessTokenProvider, IRefreshTokenProvider refreshTokenProvider, NavigationManager navigationManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _accessTokenProvider = accessTokenProvider ?? throw new ArgumentNullException(nameof(accessTokenProvider));
            _refreshTokenProvider = refreshTokenProvider ?? throw new ArgumentNullException(nameof(refreshTokenProvider));
            _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));

            _accessTokenProvider.RefreshRequested += AuthRefreshAsync;
        }

        /// <inheritdoc/>
        public async Task<ServiceResponse> AuthAsync(AuthRequest request, CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var response = await _apiClient
                .AddDeviceHeaders()
                .PostAsync<AuthRequest, AuthResponse>(
                    requestUri: "/api/auth",
                    request: request,
                    cancellationToken: cancellationToken);

            if (response.IsSuccess && response.Data is not null)
            {
                await _refreshTokenProvider.SaveAsync(response.Data.RefreshToken);
                _accessTokenProvider.Set(response.Data.AccessToken);

                return new ServiceResponse
                {
                    ProblemType = ApiProblemTypes.Authentication.AuthenticationSuccess,
                    Message = "Authentication Successful"
                };
            }

            if (response.ProblemDetails is null)
            {
                return new ServiceResponse
                {
                    ProblemType = ApiProblemTypes.Client.UnknownError,
                    Message = "Unexpected Error"
                };
            }

            return new ServiceResponse
            {
                ProblemType = response.ProblemDetails.Type,
                Message = response.ProblemDetails.Title
            };
        }

        public async Task<bool> AuthRefreshAsync(CancellationToken cancellationToken = default)
        {
            var refreshToken = await _refreshTokenProvider.GetAsync(cancellationToken);

            var request = new AuthRefreshRequest
            {
                RefreshToken = refreshToken
            };

            var response = await _apiClient
                .AddDeviceHeaders()
                .PostAsync<AuthRefreshRequest, AuthResponse>(
                    requestUri: "/api/auth/refresh",
                    request: request,
                    cancellationToken: cancellationToken);

            if (response.IsSuccess && response.Data is not null)
            {
                await _refreshTokenProvider.SaveAsync(response.Data.RefreshToken);
                _accessTokenProvider.Set(response.Data.AccessToken);

                return true;
            }

            return false;
        }

        public async Task InitializeAsync()
        {
            try
            {
                if (await AuthRefreshAsync())
                {
                    MainWindow.Instance.ContentContainer.Content = App.ServiceProvider.GetRequiredService<HomeView>();
                    return;
                }

                throw new AuthenticationRequiredException();
            }
            catch (Exception)
            {
                MainWindow.Instance.ContentContainer.Content = App.ServiceProvider.GetRequiredService<WelcomeView>();
            }
        }
    }
}
