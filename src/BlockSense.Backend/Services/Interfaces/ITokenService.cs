using BlockSense.Backend.Models.Device;
using BlockSense.Contracts.DTOs.Authentication;
using BlockSense.Contracts.DTOs.Session;
using BlockSense.Contracts.DTOs.Token;

namespace BlockSense.Backend.Services.Interfaces
{
    /// <summary>
    /// Defines operations for creating, refreshing, and revoking authentication tokens.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Refreshes an access token using a valid refresh token.
        /// </summary>
        /// <param name="request">The refresh request containing the Base64-encoded refresh token.</param>
        /// <param name="deviceContext">The device context representing the client device requesting the refresh.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>An <see cref="AuthRefreshResponse"/> containing the new access token.</returns>
        Task<AuthRefreshResponse> RefreshAccessTokenAsync(AuthRefreshRequest request, DeviceContext deviceContext, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates a new refresh token for the specified user and device context.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="deviceContext">The device context representing the client device requesting the token.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="RefreshTokenDto"/> containing the raw token and its expiration metadata.</returns>
        Task<RefreshTokenDto> CreateRefreshTokenAsync(uint userId, DeviceContext deviceContext, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new JWT access token for the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>An <see cref="AccessTokenDto"/> containing the signed JWT and its expiration time.</returns>
        Task<AccessTokenDto> CreateAccessTokenAsync(uint userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes a specific session identified by its token hash.
        /// </summary>
        /// <param name="userId">The unique identifier of the user who owns the session.</param>
        /// <param name="request">The revoke request containing the token hash and optional two-factor code.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        Task RevokeSessionAsync(uint userId, SessionRevokeRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes all active sessions for the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="request">The revoke request containing an optional two-factor code.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        Task RevokeAllSessionsAsync(uint userId, RevokeAllSessionsRequest request, CancellationToken cancellationToken = default);
    }
}