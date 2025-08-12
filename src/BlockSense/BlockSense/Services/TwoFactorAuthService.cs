using Avalonia.Controls;
using Avalonia.Diagnostics;
using Avalonia.Media.Imaging;
using BlockSense.Api;
using BlockSenseAPI.Models.TwoFactorAuth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Services
{
    public class TwoFactorAuthService
    {
        private readonly ApiClient _apiClient;
        public TwoFactorAuthService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<(string setupKey, Bitmap qrCode)> DisplayAuthSetup()
        {
            var twoFaAuthSetup = await _apiClient.OtpSetup();

            using (var memoryStream = new MemoryStream(twoFaAuthSetup.QRCodeData))
            {
                // Create and return a new Bitmap from the stream
                return (twoFaAuthSetup.SetupKey, new Bitmap(memoryStream));
            }
        }

        public async Task<bool> CompleteTwoFaSetup(TwoFactorSetupRequestModel request)
        {
            var setupResponse = await _apiClient.CompleteOtpSetup(request);
            if (setupResponse.Verification)
                return true;

            return false;
        }

        public async Task<TwoFactorVerificationResponse?> VerifyOtp(TwoFactorVerificationRequest request)
        {
            var verificationResponse = await _apiClient.VerifyOtp(request);

            if (verificationResponse == null)
                return null;

            return verificationResponse;
        }
    }
}
