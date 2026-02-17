using BlockSense.Contracts.DTOs.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Interfaces
{
    public interface ISessionService
    {
        Task InitializeSessionAsync(CancellationToken cancellationToken = default);
        Task EstablishSessionAsync(AuthResponse tokens, CancellationToken cancellationToken = default);
        Task<bool> RefreshAccessTokenAsync(CancellationToken cancellationToken = default);
        Task SignOutAsync(CancellationToken cancellationToken = default);
    }
}
