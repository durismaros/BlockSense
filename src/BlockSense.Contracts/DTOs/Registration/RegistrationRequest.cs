using System.ComponentModel.DataAnnotations;

namespace BlockSense.Contracts.DTOs.Registration
{
    /// <summary>
    /// Represents the information required to register a new user account.
    /// </summary>
    public sealed record RegistrationRequest
    {
        /// <summary>
        /// Gets the desired username for the new account.
        /// </summary>
        /// <remarks>Must be unique, non-empty, and between 3 and 32 characters.</remarks>
        [Required(ErrorMessage = "Username is required.")]
        [MinLength(3, ErrorMessage = "Username needs more characters.")]
        [MaxLength(32, ErrorMessage = "Username exceeds max length.")]
        [RegularExpression(@"^[a-zA-Z0-9_\-\.]+$", ErrorMessage = "Invalid Username format")]
        public required string Username
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the email address of the user.
        /// </summary>
        /// <remarks>Must be unique, a valid email format, and not exceed 256 characters.</remarks>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email format.")]
        [MaxLength(256, ErrorMessage = "Email exceeds max length.")]
        public required string Email
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the password for the account.
        /// </summary>
        /// <remarks>Must meet the security requirements, with a minimum of 8 characters and maximum of 128 characters.</remarks>
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password needs more characters.")]
        [MaxLength(128, ErrorMessage = "Password cannot exceed 128 characters.")]
        public required string Password
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the invitation code provided for registration.
        /// </summary>
        /// <remarks>Must be exactly 32 characters. Used to control access to the registration process.</remarks>
        [Required(ErrorMessage = "Invitation code is required.")]
        public required string InvitationCode
        {
            get;
            init;
        }
    }
}
