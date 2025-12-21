using BlockSense.Backend.Models;
using BlockSense.Contracts.DTOs.Auth.Login;
using BlockSense.Contracts.DTOs.Auth.Register;

namespace BlockSense.Backend.Services.Interfaces
{
    public interface IUserService
    {
        Task<RegistrationResponse> RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default);
        Task<LoginResponse> LoginAsync(LoginRequest request, DeviceContext deviceContext, CancellationToken cancellationToken = default);
    }
}
