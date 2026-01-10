using BlockSense.Contracts.Enums.Authentication;

namespace BlockSense.Contracts.DTOs.TwoFactorAuth
{
    /// <summary>
    /// Represents the response returned by the backend containing backup codes for two-factor authentication (2FA).
    /// </summary>
    public sealed record TwoFactorBackupCodesResponse
    {
        /// <summary>
        /// Status of the 2FA backup codes operation.
        /// </summary>
        public required TwoFactorAuthStatus Status
        {
            get;
            init;
        }

        /// <summary>
        /// A list of 2FA backup codes.
        /// </summary>
        /// <remarks>Each code is 8 alphanumeric characters in the format XXXX-XXX.</remarks>
        public required IReadOnlyList<string> Codes
        {
            get;
            init;
        }
    }
}
