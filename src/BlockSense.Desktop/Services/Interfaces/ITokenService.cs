using BlockSense.Contracts.DTOs.Session;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Verification;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Interfaces
{
    /// <summary>
    /// Defines methods for managing user session tokens,
    /// including revoking individual or all active sessions.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Revokes a specific session token identified by its hash.
        /// Prompts for a two-factor code if required.
        /// </summary>
        /// <param name="request">The revocation request containing the token hash to revoke.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        Task RevokeAsync(SessionRevokeRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes all active session tokens for the current user and signs them out.
        /// Prompts for a two-factor code if required.
        /// </summary>
        /// <param name="request">The request containing any required two-factor authentication code.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        Task RevokeAllAsync(RevokeAllSessionsRequest request, CancellationToken cancellationToken = default);
    }
}