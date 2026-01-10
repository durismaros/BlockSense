using System.ComponentModel.DataAnnotations;

namespace BlockSense.Contracts.DTOs.TwoFactorAuth.Verification
{
    /// <summary>
    /// Represents a request to verify a two-factor authentication (2FA) code.
    /// </summary>
    public sealed record TwoFactorVerificationRequest
    {
        /// <summary>
        /// The 6-digit authenticator code or an 8-character backup code (XXXX-XXX) provided by the user.
        /// </summary>
        [Required(ErrorMessage = "Two-factor authentication code is required.")]
        [RegularExpression(@"^(\d{6}|[A-Z0-9]{4}-[A-Z0-9]{3})$", ErrorMessage = "The 2FA code must be either a 6-digit numeric code or an 8-character backup code in the format XXXX-XXX.")]
        public string TwoFactorCode { get; init; } = string.Empty;
    }
}
