using BlockSense.Contracts.DTOs.TwoFactorAuth;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Setup;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Verification;

namespace BlockSense.Backend.Services.Interfaces
{
    public interface ITwoFactorAuthService
    {
        Task<bool> VerifyAsync(uint userId, string code);
        Task<TwoFactorSetupInit> SetupInitAsync(uint userId);
        Task<bool> CompleteSetupAsync(uint userId, TwoFactorSetupRequest request);
        Task<bool> DisableAsync(uint userId, string code);
        Task<TwoFactorBackupResponse> GenerateBackupAsync(uint userId);
    }
}
