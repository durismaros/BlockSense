using BlockSense.Contracts.Enums;
using System.Text.Json;

namespace BlockSense.Backend.Entities
{
    public sealed class ActivityLogEntity
    {
        public required ulong LogId
        {
            get;
            set;
        }

        public required ActivityActorType ActorType
        {
            get;
            set;
        }

        public required uint? ActorId
        {
            get;
            set;
        }

        public required string Action
        {
            get;
            set;
        }

        private string? ContextJson
        {
            get;
            set;
        }

        public Dictionary<string, object>? Context
        {
            get
            {
                return (string.IsNullOrEmpty(ContextJson))
                    ? null : JsonSerializer.Deserialize<Dictionary<string, object>?>(ContextJson);
            }
            set
            {
                ContextJson = (value is null || value.Count is 0)
                    ? null : JsonSerializer.Serialize(value);
            }
        }

        public required DateTime CreatedAt
        {
            get;
            set;
        }
    }
}
