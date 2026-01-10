using BlockSense.Contracts.DTOs.Token;

namespace BlockSense.Contracts.DTOs.Authentication
{
    /// <summary>
    /// Represents the response returned by the backend after a successful user authentication.
    /// </summary>
    public sealed record AuthResponse
    {
        /// <summary>
        /// Access token issued for API authentication.
        /// </summary>
        public required AccessTokenDto AccessToken
        {
            get;
            init;
        }

        /// <summary>
        /// Refresh token used to obtain new access tokens.
        /// </summary>
        public required RefreshTokenDto RefreshToken
        {
            get;
            init;
        }
    }
}
