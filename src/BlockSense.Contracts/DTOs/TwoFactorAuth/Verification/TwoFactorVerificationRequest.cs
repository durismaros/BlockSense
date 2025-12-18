using System.ComponentModel.DataAnnotations;

namespace BlockSense.Contracts.DTOs.TwoFactorAuth.Verification
{
    /// <summary>
    /// Represents a request to verify a two-factor authentication (2FA) code.
    /// </summary>
    public sealed record TwoFactorVerificationRequest
    {
        /// <summary>
        /// The 6-digit 2FA code provided by the user.
        /// </summary>
        [Required(ErrorMessage = "Two-factor authentication code is required.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "The 2FA code must be exactly 6 characters long.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "The 2FA code must consist of 6 digits.")]
        public string Code { get; init; } = string.Empty;
    }
}
