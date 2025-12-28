using BlockSense.Contracts.Definitions;
using BlockSense.Contracts.DTOs.Registration;
using BlockSense.Desktop.Models.Api;
using BlockSense.Desktop.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

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

        public async Task<string> RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));

            var response = await _apiClient.PostAsync<RegistrationRequest, RegistrationResponse>(
                endpoint: "/api/user/register",
                request: request,
                cancellationToken: cancellationToken);

            if (response.IsSuccess && response.Data is not null)
            {
                return ResultCodes.Registration.RegistrationSuccess;
            }

            else if (response.ProblemDetails is null || response.ProblemDetails.ResultCode is null)
            {
                return ResultCodes.Client.UnknownError;
            }
            
            return response.ProblemDetails.ResultCode;
        }
    }
}
