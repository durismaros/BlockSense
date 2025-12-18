namespace BlockSense.Contracts.Enums.Auth
{
    /// <summary>
    /// Represents the outcome of a user login attempt.
    /// </summary>
    public enum LoginStatus
    {
        /// <summary>
        /// The login status is unknown or or has not been determined.
        /// </summary>
        Unknown,

        /// <summary>
        /// Login was successful.
        /// </summary>
        Success,

        /// <summary>
        /// The username or email provided does not exist in the system.
        /// </summary>
        UserNotFound,

        /// <summary>
        /// The provided password is incorrect.
        /// </summary>
        InvalidPassword,

        /// <summary>
        /// Two-factor authentication (2FA) is required or the provided 2FA code is invalid.
        /// </summary>
        TwoFactorRequired,

        /// <summary>
        /// The user account is locked, banned, or otherwise restricted from logging in.
        /// </summary>
        AccountLocked,
    }
}
