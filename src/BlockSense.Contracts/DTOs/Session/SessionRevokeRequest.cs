using System.ComponentModel.DataAnnotations;

namespace BlockSense.Contracts.DTOs.Session
{
    public sealed record class SessionRevokeRequest
    {
        /// <summary>
        /// Unique identifier of the device session (Hashed value of the refresh token).
        /// </summary>
        [Required(ErrorMessage = "Token hash value is required.")]
        public required string TokenHash
        {
            get;
            init;
        }

        /// <summary>
        /// The 6-digit authenticator code or an 8-character backup code (XXXX-XXX) provided by the user.
        /// </summary>
        [Required(ErrorMessage = "Two-Factor authentication code is required.")]
        [RegularExpression(@"^(\d{6}|[A-Z0-9]{4}-[A-Z0-9]{3})$", ErrorMessage = "The 2FA code must be either a 6-digit numeric code or an 8-character backup code in the format XXXX-XXX.")]
        public required string TwoFactorCode
        {
            get;
            init;
        }
    }
}
