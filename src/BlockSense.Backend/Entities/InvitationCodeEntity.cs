namespace BlockSense.Backend.Entities
{
    /// <summary>
    /// Represents an invitation code used for user registration or access control.
    /// </summary>
    public sealed class InvitationCodeEntity
    {
        /// <summary>
        /// Primary key of the invitation code.
        /// </summary>
        public required uint InvitationCodeId
        {
            get;
            set;
        }

        /// <summary>
        /// The unique alphanumeric invitation code.
        /// </summary>
        public required  string InvitationCode
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether the invitation code has already been used.
        /// </summary>
        public required bool IsUsed
        {
            get;
            set;
        }

        /// <summary>
        /// The unique identifier of the user who generated the invitation code.
        /// </summary>
        public required uint GeneratedBy
        {
            get;
            set;
        }

        /// <summary>
        /// UTC timestamp when the invitation code was created.
        /// </summary>
        public required DateTime CreatedAt
        {
            get;
            set;
        }

        /// <summary>
        /// Optional UTC timestamp indicating when the invitation code expires.
        /// </summary>
        /// <remarks><c>null</c> means the code does not expire.</remarks>
        public DateTime? ExpiresAt
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether the code has been revoked and is no longer valid.
        /// </summary>
        public required bool IsRevoked
        {
            get;
            set;
        }

        /// <summary>
        /// Returns <c>true</c> if the invitation code is currently valid.
        /// </summary>
        public bool IsActive
            => !IsUsed && !IsRevoked && (ExpiresAt is null || ExpiresAt > DateTime.UtcNow);

        /// <summary>
        /// Returns the remaining time until the code expires.
        /// If no expiration is set, returns <see cref="TimeSpan.Zero"/>.
        /// </summary>
        public TimeSpan TimeUntilExpiration
            => ExpiresAt.HasValue ? ExpiresAt.Value - DateTime.UtcNow : TimeSpan.Zero;

    }
}
