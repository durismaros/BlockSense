using BlockSense.Contracts.DTOs.Token;

namespace BlockSense.Contracts.DTOs.Auth.Login
{
    /// <summary>
    /// Represents the response returned by the backend after a user login attempt.
    /// </summary>
    public sealed record LoginResponse
    {
        /// <summary>
        /// Access token issued for API authentication.
        /// </summary>
        public AccessTokenDto? AccessToken { get; init; }

        /// <summary>
        /// Refresh token used to obtain new access tokens.
        /// </summary>
        public RefreshTokenDto? RefreshToken { get; init; }
    }
}
