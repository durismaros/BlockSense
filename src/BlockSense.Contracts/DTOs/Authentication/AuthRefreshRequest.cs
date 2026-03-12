using System.ComponentModel.DataAnnotations;

namespace BlockSense.Contracts.DTOs.Authentication
{
    /// <summary>
    /// Represents a request to refresh an expired access token using a valid refresh token.
    /// </summary>
    public sealed record AuthRefreshRequest
    {
        /// <summary>
        /// The refresh token used to obtain a new access token.
        /// </summary>
        [Required(ErrorMessage = "Refresh Token is required.")]
        public required string RefreshToken
        {
            get;
            init;
        }
    }
}