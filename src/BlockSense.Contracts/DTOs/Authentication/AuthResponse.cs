using BlockSense.Contracts.DTOs.Token;

namespace BlockSense.Contracts.DTOs.Authentication
{
    /// <summary>
    /// Represents the response returned after a successful user authentication.
    /// </summary>
    public sealed record AuthResponse
    {
        /// <summary>
        /// The access token issued for API authentication.
        /// </summary>
        public required AccessTokenDto AccessToken
        {
            get;
            init;
        }

        /// <summary>
        /// The refresh token used to obtain new access tokens when the current one expires.
        /// </summary>
        public required RefreshTokenDto RefreshToken
        {
            get;
            init;
        }
    }
}