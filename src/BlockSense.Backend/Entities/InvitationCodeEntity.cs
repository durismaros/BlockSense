namespace BlockSense.Backend.Entities
{
    /// <summary>
    /// Represents an invitation code which can be used for user registration.
    /// </summary>
    public sealed class InvitationCodeEntity
    {
        /// <summary>
        /// Primary key of the invitation code.
        /// </summary>
        public uint InvitationCodeId { get; set; }

        /// <summary>
        /// The unique alphanumeric invitation code.
        /// </summary>
        public string InvitationCode { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the code has already been used.
        /// </summary>
        public bool IsUsed { get; set; }

        /// <summary>
        /// The ID of the user who generated this invitation code.
        /// </summary>
        public uint GeneratedBy { get; set; }

        /// <summary>
        /// UTC timestamp when the invitation code was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Optional UTC timestamp indicating when the invitation code expires.
        /// </summary>
        /// <remarks>Null means the code does not expire.</remarks>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Indicates whether the code has been revoked and is no longer valid.
        /// </summary>
        public bool IsRevoked { get; set; }

        /// <summary>
        /// Determines if the code is currently active.
        /// </summary>
        public bool IsActive => !IsUsed && !IsRevoked && (ExpiresAt == null || ExpiresAt > DateTime.UtcNow);
    }
}
