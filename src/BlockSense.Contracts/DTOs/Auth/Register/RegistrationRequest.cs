using System.ComponentModel.DataAnnotations;

namespace BlockSense.Contracts.DTOs.Auth.Register
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
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters.")]
        [MaxLength(32, ErrorMessage = "Username cannot exceed 32 characters.")]
        public string Username { get; init; } = string.Empty;

        /// <summary>
        /// Gets the email address of the user.
        /// </summary>
        /// <remarks>Must be unique, a valid email format, and not exceed 256 characters.</remarks>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [MaxLength(256, ErrorMessage = "Email cannot exceed 256 characters.")]
        public string Email { get; init; } = string.Empty;

        /// <summary>
        /// Gets the password for the account.
        /// </summary>
        /// <remarks>Must meet the security requirements, with a minimum of 8 characters and maximum of 128 characters.</remarks>
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [MaxLength(128, ErrorMessage = "Password cannot exceed 128 characters.")]
        public string Password { get; init; } = string.Empty;

        /// <summary>
        /// Gets the invitation code provided for registration.
        /// </summary>
        /// <remarks>Must be exactly 32 characters. Used to control access to the registration process.</remarks>
        [Required(ErrorMessage = "Invitation code is required.")]
        [StringLength(32, MinimumLength = 32, ErrorMessage = "Invitation code must be exactly 32 characters.")]
        public string InvitationCode { get; init; } = string.Empty;
    }
}
