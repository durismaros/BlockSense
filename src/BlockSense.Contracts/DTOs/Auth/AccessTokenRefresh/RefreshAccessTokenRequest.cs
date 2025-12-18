using BlockSense.Contracts.DTOs.Token;

namespace BlockSense.Contracts.DTOs.Auth.AccessTokenRefresh
{
    /// <summary>
    /// Represents a request to refresh an access token using a valid refresh token.
    /// </summary>
    public sealed record RefreshAccessTokenRequest
    {
        /// <summary>
        /// The refresh token previously issued to the client.
        /// </summary>
        /// <remarks>Must be a valid refresh token issued by the backend and not expired or revoked.</remarks>
        public RefreshTokenDto RefreshToken { get; init; } = new();
    }
}
