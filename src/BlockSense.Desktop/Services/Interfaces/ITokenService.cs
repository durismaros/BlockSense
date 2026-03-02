using BlockSense.Contracts.DTOs.Session;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Verification;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Interfaces
{
    public interface ITokenService
    {
        Task RevokeAsync(SessionRevokeRequest request, CancellationToken cancellationToken = default);
        Task RevokeAllAsync(RevokeAllSessionsRequest request, CancellationToken cancellationToken = default);
    }
}
