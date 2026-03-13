using BlockSense.Contracts.DTOs.Token;

namespace BlockSense.Contracts.DTOs.Authentication
{
    /// <summary>
    /// Represents the response returned after a successful token refresh operation.
    /// </summary>
    public sealed record AuthRefreshResponse
    {
        /// <summary>
        /// The new access token issued for API authentication.
        /// </summary>
        public required AccessTokenDto AccessToken
        {
            get;
            init;
        }
    }
}