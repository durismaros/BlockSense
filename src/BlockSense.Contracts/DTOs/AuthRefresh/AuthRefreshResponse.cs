using BlockSense.Contracts.DTOs.Token;
using BlockSense.Contracts.Enums.Authentication;

namespace BlockSense.Contracts.DTOs.AuthRefresh
{
    /// <summary>
    /// Represents the response returned by the backend after a refresh token request.
    /// </summary>
    public sealed record AuthRefreshResponse
    {
        /// <summary>
        /// The status of the refresh token request.
        /// </summary>
        public RefreshTokenStatus Status
        {
            get;
            init;
        }

        /// <summary>
        /// Optional human-readable message providing additional context.
        /// </summary>
        public string? Message
        {
            get;
            init;
        }

        /// <summary>
        /// The new access token issued by the server, if the refresh was successful.
        /// </summary>
        public AccessTokenDto? AccessToken
        {
            get;
            init;
        }
    }
}
