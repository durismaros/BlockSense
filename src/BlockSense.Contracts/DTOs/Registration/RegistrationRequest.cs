using System.ComponentModel.DataAnnotations;

namespace BlockSense.Contracts.DTOs.Registration
{
    /// <summary>
    /// Represents the information required to register a new user account.
    /// </summary>
    public sealed record RegistrationRequest
    {
        /// <summary>
        /// The desired username for the new account. Must be between 4 and 32 characters
        /// and may only contain letters, digits, underscores, hyphens, and dots.
        /// </summary>
        [Required(ErrorMessage = "Username is required.")]
        [MinLength(4, ErrorMessage = "Username needs more characters.")]
        [MaxLength(32, ErrorMessage = "Username exceeds max length.")]
        [RegularExpression(@"^[a-zA-Z0-9_\-\.]+$", ErrorMessage = "Invalid Username format.")]
        public required string Username
        {
            get;
            init;
        }

        /// <summary>
        /// The email address of the user. Must be a valid email format and cannot exceed 256 characters.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email format.")]
        [MaxLength(256, ErrorMessage = "Email exceeds max length.")]
        public required string Email
        {
            get;
            init;
        }

        /// <summary>
        /// The password for the account. Must be between 8 and 128 characters.
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
        /// The invitation code required to complete registration.
        /// </summary>
        [Required(ErrorMessage = "Invitation code is required.")]
        public required string InvitationCode
        {
            get;
            init;
        }
    }
}