using BlockSense.Contracts.DTOs.TwoFactorAuth.Setup;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Verification;

namespace BlockSense.Backend.Services.Interfaces
{
    public interface ITwoFactorAuthService
    {
        Task<TwoFactorSetupInit> SetupInitAsync(uint userId, CancellationToken cancellationToken = default);
        Task<bool> CompleteSetupAsync(uint userId, TwoFactorSetupRequest request, CancellationToken cancellationToken = default);
        Task VerifyAsync(uint userId, TwoFactorVerificationRequest request, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<string>> GenerateBackupCodesAsync(uint userId, CancellationToken cancellationToken = default);
        Task<bool> DisableAsync(uint userId, TwoFactorVerificationRequest request, CancellationToken cancellationToken = default);
    }
}
