using BlockSense.Contracts.DTOs.Token;

namespace BlockSense.Contracts.DTOs.Authentication
{
    public sealed record AuthRefreshResponse
    {
        /// <summary>
        /// Access token issued for API authentication.
        /// </summary>
        public required AccessTokenDto AccessToken
        {
            get;
            init;
        }
    }
}
