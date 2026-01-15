using BlockSense.Contracts.DTOs.TwoFactorAuth.Setup;

namespace BlockSense.Backend.Services.Interfaces
{
    public interface ITwoFactorAuthService
    {
        Task<TwoFactorSetupInit> SetupInitAsync(uint userId);
        Task<bool> CompleteSetupAsync(uint userId, TwoFactorSetupRequest request);
        Task<bool> VerifyAsync(uint userId, string code);
        Task<IReadOnlyList<string>> GenerateBackupAsync(uint userId);
        Task<bool> DisableAsync(uint userId, string code);
    }
}
