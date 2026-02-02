using BlockSense.Contracts.DTOs.Registration;
using BlockSense.Contracts.DTOs.User;
using BlockSense.Desktop.Models.Api;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Implementations
{
    /// <summary>
    /// Implements <see cref="IUserService"/> to handle user registration by communicating with the backend API via <see cref="IApiClient"/>.
    /// </summary>
    public sealed class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IApiClient _apiClient;
        private readonly IAuthService _authService;
        private readonly ICurrentUserProvider _currentUserProvider;

        /// <summary>
        /// Initializes a new instance of <see cref="UserService"/>.
        /// </summary>
        /// <param name="logger">Logger for capturing registration-related events.</param>
        /// <param name="apiClient">The API client used to send registration requests.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="apiClient"/> or <paramref name="logger"/> is null.</exception>
        public UserService(
            ILogger<UserService> logger,
            IApiClient apiClient,
            IAuthService authService,
            ICurrentUserProvider currentUserProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _currentUserProvider = currentUserProvider ?? throw new ArgumentNullException(nameof(currentUserProvider));
        }

        /// <inheritdoc/>
        public async Task RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default)
        {
            var delayTask = Task.Delay(1000, cancellationToken);
            var registerTask = _apiClient
                .PostAsync<RegistrationRequest, RegistrationResponse>(
                    requestUri: "/api/users",
                    request: request,
                    cancellationToken: cancellationToken);

            // Wait for whichever finishes first
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
                    MainWindow.Instance.ShowNotification(
                        "Registration",
                        "You've been successfully registered.");
                    break;

                case ApiResult.Failure failure:
                    MainWindow.Instance.ShowNotification(
                        failure.ProblemDetails.Title,
                        failure.ProblemDetails.Detail);
                    break;
            }
        }

        /// <inheritdoc/>
        public async Task LoadCurrentUserAsync(CancellationToken cancellationToken = default)
        {
            var response = await _apiClient
                .AddBearerToken()
                .GetAsync<UserDashboardDto>(
                    requestUri: "/api/users/me/dashboard",
                    cancellationToken: cancellationToken);

            switch (response)
            {
                case ApiResult<UserDashboardDto>.Success success:
                    _currentUserProvider.Set(success.Data);
                    break;

                case ApiResult.Failure failure:
                    MainWindow.Instance.ShowNotification(
                        failure.ProblemDetails.Title,
                        failure.ProblemDetails.Detail);
                    break;
            }
        }

        /// <inheritdoc/>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            var homeView = App.ServiceProvider.GetRequiredService<HomeView>();
            var welcomeView = App.ServiceProvider.GetRequiredService<WelcomeView>();

            try
            {
                await _authService.AuthRefreshAsync(cancellationToken);
                await LoadCurrentUserAsync(cancellationToken);

                await Task.Delay(800);
                await MainWindow.Instance.SwitchViewAsync(homeView);
            }
            catch
            {
                MainWindow.Instance.ContentContainer.Content = welcomeView;
            }
        }
    }
}
