using BlockSense.Contracts.DTOs.TwoFactorAuth.Setup;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Verification;
using BlockSense.Desktop.Models.Services;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Interfaces
{
    public interface ITwoFactorAuthService
    {
        Task<TwoFactorSetupInit> GetSetupInitAsync(CancellationToken cancellationToken = default);
        Task<bool> EnableAsync(TwoFactorSetupRequest request, CancellationToken cancellationToken = default);
        Task<bool> DisableAsync(TwoFactorVerificationRequest request, CancellationToken cancellationToken = default);
        Task<ServiceResponse> GenerateBackupCodesAsync(CancellationToken cancellationToken = default);
    }
}
