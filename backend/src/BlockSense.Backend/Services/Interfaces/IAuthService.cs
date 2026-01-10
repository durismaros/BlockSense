using BlockSense.Backend.Models;
using BlockSense.Contracts.DTOs.Auth;

namespace BlockSense.Backend.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> AuthenticateAsync(AuthRequest request, DeviceContext deviceContext, CancellationToken cancellationToken = default);
    }
}
