namespace BlockSense.Contracts.Enums.Auth
{
    /// <summary>
    /// Represents the outcome of a user registration attempt.
    /// </summary>
    public enum RegistrationStatus
    {
        /// <summary>
        /// The registration status is unknown or has not been determined.
        /// </summary>
        Unknown,

        /// <summary>
        /// Registration completed successfully.
        /// </summary>
        Success,

        /// <summary>
        /// The requested username is already taken by another user.
        /// </summary>
        UsernameTaken,

        /// <summary>
        /// The email address is already registered with another account.
        /// </summary>
        EmailTaken,

        /// <summary>
        /// The provided password did not meet the security requirements.
        /// </summary>
        WeakPassword,

        /// <summary>
        /// The provided invitation code is invalid or expired.
        /// </summary>
        InvalidInvitationCode,
    }
}
