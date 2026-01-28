using BlockSense.Contracts.DTOs.Registration;
using BlockSense.Desktop.Models.Services;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Interfaces
{
    /// <summary>
    /// Defines methods for user-related operations in the BlockSense desktop application.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Registers a new user account using the provided <see cref="RegistrationRequest"/>.
        /// </summary>
        /// <param name="request">The registration request containing username, email, password, and invitation code.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="ServiceResponse"/> indicating the result of the registration attempt.</returns>
        Task<ServiceResponse> RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default);

        Task InitializeAsync(CancellationToken cancellationToken = default);
    }
}
