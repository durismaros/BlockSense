using BlockSense.Contracts.Definitions;
using BlockSense.Contracts.DTOs.Authentication;
using BlockSense.Desktop.Models.Api;
using BlockSense.Desktop.Services.Interfaces;
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
        private readonly ISessionService _sessionService;
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
            ISessionService sessionService)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));

            _apiClient = apiClient
                ?? throw new ArgumentNullException(nameof(apiClient));

            _sessionService = sessionService
                ?? throw new ArgumentNullException(nameof(sessionService));

            _twoFactorSlidingPanel = MainWindow.Instance.TwoFactorSlidingPanel
                ?? throw new ArgumentNullException(nameof(MainWindow.Instance.TwoFactorSlidingPanel));
        }

        /// <inheritdoc/>
        public async Task AuthenticateAsync(AuthRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting authentication for user: {Login}", request.Login);

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
                    await HandleAuthenticationSuccessAsync(success.Data, cancellationToken);
                    break;

                case ApiResult.Failure error:
                    await HandleAuthenticationFailureAsync(error, request, cancellationToken);
                    break;
            }
        }

        #region Private Helper Methods

        private async Task HandleAuthenticationSuccessAsync(AuthResponse response, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Authentication successful");

            _twoFactorSlidingPanel.HidePanel();

            await _sessionService.EstablishSessionAsync(response, cancellationToken);
        }

        private async Task HandleAuthenticationFailureAsync(
            ApiResult.Failure error,
            AuthRequest originalRequest,
            CancellationToken cancellationToken)
        {
            _logger.LogWarning("Authentication failed: {ErrorTitle}", error.ProblemDetails.Title);

            switch (error.ProblemDetails.Type)
            {
                case StandardizedCodes.Authentication.TwoFactorRequired:
                    _logger.LogInformation("2FA required [Showing sliding panel]");
                    _twoFactorSlidingPanel.ShowPanel(async code =>
                    {
                        await AuthenticateAsync(originalRequest with { TwoFactorCode = code }, cancellationToken);
                    });
                    break;

                case StandardizedCodes.TwoFactorAuthentication.Invalid:
                    _logger.LogWarning("Invalid 2FA code");
                    await _twoFactorSlidingPanel.ShowErrorState();
                    break;

                default:
                    MainWindow.Instance.ShowNotification(
                        error.ProblemDetails.Title,
                        error.ProblemDetails.Detail);
                    break;
            }
        }

        #endregion
    }
}
