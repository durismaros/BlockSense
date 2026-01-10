using BlockSense.Contracts.DTOs.Registration;
using BlockSense.Desktop.Models;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResponse> RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default);
    }
}
