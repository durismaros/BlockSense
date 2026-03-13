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
    /// Implements <see cref="IAuthService"/> to handle user authentication
    /// by communicating with the backend API via <see cref="IApiClient"/>.
    /// Manages two-factor authentication challenges when required.
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
        /// <param name="logger">The logger used to record authentication events.</param>
        /// <param name="apiClient">The API client used to send authentication requests.</param>
        /// <param name="sessionService">The session service used to establish a session after successful authentication.</param>
        /// <exception cref="ArgumentNullException">Thrown if any required dependency is null.</exception>
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

            var result = await SendAuthRequestWithDelayNotificationAsync(request, cancellationToken);

            switch (result)
            {
                case ApiResult<AuthResponse>.Success success:
                    await HandleAuthSuccessAsync(success.Data, cancellationToken);
                    break;

                case ApiResult.Failure failure:
                    await HandleAuthFailureAsync(failure, request, cancellationToken);
                    break;
            }
        }

        private async Task<ApiResult> SendAuthRequestWithDelayNotificationAsync(
            AuthRequest request,
            CancellationToken cancellationToken)
        {
            var delayTask = Task.Delay(1000, cancellationToken);
            var authTask = _apiClient
                .AddDeviceHeaders()
                .PostAsync<AuthRequest, AuthResponse>(
                    requestUri: "/api/auth",
                    request: request,
                    cancellationToken: cancellationToken);

            var completedFirst = await Task.WhenAny(authTask, delayTask);

            if (completedFirst == delayTask)
            {
                MainWindow.Instance.ShowNotification(
                    "Authentication",
                    "Your request is being processed. Please wait.");
            }

            return await authTask;
        }

        private async Task HandleAuthSuccessAsync(AuthResponse response, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Authentication successful");

            _twoFactorSlidingPanel.HidePanel();

            await _sessionService.EstablishSessionAsync(response, cancellationToken);
        }

        private async Task HandleAuthFailureAsync(
            ApiResult.Failure failure,
            AuthRequest originalRequest,
            CancellationToken cancellationToken)
        {
            _logger.LogWarning("Authentication failed: {ErrorTitle}", failure.ProblemDetails.Title);

            switch (failure.ProblemDetails.Type)
            {
                case StandardizedCodes.Authentication.TwoFactorRequired:
                    HandleTwoFactorRequired(originalRequest, cancellationToken);
                    break;

                case StandardizedCodes.TwoFactorAuthentication.Invalid:
                    await HandleInvalidTwoFactorCodeAsync();
                    break;

                default:
                    ShowErrorNotification(failure);
                    break;
            }
        }

        private void HandleTwoFactorRequired(AuthRequest originalRequest, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Two-factor authentication required — showing sliding panel");

            _twoFactorSlidingPanel.ShowPanel(async code =>
            {
                await AuthenticateAsync(originalRequest with { TwoFactorCode = code }, cancellationToken);
            });
        }

        private async Task HandleInvalidTwoFactorCodeAsync()
        {
            _logger.LogWarning("Invalid two-factor authentication code entered");
            await _twoFactorSlidingPanel.ShowErrorState();
        }

        private static void ShowErrorNotification(ApiResult.Failure failure)
        {
            MainWindow.Instance.ShowNotification(
                failure.ProblemDetails.Title,
                failure.ProblemDetails.Detail);
        }
    }
}