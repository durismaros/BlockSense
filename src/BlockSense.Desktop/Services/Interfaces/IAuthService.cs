using BlockSense.Contracts.DTOs.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Interfaces
{
    public interface IAuthService
    {
        Task AuthenticateAsync(AuthRequest request, CancellationToken cancellationToken = default);
    }
}
