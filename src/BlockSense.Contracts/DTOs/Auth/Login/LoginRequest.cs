using System.ComponentModel.DataAnnotations;

namespace BlockSense.Contracts.DTOs.Auth.Login
{
    /// <summary>
    /// Represents a request to authenticate a user in the system.
    /// </summary>
    public sealed record LoginRequest
    {
        /// <summary>
        /// The username or email of the user.
        /// </summary>
        /// <remarks>Must be non-empty and cannot exceed 256 characters.</remarks>
        [Required(ErrorMessage = "Login is required.")]
        [MaxLength(256, ErrorMessage = "Login cannot exceed 256 characters.")]
        public string Login { get; init; } = string.Empty;

        /// <summary>
        /// The password for the user account.
        /// </summary>
        /// <remarks>Must meet the security requirements, with a minimum of 8 characters and maximum of 128 characters.</remarks>
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [MaxLength(128, ErrorMessage = "Password cannot exceed 128 characters.")]
        public string Password { get; init; } = string.Empty;

        /// <summary>
        /// Optional two-factor authentication (2FA) code.
        /// </summary>
        /// <remarks>The 6-digit authenticator code or an 8-character backup code (XXXX-XXX) provided by the user.</remarks>
        [RegularExpression(@"^(\d{6}|[A-Z0-9]{4}-[A-Z0-9]{3})$", ErrorMessage = "The 2FA code must be either a 6-digit numeric code or an 8-character backup code in the format XXXX-XXX.")]
        public string? TwoFactorCode { get; init; }
    }
}
