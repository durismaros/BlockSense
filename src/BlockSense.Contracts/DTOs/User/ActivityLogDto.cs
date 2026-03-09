using BlockSense.Contracts.Enums;

namespace BlockSense.Contracts.DTOs.User
{
    public sealed class ActivityLogDto
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

        public required string ActivityMessage
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
