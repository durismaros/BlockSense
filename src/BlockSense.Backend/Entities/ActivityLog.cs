using BlockSense.Contracts.Enums;

namespace BlockSense.Backend.Entities
{
    public sealed class ActivityLog
    {
        public required ulong Id
        {
            get;
            init;
        }

        public required ActivityType Type
        {
            get;
            init;
        }

        public required uint UserId
        {
            get;
            init;
        }

        public required string Action
        {
            get;
            init;
        }

        public string? Context
        {
            get;
            init;
        }

        public required DateTime OccurredAt
        {
            get;
            init; 
        }
    }
}
