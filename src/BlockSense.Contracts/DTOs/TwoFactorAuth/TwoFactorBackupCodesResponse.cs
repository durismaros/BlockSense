using BlockSense.Contracts.Enums.Auth;

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
        public TwoFactorAuthStatus Status { get; init; } = TwoFactorAuthStatus.Unknown;

        /// <summary>
        /// A list of 2FA backup codes.
        /// </summary>
        /// <remarks>Each code is 8 alphanumeric characters in the format XXXX-XXX.</remarks>
        public IReadOnlyList<string> Codes { get; init; } = new List<string>();
    }
}
