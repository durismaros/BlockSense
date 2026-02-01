namespace BlockSense.Contracts.Enums
{
    /// <summary>
    /// Represents the classification and access level of a user account.
    /// </summary>
    public enum UserType
    {
        /// <summary>
        /// A standard user with normal application access.
        /// </summary>
        Standard,

        /// <summary>
        /// A user with administrative privileges.
        /// </summary>
        Administrator,

        /// <summary>
        /// A user with all possible privileges.
        /// </summary>
        Founder,

        /// <summary>
        /// A user account that has been banned and is restricted from accessing the system.
        /// </summary>
        Banned
    }
}
