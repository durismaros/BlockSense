using BlockSense.Contracts.Definitions;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Setup;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Verification;
using BlockSense.Contracts.DTOs.User;
using BlockSense.Desktop.Models.Api;
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
        private readonly TwoFactorSlidingPanel _twoFactorSlidingPanel;

        public TwoFactorAuthService(IApiClient apiClient, ICurrentUserProvider currentUserProvider)
        {
            _apiClient = apiClient
                ?? throw new ArgumentNullException(nameof(apiClient));
            _currentUserProvider = currentUserProvider
                ?? throw new ArgumentNullException(nameof(currentUserProvider));
            _twoFactorSlidingPanel = MainWindow.Instance.TwoFactorSlidingPanel
                ?? throw new ArgumentNullException(nameof(MainWindow.Instance.TwoFactorSlidingPanel));
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

        public async Task EnableAsync(string setupKey, CancellationToken cancellationToken = default)
        {
            _twoFactorSlidingPanel.ShowPanel(async code =>
            {
                var request = new TwoFactorSetupRequest
                {
                    SetupKey = setupKey,
                    TwoFactorCode = code
                };

                var response = await _apiClient
                    .AddBearerToken()
                    .PostAsync<TwoFactorSetupRequest, UserSummaryDto>(
                        request: request,
                        requestUri: "/api/users/me/2fa",
                        cancellationToken: cancellationToken);

                switch (response)
                {
                    case ApiResult<UserSummaryDto>.Success success:
                        _currentUserProvider.SetProfile(success.Data);

                        await _twoFactorSlidingPanel.ShowVerifiedState();
                        return;

                    case ApiResult.Failure failure:
                        await HandleFailureAsync(failure);
                        break;
                }
            });
        }

        public async Task DisableAsync(CancellationToken cancellationToken = default)
        {
            _twoFactorSlidingPanel.BackUpToggleButton.IsVisible = false;
            _twoFactorSlidingPanel.ShowPanel(async code =>
            {
                var request = new TwoFactorVerificationRequest
                {
                    TwoFactorCode = code
                };

                var response = await _apiClient
                    .AddBearerToken()
                    .DeleteAsync<TwoFactorVerificationRequest, UserSummaryDto>(
                        request: request,
                        requestUri: "/api/users/me/2fa",
                        cancellationToken: cancellationToken);

                switch (response)
                {
                    case ApiResult<UserSummaryDto>.Success success:
                        _currentUserProvider.SetProfile(success.Data);
                        _currentUserProvider.SetTwoFactorBackupCodes(null);

                        await _twoFactorSlidingPanel.ShowVerifiedState();
                        return;

                    case ApiResult.Failure failure:
                        await HandleFailureAsync(failure);
                        break;
                }
            });
        }

        public async Task GenerateBackupCodesAsync(CancellationToken cancellationToken = default)
        {
            var response = await _apiClient
                .AddBearerToken()
                .GetAsync<IReadOnlyList<string>>(
                    requestUri: "/api/users/me/2fa/backup",
                    cancellationToken: cancellationToken);

            switch (response)
            {
                case ApiResult<IReadOnlyList<string>>.Success success:
                    _currentUserProvider.SetTwoFactorBackupCodes(success.Data);

                    MainWindow.Instance.ShowNotification(
                        "Two Factor Authentication",
                        "Backup codes have been successfully generated and are now available for download.");

                    break;

                case ApiResult.Failure failure:
                    MainWindow.Instance.ShowNotification(
                        failure.ProblemDetails.Title,
                        failure.ProblemDetails.Detail);
                    break;
            }
        }

        private async Task HandleFailureAsync(ApiResult.Failure failure)
        {
            if (failure.ProblemDetails.Type is StandardizedCodes.TwoFactorAuthentication.Invalid)
            {
                await _twoFactorSlidingPanel.ShowErrorState();
                return;
            }

            MainWindow.Instance.ShowNotification(
                failure.ProblemDetails.Title,
                failure.ProblemDetails.Detail);
        }
    }
}
