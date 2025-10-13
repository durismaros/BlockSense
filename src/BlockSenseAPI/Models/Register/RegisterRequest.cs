using System.ComponentModel.DataAnnotations;

namespace BlockSenseAPI.Models.Requests
{
    /// <summary>
    /// Represents the payload for user registration.
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// The desired username for the new account.
        /// Must be between 3 and 50 characters.
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string? Username { get; set; }

        /// <summary>
        /// The email address for the new account.
        /// Must be a valid email format and up to 255 characters.
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string? Email { get; set; }

        /// <summary>
        /// The password for the new account.
        /// Must be between 8 and 128 characters.
        /// </summary>
        [Required]
        [StringLength(128, MinimumLength = 8)]
        public string? Password { get; set; }

        /// <summary>
        /// Invitation code required to register.
        /// Must be valid, unused, and not expired. Maximum length of 32 characters.
        /// </summary>
        [Required]
        [StringLength(32)]
        public string? InvitationCode { get; set; }
    }
}
