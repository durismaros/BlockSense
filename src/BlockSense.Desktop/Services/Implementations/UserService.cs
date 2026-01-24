using BlockSense.Contracts.Definitions;
using BlockSense.Contracts.DTOs.Registration;
using BlockSense.Desktop.Models.Services;
using BlockSense.Desktop.Services.Interfaces;
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

        /// <summary>
        /// Initializes a new instance of <see cref="UserService"/>.
        /// </summary>
        /// <param name="logger">Logger for capturing registration-related events.</param>
        /// <param name="apiClient">The API client used to send registration requests.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="apiClient"/> or <paramref name="logger"/> is null.</exception>
        public UserService(ILogger<UserService> logger, IApiClient apiClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
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
    }
}
