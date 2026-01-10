using BlockSense.Contracts.DTOs.Registration;

namespace BlockSense.Backend.Services.Interfaces
{
    public interface IUserService
    {
        Task<RegistrationResponse> RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default);
    }
}
