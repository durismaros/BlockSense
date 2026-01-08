using BlockSense.Contracts.Definitions;
using BlockSense.Contracts.DTOs.Registration;
using BlockSense.Desktop.Models;
using BlockSense.Desktop.Services.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BlockSense.Desktop.Services.Implementations
{
    public sealed class UserService : IUserService
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger<UserService> _logger;

        public UserService(ApiClient apiClient, ILogger<UserService> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResponse> RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var response = await _apiClient.PostAsync<RegistrationRequest, RegistrationResponse>(
                endpoint: "/api/user/register",
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
