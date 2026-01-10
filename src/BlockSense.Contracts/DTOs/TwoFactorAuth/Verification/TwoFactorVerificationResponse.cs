using BlockSense.Contracts.Enums.Auth;

namespace BlockSense.Contracts.DTOs.TwoFactorAuth.Verification
{
    /// <summary>
    /// Represents the response returned by the backend after a two-factor authentication (2FA) verification attempt.
    /// </summary>
    public sealed record TwoFactorVerificationResponse
    {
        /// <summary>
        /// Status of the 2FA verification attempt.
        /// </summary>
        public TwoFactorAuthStatus Status { get; init; } = TwoFactorAuthStatus.Unknown;

        /// <summary>
        /// Optional human-readable message providing additional context.
        /// </summary>
        public string? Message { get; init; }
    }
}
