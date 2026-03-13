using BlockSense.Contracts.DTOs.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Interfaces
{
    /// <summary>
    /// Defines methods for authenticating users against the backend API.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates a user using the provided credentials.
        /// Handles two-factor authentication challenges when required.
        /// </summary>
        /// <param name="request">The authentication request containing user credentials.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        Task AuthenticateAsync(AuthRequest request, CancellationToken cancellationToken = default);
    }
}