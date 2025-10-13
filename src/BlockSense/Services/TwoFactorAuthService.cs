using Avalonia.Controls;
using Avalonia.Diagnostics;
using Avalonia.Media.Imaging;
using BlockSense.Api;
using BlockSense.Models.TwoFactorAuth.BackupCode;
using BlockSense.Models.TwoFactorAuth.Setup;
using BlockSense.Models.TwoFactorAuth.Verification;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlockSense.Services
{
    public class TwoFactorAuthService
    {
        private readonly ApiClient _apiClient;
        private readonly TwoFactorBackupCodes _twoFactorBackupCodes;
        public TwoFactorAuthService(ApiClient apiClient, TwoFactorBackupCodes twoFactorBackupCodes)
        {
            _apiClient = apiClient;
            _twoFactorBackupCodes = twoFactorBackupCodes;
        }

        public async Task<(string setupKey, Bitmap? qrCode)> DisplayAuthSetup()
        {
            var twoFaAuthSetup = await _apiClient.BeginSetup();

            if (twoFaAuthSetup is null || twoFaAuthSetup.SetupKey is null || twoFaAuthSetup.QRCodeData is null)
                return (string.Empty, null);

            using (var memoryStream = new MemoryStream(twoFaAuthSetup.QRCodeData))
            {
                // Create and return a new Bitmap from the stream
                return (twoFaAuthSetup.SetupKey, new Bitmap(memoryStream));
            }
        }

        public async Task<bool> CompleteTwoFaSetup(TwoFactorSetupRequest request)
        {
            var setupResponse = await _apiClient.CompleteSetup(request);

            if (setupResponse is null || !setupResponse.Verification)
                return false;

            return true;
        }

        public async Task<bool> VerifyOtp(TwoFactorVerificationRequest request)
        {
            var verificationResponse = await _apiClient.VerifyOtp(request);

            if (verificationResponse is null || !verificationResponse.Verification)
                return false;

            return true;
        }

        public async Task<bool> DisableTwoFa(TwoFactorVerificationRequest request)
        {
            var disableResponse = await _apiClient.DisableTwoFa(request);

            if (disableResponse is null || !disableResponse.Verification)
                return false;

            return true;
        }

        public async Task<bool> GenerateBackupCodes()
        {
            var backupGenerationResponse = await _apiClient.GenerateBackupCodes();

            if (backupGenerationResponse is null || !backupGenerationResponse.Success || backupGenerationResponse.BackupCodes is null)
                return false;

            _twoFactorBackupCodes.Codes.Clear();
            _twoFactorBackupCodes.Codes.AddRange(backupGenerationResponse.BackupCodes.Codes);

            return true;
        }
    }
}
