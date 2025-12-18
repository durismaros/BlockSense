using BlockSense.Contracts.DTOs.Token;
using BlockSense.Contracts.Enums.Auth;

namespace BlockSense.Contracts.DTOs.Auth.Login
{
    /// <summary>
    /// Represents the response returned by the backend after a user login attempt.
    /// </summary>
    public sealed record LoginResponse
    {
        /// <summary>
        /// The status of the login attempt.
        /// </summary>
        public LoginStatus Status { get; init; } = LoginStatus.Unknown;

        /// <summary>
        /// Optional human-readable message providing additional context.
        /// </summary>
        public string? Message { get; init; }

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
