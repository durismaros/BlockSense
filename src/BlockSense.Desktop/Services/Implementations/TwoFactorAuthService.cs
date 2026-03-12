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
    /// <summary>
    /// Implements <see cref="ITwoFactorAuthService"/> to manage two-factor authentication (2FA)
    /// for the current user, including setup, enabling, disabling, and backup code generation.
    /// </summary>
    public sealed class TwoFactorAuthService : ITwoFactorAuthService
    {
        private readonly IApiClient _apiClient;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly TwoFactorSlidingPanel _twoFactorSlidingPanel;

        /// <summary>
        /// Initializes a new instance of <see cref="TwoFactorAuthService"/>.
        /// </summary>
        /// <param name="apiClient">The API client used to communicate with the backend.</param>
        /// <param name="currentUserProvider">The provider for accessing and updating the current user's profile.</param>
        /// <exception cref="ArgumentNullException">Thrown if any required dependency is null.</exception>
        public TwoFactorAuthService(IApiClient apiClient, ICurrentUserProvider currentUserProvider)
        {
            _apiClient = apiClient
                ?? throw new ArgumentNullException(nameof(apiClient));

            _currentUserProvider = currentUserProvider
                ?? throw new ArgumentNullException(nameof(currentUserProvider));

            _twoFactorSlidingPanel = MainWindow.Instance.TwoFactorSlidingPanel
                ?? throw new ArgumentNullException(nameof(MainWindow.Instance.TwoFactorSlidingPanel));
        }

        /// <inheritdoc/>
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

            throw new InvalidOperationException("Failed to retrieve 2FA setup initialization data.");
        }

        /// <inheritdoc/>
        public Task EnableAsync(string setupKey, CancellationToken cancellationToken = default)
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
                    .PostAsync<TwoFactorSetupRequest, object>(
                        requestUri: "/api/users/me/2fa",
                        request: request,
                        cancellationToken: cancellationToken);

                switch (response)
                {
                    case ApiResult<UserSummaryDto>.Success:
                        _currentUserProvider.SetProfile(
                            _currentUserProvider.Profile with { TwoFactorEnabled = true });

                        await _twoFactorSlidingPanel.ShowVerifiedState();
                        break;

                    case ApiResult.Failure failure:
                        await HandleTwoFactorFailureAsync(failure);
                        break;
                }
            });

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task DisableAsync(CancellationToken cancellationToken = default)
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
                    .DeleteAsync<TwoFactorVerificationRequest, object>(
                        requestUri: "/api/users/me/2fa",
                        request: request,
                        cancellationToken: cancellationToken);

                switch (response)
                {
                    case ApiResult<UserSummaryDto>.Success:
                        _currentUserProvider.SetProfile(
                            _currentUserProvider.Profile with { TwoFactorEnabled = false });

                        _currentUserProvider.SetTwoFactorBackupCodes(null);

                        await _twoFactorSlidingPanel.ShowVerifiedState();
                        break;

                    case ApiResult.Failure failure:
                        await HandleTwoFactorFailureAsync(failure);
                        break;
                }
            });

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
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
                        "Two-Factor Authentication",
                        "Backup codes have been generated and are available for download.");

                    break;

                case ApiResult.Failure failure:
                    ShowErrorNotification(failure);
                    break;
            }
        }

        private async Task HandleTwoFactorFailureAsync(ApiResult.Failure failure)
        {
            if (failure.ProblemDetails.Type is StandardizedCodes.TwoFactorAuthentication.Invalid)
            {
                await _twoFactorSlidingPanel.ShowErrorState();
                return;
            }

            ShowErrorNotification(failure);
        }

        private static void ShowErrorNotification(ApiResult.Failure failure)
        {
            MainWindow.Instance.ShowNotification(
                failure.ProblemDetails.Title,
                failure.ProblemDetails.Detail);
        }
    }
}