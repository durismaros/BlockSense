namespace BlockSense.Contracts.Enums.User
{
    /// <summary>
    /// Represents the current status of an invitation code.
    /// </summary>
    public enum InvitationStatus
    {
        /// <summary>
        /// The invitation is active and can be used to register a new account.
        /// </summary>
        Active,

        /// <summary>
        /// The invitation has already been used to register an account.
        /// </summary>
        Used,

        /// <summary>
        /// The invitation has expired and can no longer be used.
        /// </summary>
        Expired,

        /// <summary>
        /// The invitation has been revoked and is no longer valid.
        /// </summary>
        Revoked
    }
}
