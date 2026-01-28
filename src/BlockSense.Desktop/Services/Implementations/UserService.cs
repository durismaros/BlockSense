using BlockSense.Contracts.Definitions;
using BlockSense.Contracts.DTOs.Registration;
using BlockSense.Contracts.DTOs.User;
using BlockSense.Desktop.Models.Services;
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
        public UserService(ILogger<UserService> logger, IApiClient apiClient, IAuthService authService, ICurrentUserProvider currentUserProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _currentUserProvider = currentUserProvider ?? throw new ArgumentNullException(nameof(currentUserProvider));
        }

        /// <inheritdoc/>
        public async Task<ServiceResponse> RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var response = await _apiClient.PostAsync<RegistrationRequest, RegistrationResponse>(
                requestUri: "/api/users",
                request: request,
                cancellationToken: cancellationToken);

            if (response.IsSuccess && response.Data is not null)
            {
                return new ServiceResponse
                {
                    ProblemType = ApiProblemTypes.Registration.RegistrationSuccess,
                    Message = "Registration Successful"
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

        public async Task LoadCurrentUserAsync(CancellationToken cancellationToken = default)
        {
            var response = await _apiClient
                .AddBearerToken()
                .GetAsync<UserDashboardDto>(
                requestUri: "/api/users/me/dashboard",
                cancellationToken: cancellationToken);

            if (response.IsSuccess && response.Data is not null)
            {
                _currentUserProvider.Set(response.Data);
                return;
            }

            throw new InvalidOperationException(
                response.ProblemDetails?.Title ?? "Failed to load User Dashboard data.");
        }

        /// <inheritdoc/>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _authService.AuthRefreshAsync(cancellationToken);
                await LoadCurrentUserAsync(cancellationToken);

                MainWindow.Instance.ContentContainer.Content =
                    App.ServiceProvider.GetRequiredService<HomeView>();
            }
            catch
            {
                MainWindow.Instance.ContentContainer.Content =
                    App.ServiceProvider.GetRequiredService<WelcomeView>();
            }
        }
    }
}
