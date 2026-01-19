using System.ComponentModel.DataAnnotations;

namespace BlockSense.Contracts.DTOs.Authentication
{
    /// <summary>
    /// Represents a request to authenticate a user in the BlockSense system.
    /// </summary>
    public sealed record AuthRequest
    {
        /// <summary>
        /// The username or email of the user.
        /// </summary>
        /// <remarks>Must be non-empty and cannot exceed 256 characters.</remarks>
        [Required(ErrorMessage = "Login is required.")]
        [MaxLength(256, ErrorMessage = "Login exceeds max length.")]
        public required string Login
        {
            get;
            init;
        }

        /// <summary>
        /// The password for the user account.
        /// </summary>
        /// <remarks>Must meet the security requirements, with a minimum of 8 characters and maximum of 128 characters.</remarks>
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password needs more characters.")]
        [MaxLength(128, ErrorMessage = "Password exceeds max length.")]
        public required string Password
        {
            get;
            init;
        }

        /// <summary>
        /// Optional two-factor authentication (2FA) code.
        /// </summary>
        /// <remarks>The 6-digit authenticator code or an 8-character backup code (XXXX-XXX) provided by the user.</remarks>
        [RegularExpression(@"^(\d{6}|[A-Z0-9]{4}-[A-Z0-9]{3})$", ErrorMessage = "Invalid 2FA code format.")]
        public string? TwoFactorCode
        {
            get;
            init;
        }
    }
}
