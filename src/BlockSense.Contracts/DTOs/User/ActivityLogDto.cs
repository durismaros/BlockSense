using BlockSense.Contracts.Enums;

namespace BlockSense.Contracts.DTOs.User
{
    /// <summary>
    /// Represents a single entry in a user's activity log.
    /// </summary>
    public sealed record ActivityLogDto
    {
        /// <summary>
        /// The unique identifier of the activity log entry.
        /// </summary>
        public required ulong Id
        {
            get;
            init;
        }

        /// <summary>
        /// The category of the activity.
        /// </summary>
        public required ActivityType Type
        {
            get;
            init;
        }

        /// <summary>
        /// The identifier of the user who performed the activity.
        /// </summary>
        public required uint UserId
        {
            get;
            init;
        }

        /// <summary>
        /// The standardized action identifier describing what was performed.
        /// </summary>
        public required string Action
        {
            get;
            init;
        }

        /// <summary>
        /// A human-readable description of the activity.
        /// </summary>
        public required string ActivityMessage
        {
            get;
            init;
        }

        /// <summary>
        /// Optional additional context associated with this activity, such as IP address or device name.
        /// </summary>
        public string? Context
        {
            get;
            init;
        }

        /// <summary>
        /// The UTC timestamp when this activity occurred.
        /// </summary>
        public required DateTime OccurredAt
        {
            get;
            init;
        }
    }
}