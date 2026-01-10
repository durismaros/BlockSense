namespace BlockSense.Contracts.Enums.Authentication
{
    /// <summary>
    /// Represents the result of a two-factor authentication (2FA) verification attempt.
    /// </summary>
    public enum TwoFactorAuthStatus
    {
        /// <summary>
        /// The verification status is unknown or has not been determined.
        /// </summary>
        Unknown,

        /// <summary>
        /// The 2FA code was correct and verification succeeded.
        /// </summary>
        Success,

        /// <summary>
        /// The provided 2FA code was invalid.
        /// </summary>
        InvalidCode,

        /// <summary>
        /// The user has already enabled two-factor authentication.
        /// </summary>
        AlreadyEnabled,

        /// <summary>
        /// Operation cannot be performed yet due to a cooldown or timeout.
        /// </summary>
        TimeOut,

        /// <summary>
        /// Verification failed due to an unexpected error.
        /// </summary>
        Error
    }
}
