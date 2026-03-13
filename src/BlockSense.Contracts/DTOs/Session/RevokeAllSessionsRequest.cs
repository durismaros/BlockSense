using System.ComponentModel.DataAnnotations;

namespace BlockSense.Contracts.DTOs.Session
{
    /// <summary>
    /// Represents a request to revoke all active sessions for a user account.
    /// </summary>
    public sealed record RevokeAllSessionsRequest
    {
        /// <summary>
        /// The optional 6-digit two-factor authentication code provided by the user.
        /// </summary>
        [RegularExpression(@"^\d{6}$", ErrorMessage = "The 2FA code must consist of 6 digits.")]
        public string? TwoFactorCode
        {
            get;
            init;
        }
    }
}