using System.ComponentModel.DataAnnotations;

namespace BlockSense.Contracts.DTOs.Session
{
    public sealed class RevokeAllSessionsRequest
    {
        /// <summary>
        /// The 6-digit authenticator code or an 8-character backup code (XXXX-XXX) provided by the user.
        /// </summary>
        [RegularExpression(@"^\d{6}$", ErrorMessage = "The 2FA code must consist of 6 digits.")]
        public string? TwoFactorCode
        {
            get;
            init;
        }
    }
}
