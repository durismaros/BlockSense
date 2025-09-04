using BlockSenseAPI.Models.TwoFactorAuth.BackupCode;
using BlockSenseAPI.Models.TwoFactorAuth.Setup;
using BlockSenseAPI.Models.TwoFactorAuth.Verification;

namespace BlockSenseAPI.Services.TwoFactorAuth
{
    public interface ITwoFactorAuthService
    {
        Task<TwoFactorSetupResponse?> BeginSetup(int userId);
        Task<TwoFactorVerificationResponse?> CompleteSetup(int userId, TwoFactorSetupRequest request);
        Task<TwoFactorVerificationResponse?> VerifyOtp(int userId, string? code);
        Task<TwoFactorVerificationResponse?> DisableTwoFa(int userId, string? code);
        Task<TwoFactorBackupResponse?> GenerateBackupCodes(int userId);
    }
}
