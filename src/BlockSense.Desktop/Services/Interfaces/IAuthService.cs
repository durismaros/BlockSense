using BlockSense.Contracts.DTOs.Authentication;
using BlockSense.Contracts.DTOs.Token;
using BlockSense.Desktop.Models.Services;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Interfaces
{
    /// <summary>
    /// Defines methods for user authentication in the BlockSense desktop application.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates a user using the provided <see cref="AuthRequest"/> containing login credentials.
        /// </summary>
        /// <param name="request">The authentication request with username/email and password.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="ServiceResponse"/> indicating the result of the authentication attempt.</returns>
        Task<ServiceResponse> AuthAsync(AuthRequest request, CancellationToken cancellationToken = default);
        Task<bool> AuthRefreshAsync(CancellationToken cancellationToken = default);
        Task InitializeAsync();
    }
}
