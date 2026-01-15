using BlockSense.Backend.Entities;
using BlockSense.Backend.Models.DeviceContext;
using BlockSense.Contracts.DTOs.Token;

namespace BlockSense.Backend.Services.Interfaces
{
    /// <summary>
    /// Defines operations for creating, validating, and issuing authentication tokens.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a new refresh token for a given user and device context.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="deviceContext">The device context representing the client device requesting the token.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="RefreshTokenDto"/> containing the raw token and associated metadata.</returns>
        Task<RefreshTokenDto> CreateRefreshTokenAsync(uint userId, DeviceContext deviceContext, CancellationToken cancellationToken = default);
        /// <summary>
        /// Validates a provided refresh token by checking its existence, revocation status, and expiration.
        /// </summary>
        /// <param name="token">The Base64-encoded refresh token to validate.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns><c>true</c> if the token is valid and active; otherwise, <c>false</c>.</returns>
        Task<bool> ValidateRefreshTokenAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new JWT access token for the specified user.
        /// </summary>
        /// <param name="user">The user entity for whom the access token will be issued.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>An <see cref="AccessTokenDto"/> containing the JWT and its expiration time.</returns>
        Task<AccessTokenDto> CreateAccessTokenAsync(UserEntity user, CancellationToken cancellationToken = default);
    }
}
