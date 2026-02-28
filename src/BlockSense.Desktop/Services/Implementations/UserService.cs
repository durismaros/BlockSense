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
    public sealed class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IApiClient _apiClient;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly NavigationManager _navigationManager;

        public UserService(
            ILogger<UserService> logger,
            IApiClient apiClient,
            ICurrentUserProvider currentUserProvider,
            NavigationManager navigationManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _currentUserProvider = currentUserProvider ?? throw new ArgumentNullException(nameof(currentUserProvider));
            _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
        }

        /// <inheritdoc/>
        public async Task RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting registration for user: {Username}", request.Username);

            var delayTask = Task.Delay(1000, cancellationToken);
            var registerTask = _apiClient
                .PostAsync<RegistrationRequest, RegistrationResponse>(
                    requestUri: "/api/users",
                    request: request,
                    cancellationToken: cancellationToken);

            var completedTask = await Task.WhenAny(registerTask, delayTask);

            if (completedTask == delayTask)
            {
                MainWindow.Instance.ShowNotification(
                    "Registration",
                    "Your request is being processed. Please wait.");
            }

            var response = await registerTask;

            switch (response)
            {
                case ApiResult<RegistrationResponse>.Success:
                    await HandleRegistrationSuccess();
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

                case ApiResult.Failure error:
                    HandleDashboardLoadFailure(error);
                    break;
            }
        }

        #region Private Helper Methods

        private async Task HandleRegistrationSuccess()
        {
            _logger.LogInformation("Registration successful");

            MainWindow.Instance.ShowNotification(
                "Registration Successful",  
                "Your account has been created successfully.");

            await _navigationManager.NavigateToAsync<AuthenticationView>();
        }

        private void HandleRegistrationFailure(ApiResult.Failure error)
        {
            _logger.LogWarning("Registration failed: {ErrorTitle}", error.ProblemDetails.Title);

            MainWindow.Instance.ShowNotification(
                error.ProblemDetails.Title,
                error.ProblemDetails.Detail);
        }

        private void HandleDashboardLoadSuccess(UserDashboardDto dashboardData)
        {
            _logger.LogInformation("Dashboard data loaded successfully for user: {UserId}", dashboardData.Profile.UserId);

            _currentUserProvider.Set(dashboardData);
        }

        private void HandleDashboardLoadFailure(ApiResult.Failure error)
        {
            _logger.LogWarning("Failed to load dashboard: {ErrorTitle}", error.ProblemDetails.Title);

            MainWindow.Instance.ShowNotification(
                error.ProblemDetails.Title,
                error.ProblemDetails.Detail);
        }

        #endregion
    }
}
