using System.ComponentModel.DataAnnotations;

namespace BlockSense.Contracts.DTOs.Session
{
    /// <summary>
    /// Represents a request to revoke an active device session.
    /// </summary>
    public sealed record SessionRevokeRequest
    {
        /// <summary>
        /// The hashed value of the refresh token identifying the session to revoke.
        /// </summary>
        [Required(ErrorMessage = "Token hash value is required.")]
        public required string TokenHash
        {
            get;
            init;
        }

        /// <summary>
        /// The optional 6-digit two-factor authentication code provided by the user.
        /// </summary>
        [RegularExpression(@"^\d{6}$", ErrorMessage = "The 2FA code must consist of 6 digits.")]
        public string? TwoFactorCode
        {
            get;
            init;
        }
    }
}