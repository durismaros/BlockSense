using BlockSense.Contracts.Definitions;
using BlockSense.Contracts.DTOs.Authentication;
using BlockSense.Desktop.Models.Services;
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

        /// <summary>
        /// Initializes a new instance of <see cref="AuthService"/>.
        /// </summary>
        /// <param name="logger">Logger for capturing authentication-related events.</param>
        /// <param name="apiClient">The API client used to send authentication requests.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="apiClient"/> or <paramref name="logger"/> is null.</exception>
        public AuthService(ILogger<AuthService> logger, IApiClient apiClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        }

        /// <inheritdoc/>
        public async Task<ServiceResponse> AuthAsync(AuthRequest request, CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var response = await _apiClient.PostAsync<AuthRequest, AuthResponse>(
                endpoint: "/api/auth",
                request: request,
                cancellationToken: cancellationToken);

            if (response.IsSuccess && response.Data is not null)
            {
                return new ServiceResponse
                {
                    ProblemType = ApiProblemTypes.Registration.RegistrationSuccess,
                    Message = "Authentication Successful"
                };
            }

            if (response.ProblemDetails is null ||
                response.ProblemDetails.Type is null ||
                response.ProblemDetails.Status is null ||
                response.ProblemDetails.Title is null ||
                response.ProblemDetails.Detail is null)
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
