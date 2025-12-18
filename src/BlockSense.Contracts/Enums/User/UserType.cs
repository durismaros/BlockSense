namespace BlockSense.Contracts.Enums.User
{
    /// <summary>
    /// Represents the classification and access level of a user account.
    /// </summary>
    public enum UserType
    {
        /// <summary>
        /// No user type has been assigned.
        /// </summary>
        None,

        /// <summary>
        /// A standard user with normal application access.
        /// </summary>
        Standard,

        /// <summary>
        /// A user with administrative privileges.
        /// </summary>
        Administrator,

        /// <summary>
        /// A user account that has been banned and is restricted from accessing the system.
        /// </summary>
        Banned
    }
}
