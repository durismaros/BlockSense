namespace BlockSense.Contracts.Enums.User
{
    /// <summary>
    /// Represents the current status of a user device session.
    /// </summary>
    public enum UserDeviceStatus
    {
        /// <summary>
        /// The status is unknown or has not been determined.
        /// </summary>
        Unknown,

        /// <summary>
        /// The device session is active and valid.
        /// </summary>
        Active,

        /// <summary>
        /// The device session has been revoked and can no longer be used.
        /// </summary>
        Revoked,

        /// <summary>
        /// The device session has expired due to reaching its expiration time or inactivity.
        /// </summary>
        Expired
    }
}