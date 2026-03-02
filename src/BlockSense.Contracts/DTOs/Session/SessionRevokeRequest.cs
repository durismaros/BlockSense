using System.ComponentModel.DataAnnotations;

namespace BlockSense.Contracts.DTOs.Session
{
    public sealed record class SessionRevokeRequest
    {
        /// <summary>
        /// Unique identifier of the device session (Raw value of the refresh token).
        /// </summary>
        [Required(ErrorMessage = "Token hash value is required.")]
        public required string TokenHash
        {
            get;
            init;
        }

        /// <summary>
        /// The 6-digit authenticator code provided by the user.
        /// </summary>
        [RegularExpression(@"^\d{6}$", ErrorMessage = "The 2FA code must consist of 6 digits.")]
        public string? TwoFactorCode
        {
            get;
            init;
        }
    }
}
