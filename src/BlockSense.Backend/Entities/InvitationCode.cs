namespace BlockSense.Backend.Entities
{
    /// <summary>
    /// Represents an invitation code issued to a user for onboarding purposes.
    /// </summary>
    public sealed class InvitationCode
    {
        /// <summary>
        /// The unique identifier of the invitation code.
        /// </summary>
        public required uint Id
        {
            get;
            init;
        }

        /// <summary>
        /// The invitation code string presented to the recipient.
        /// </summary>
        public required string Code
        {
            get;
            init;
        }

        /// <summary>
        /// The identifier of the user to whom this invitation code was issued.
        /// </summary>
        public required uint IssuedToId
        {
            get;
            init;
        }

        /// <summary>
        /// The identifier of the user who redeemed this invitation code, or <c>null</c> if not yet redeemed.
        /// </summary>
        public required uint? RedeemedById
        {
            get;
            set;
        }

        /// <summary>
        /// The username of the user who redeemed this invitation code, or <c>null</c> if not yet redeemed.
        /// </summary>
        public string? RedeemedByUsername
        {
            get;
            set;
        }

        /// <summary>
        /// The UTC date and time at which this invitation code was created.
        /// </summary>
        public required DateTime CreatedAt
        {
            get;
            init;
        }

        /// <summary>
        /// The UTC date and time at which this invitation code expires.
        /// </summary>
        public required DateTime ExpiresAt
        {
            get;
            init;
        }

        /// <summary>
        /// Indicates whether this invitation code has been manually revoked.
        /// </summary>
        public required bool IsRevoked
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether this invitation code has been redeemed by a user.
        /// </summary>
        public bool IsRedeemed
            => RedeemedById is not null;

        /// <summary>
        /// Indicates whether this invitation code has passed its expiration date.
        /// </summary>
        public bool IsExpired
            => DateTime.UtcNow >= ExpiresAt;

        /// <summary>
        /// Indicates whether this invitation code can still be used.
        /// Returns <c>true</c> only when the code is not revoked, not redeemed, and not expired.
        /// </summary>
        public bool IsValid
            => !IsRevoked && !IsRedeemed && DateTime.UtcNow < ExpiresAt;

        /// <summary>
        /// The remaining time before this invitation code expires.
        /// Returns a negative value if the code has already expired.
        /// </summary>
        public TimeSpan TimeUntilExpiration
            => ExpiresAt - DateTime.UtcNow;
    }
}