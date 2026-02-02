using BlockSense.Contracts.DTOs.TwoFactorAuth.Setup;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Interfaces
{
    public interface ITwoFactorAuthService
    {
        Task<TwoFactorSetupInit> GetSetupInitAsync(CancellationToken cancellationToken = default);
        Task EnableAsync(string setupKey, CancellationToken cancellationToken = default);
        Task DisableAsync(CancellationToken cancellationToken = default);
        Task GenerateBackupCodesAsync(CancellationToken cancellationToken = default);
    }
}
