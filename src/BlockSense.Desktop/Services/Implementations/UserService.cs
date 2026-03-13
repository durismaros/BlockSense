using BlockSense.Contracts.DTOs.Registration;
using BlockSense.Contracts.DTOs.User;
using BlockSense.Desktop.Models.Api;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.UIComponents;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Implementations
{
    /// <summary>
    /// Implements <see cref="IUserService"/> to manage user account registration
    /// and loading of the current user's dashboard data.
    /// </summary>
    public sealed class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IApiClient _apiClient;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly NavigationManager _navigationManager;

        /// <summary>
        /// Initializes a new instance of <see cref="UserService"/>.
        /// </summary>
        /// <param name="logger">The logger used to record user service events.</param>
        /// <param name="apiClient">The API client used to communicate with the backend.</param>
        /// <param name="currentUserProvider">The provider for accessing and updating the current user's state.</param>
        /// <param name="navigationManager">The navigation manager used to redirect the user between views.</param>
        /// <exception cref="ArgumentNullException">Thrown if any required dependency is null.</exception>
        public UserService(
            ILogger<UserService> logger,
            IApiClient apiClient,
            ICurrentUserProvider currentUserProvider,
            NavigationManager navigationManager)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));

            _apiClient = apiClient
                ?? throw new ArgumentNullException(nameof(apiClient));

            _currentUserProvider = currentUserProvider
                ?? throw new ArgumentNullException(nameof(currentUserProvider));

            _navigationManager = navigationManager
                ?? throw new ArgumentNullException(nameof(navigationManager));
        }

        /// <inheritdoc/>
        public async Task RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting registration for user: {Username}", request.Username);

            var result = await SendRegistrationRequestWithDelayNotificationAsync(request, cancellationToken);

            switch (result)
            {
                case ApiResult<RegistrationResponse>.Success:
                    await HandleRegistrationSuccessAsync();
                    break;

                case ApiResult.Failure failure:
                    HandleRegistrationFailure(failure);
                    break;
            }
        }

        /// <inheritdoc/>
        public async Task LoadCurrentUserAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Loading current user dashboard data");

            var response = await _apiClient
                .AddBearerToken()
                .GetAsync<UserDashboardDto>(
                    requestUri: "/api/users/me/dashboard",
                    cancellationToken: cancellationToken);

            switch (response)
            {
                case ApiResult<UserDashboardDto>.Success success:
                    HandleDashboardLoadSuccess(success.Data);
                    break;

                case ApiResult.Failure failure:
                    HandleDashboardLoadFailure(failure);
                    break;
            }
        }

        private async Task<ApiResult> SendRegistrationRequestWithDelayNotificationAsync(
            RegistrationRequest request,
            CancellationToken cancellationToken)
        {
            var delayTask = Task.Delay(1000, cancellationToken);
            var registerTask = _apiClient
                .PostAsync<RegistrationRequest, RegistrationResponse>(
                    requestUri: "/api/users",
                    request: request,
                    cancellationToken: cancellationToken);

            var completedFirst = await Task.WhenAny(registerTask, delayTask);

            if (completedFirst == delayTask)
            {
                MainWindow.Instance.ShowNotification(
                    "Registration",
                    "Your request is being processed. Please wait.");
            }

            return await registerTask;
        }

        private async Task HandleRegistrationSuccessAsync()
        {
            _logger.LogInformation("Registration completed successfully");

            MainWindow.Instance.ShowNotification(
                "Registration Successful",
                "Your account has been created successfully.");

            await _navigationManager.NavigateToAsync<AuthenticationView>();
        }

        private void HandleRegistrationFailure(ApiResult.Failure failure)
        {
            _logger.LogWarning("Registration failed: {ErrorTitle}", failure.ProblemDetails.Title);

            MainWindow.Instance.ShowNotification(
                failure.ProblemDetails.Title,
                failure.ProblemDetails.Detail);
        }

        private void HandleDashboardLoadSuccess(UserDashboardDto dashboardData)
        {
            _logger.LogInformation(
                "Dashboard data loaded successfully for user: {UserId}",
                dashboardData.Profile.UserId);

            _currentUserProvider.Set(dashboardData);
        }

        private void HandleDashboardLoadFailure(ApiResult.Failure failure)
        {
            _logger.LogWarning("Failed to load dashboard data: {ErrorTitle}", failure.ProblemDetails.Title);

            MainWindow.Instance.ShowNotification(
                failure.ProblemDetails.Title,
                failure.ProblemDetails.Detail);
        }
    }
}