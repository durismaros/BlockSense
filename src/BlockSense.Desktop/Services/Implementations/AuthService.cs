using BlockSense.Contracts.Definitions;
using BlockSense.Contracts.DTOs.Auth;
using BlockSense.Contracts.DTOs.Registration;
using BlockSense.Desktop.Models;
using BlockSense.Desktop.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Implementations
{
    public sealed class AuthService : IAuthService
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger<AuthService> _logger;

        public AuthService(ApiClient apiClient, ILogger<AuthService> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResponse> AuthAsync(AuthRequest request, CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

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

            else if (response.ProblemDetails is null || response.ProblemDetails.Type is null || response.ProblemDetails.Title is null)
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
