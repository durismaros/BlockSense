namespace BlockSense.Contracts.Enums
{
    /// <summary>
    /// Defines the access role assigned to a user account.
    /// </summary>
    /// <remarks>
    /// Roles are ordered by privilege level from lowest to highest.
    /// The string values must match the database <c>users.role</c> ENUM exactly.
    /// </remarks>
    public enum UserRole
    {
        /// <summary>
        /// Default role assigned at registration.
        /// Grants access to standard platform features only.
        /// No elevated privileges of any kind.
        /// </summary>
        Standard = 0,

        /// <summary>
        /// Elevated role for platform management tasks such as user moderation,
        /// content oversight, and operational tooling.
        /// Must not be granted to automated or service accounts.
        /// </summary>
        Administrator = 1,

        /// <summary>
        /// Highest-privilege role, reserved for system owners.
        /// Grants unrestricted access to all platform capabilities.
        /// Should be assigned to at most one or a small fixed set of accounts;
        /// treat assignment of this role as a security-critical operation.
        /// </summary>
        Founder = 2,

        /// <summary>
        /// Suspended account. All login attempts MUST be rejected at the
        /// authentication layer before any token is issued.
        /// Preferred over hard deletion to preserve referential integrity
        /// and maintain a complete audit history.
        /// </summary>
        Banned = 3,
    }
}
