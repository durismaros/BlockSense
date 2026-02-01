using BlockSense.Contracts.Definitions;
using BlockSense.Contracts.DTOs.Authentication;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Setup;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Verification;
using BlockSense.Contracts.DTOs.User;
using BlockSense.Desktop.Models.Api;
using BlockSense.Desktop.Models.Services;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Implementations
{
    public sealed class TwoFactorAuthService : ITwoFactorAuthService
    {
        private readonly IApiClient _apiClient;
        private readonly ICurrentUserProvider _currentUserProvider;

        public TwoFactorAuthService(IApiClient apiClient, ICurrentUserProvider currentUserProvider)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _currentUserProvider = currentUserProvider ?? throw new ArgumentNullException(nameof(currentUserProvider));
        }

        public async Task<TwoFactorSetupInit> GetSetupInitAsync(CancellationToken cancellationToken = default)
        {
            var response = await _apiClient
                .AddBearerToken()
                .GetAsync<TwoFactorSetupInit>(
                    requestUri: "/api/users/me/2fa",
                    cancellationToken: cancellationToken);

            if (response is ApiResult<TwoFactorSetupInit>.Success success)
            {
                return success.Data;
            }

            MainWindow.Instance.ShowNotification(
                "Two-Factor Setup Error",
                "Unable to load your Two-Factor Authentication setup data.");

            throw new InvalidOperationException();
        }

        public async Task<bool> EnableAsync(TwoFactorSetupRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _apiClient
                .AddBearerToken()
                .PostAsync<TwoFactorSetupRequest, UserSummaryDto>(
                    request: request,
                    requestUri: "/api/users/me/2fa",
                    cancellationToken: cancellationToken);

            if (response.IsSuccess &&
                response.Data is not null)
            {
                _currentUserProvider.SetProfile(response.Data);

                return true;
            }

            if (response.ProblemDetails is not null &&
                response.ProblemDetails.Type is ApiProblemTypes.TwoFactorAuthentication.InvalidCode)
            {
                return false;
            }

            throw new InvalidOperationException(
                response.ProblemDetails?.Title ?? "Failed to enable Two-Factor Authentication.");
        }

        public async Task<bool> DisableAsync(TwoFactorVerificationRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _apiClient
                .AddBearerToken()
                .DeleteAsync<TwoFactorVerificationRequest, UserSummaryDto>(
                request: request,
                requestUri: "/api/users/me/2fa",
                cancellationToken: cancellationToken);

            if (response.IsSuccess &&
                response.Data is not null)
            {
                _currentUserProvider.SetProfile(response.Data);
                _currentUserProvider.SetTwoFactorBackupCodes(null);

                return true;
            }

            if (response.ProblemDetails is not null &&
                response.ProblemDetails.Type is ApiProblemTypes.TwoFactorAuthentication.InvalidCode)
            {
                return false;
            }

            throw new InvalidOperationException(
                response.ProblemDetails?.Title ?? "Failed to disable Two-Factor Authentication.");
        }

        public async Task<ServiceResponse> GenerateBackupCodesAsync(CancellationToken cancellationToken = default)
        {
            var response = await _apiClient
                .AddBearerToken()
                .GetAsync<IReadOnlyList<string>>(
                requestUri: "/api/users/me/2fa/backup",
                cancellationToken: cancellationToken);

            if (response.IsSuccess && response.Data is not null)
            {
                _currentUserProvider.SetTwoFactorBackupCodes(response.Data);

                return new ServiceResponse
                {
                    ProblemType = ApiProblemTypes.TwoFactorAuthentication.TwoFactorAuthenticationSuccess,
                    Message = "Backup codes have been successfully generated and are now available for download."
                };
            }

            if (response.ProblemDetails is not null &&
                response.ProblemDetails.Type is ApiProblemTypes.TwoFactorAuthentication.BackupCodesCooldown)
            {
                return new ServiceResponse
                {
                    ProblemType = response.ProblemDetails.Type,
                    Message = response.ProblemDetails.Detail
                };
            }

            throw new InvalidOperationException(
                response.ProblemDetails?.Title ?? "Failed to generate User Two-Factor backup codes.");
        }
    }
}
