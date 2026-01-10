using BlockSense.Contracts.DTOs.Auth;
using BlockSense.Desktop.Models;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResponse> AuthAsync(AuthRequest request, CancellationToken cancellationToken = default);
    }
}
