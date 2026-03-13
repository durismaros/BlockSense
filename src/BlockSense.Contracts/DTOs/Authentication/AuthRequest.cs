using System.ComponentModel.DataAnnotations;

namespace BlockSense.Contracts.DTOs.Authentication
{
    /// <summary>
    /// Represents a request to authenticate a user in the BlockSense system.
    /// </summary>
    public sealed record AuthRequest
    {
        /// <summary>
        /// The username or email address of the user. Cannot exceed 256 characters.
        /// </summary>
        [Required(ErrorMessage = "Login is required.")]
        [MaxLength(256, ErrorMessage = "Login exceeds max length.")]
        public required string Login
        {
            get;
            init;
        }

        /// <summary>
        /// The password for the user account. Must be between 8 and 128 characters.
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password needs more characters.")]
        [MaxLength(128, ErrorMessage = "Password exceeds max length.")]
        public required string Password
        {
            get;
            init;
        }

        /// <summary>
        /// The optional two-factor authentication code.
        /// Accepts a 6-digit authenticator code or an 8-character backup code in XXXX-XXX format.
        /// </summary>
        [RegularExpression(@"^(\d{6}|[A-Z0-9]{4}-[A-Z0-9]{3})$", ErrorMessage = "Invalid 2FA code format.")]
        public string? TwoFactorCode
        {
            get;
            init;
        }
    }
}