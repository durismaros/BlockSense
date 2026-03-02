using BlockSense.Backend.Models.Device;
using BlockSense.Contracts.DTOs.Authentication;
using BlockSense.Contracts.DTOs.Session;
using BlockSense.Contracts.DTOs.Token;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Verification;

namespace BlockSense.Backend.Services.Interfaces
{
    /// <summary>
    /// Defines operations for creating, validating, and issuing authentication tokens.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Validates a provided refresh token by checking its existence, revocation status, and expiration.
        /// </summary>
        /// <param name="token">The Base64-encoded refresh token to validate.</param>
        /// <param name="deviceContext">The device context representing the client device requesting the token refresh.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>An <see cref="AuthResponse"/> containing the JWT and a new Refresh token.</returns>
        Task<AuthRefreshResponse> RefreshAccessTokenAsync(AuthRefreshRequest request, DeviceContext deviceContext, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates a new refresh token for a given user and device context.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="deviceContext">The device context representing the client device requesting the token.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="RefreshTokenDto"/> containing the raw token and associated metadata.</returns>
        Task<RefreshTokenDto> CreateRefreshTokenAsync(uint userId, DeviceContext deviceContext, CancellationToken cancellationToken = default);
        /// <summary>
        /// Creates a new JWT access token for the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>An <see cref="AccessTokenDto"/> containing the JWT and its expiration time.</returns>
        Task<AccessTokenDto> CreateAccessTokenAsync(uint userId, CancellationToken cancellationToken = default);

        Task RevokeSessionAsync(uint userId, SessionRevokeRequest request, CancellationToken cancellationToken = default);
        Task RevokeAllSessionsAsync(uint userId, RevokeAllSessionsRequest request, CancellationToken cancellationToken = default);
    }
}
