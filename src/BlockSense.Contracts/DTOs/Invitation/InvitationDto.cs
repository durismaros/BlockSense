using BlockSense.Contracts.Enums;

namespace BlockSense.Contracts.DTOs.Invitation
{
    public sealed record InvitationDto
    {
        public required string Code
        {
            get;
            init;
        }

        public required string? RedeemedBy
        {
            get;
            init;
        }

        public required DateTime CreatedAt
        {
            get;
            init;
        }

        public required DateTime ExpiresAt
        {
            get;
            init;
        }

        public required InvitationStatus Status
        {
            get;
            init;
        }

        public required bool IsRevoked
        {
            get;
            init;
        }
    }
}
