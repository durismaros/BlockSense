using BlockSense.Contracts.DTOs.Registration;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Interfaces
{
    public interface IUserService
    {
        Task RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default);
        Task LoadCurrentUserAsync(CancellationToken cancellationToken = default);
    }
}
