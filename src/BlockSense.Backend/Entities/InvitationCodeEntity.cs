using BlockSense.Contracts.Enums;

namespace BlockSense.Backend.Entities
{
    public sealed class InvitationCodeEntity
    {
        public required uint InvitationId
        {
            get;
            set;
        }

        public required string InvitationCode
        {
            get;
            set;
        }

        public required uint GeneratedBy
        {
            get;
            set;
        }

        public uint? UsedBy
        {
            get;
            set;
        }

        public required DateTime CreatedAt
        {
            get;
            set;
        }

        public required DateTime ExpiresAt
        {
            get;
            set;
        }

        public required bool IsRevoked
        {
            get;
            set;
        }

        public InvitationStatus Status
            => IsRevoked ? InvitationStatus.Revoked :
            UsedBy.HasValue ? InvitationStatus.Used :
            ExpiresAt < DateTime.UtcNow ? InvitationStatus.Expired :
            InvitationStatus.Active;

        public TimeSpan TimeUntilExpiration
            => ExpiresAt - DateTime.UtcNow;
    }
}
