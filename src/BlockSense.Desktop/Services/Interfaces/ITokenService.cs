using BlockSense.Contracts.DTOs.Session;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Verification;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Interfaces
{
    public interface ITokenService
    {
        Task<bool> RevokeAsync(SessionRevokeRequest request, CancellationToken cancellationToken = default);
        Task<bool> RevokeAllAsync(TwoFactorVerificationRequest request, CancellationToken cancellationToken = default);
    }
}
