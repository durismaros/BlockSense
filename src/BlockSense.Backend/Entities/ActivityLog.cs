using BlockSense.Contracts.Enums;

namespace BlockSense.Backend.Entities
{
    /// <summary>
    /// Represents a recorded entry in the system activity log.
    /// </summary>
    public sealed class ActivityLog
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
        /// The category of activity that was performed.
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
        /// A short description of the action that was performed.
        /// </summary>
        public required string Action
        {
            get;
            init;
        }

        /// <summary>
        /// Optional additional context or metadata about the action.
        /// </summary>
        public string? Context
        {
            get;
            init;
        }

        /// <summary>
        /// The UTC date and time at which the activity occurred.
        /// </summary>
        public required DateTime OccurredAt
        {
            get;
            init;
        }
    }
}