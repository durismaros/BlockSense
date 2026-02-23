using BlockSense.Contracts.Enums;

namespace BlockSense.Contracts.DTOs.User
{
    public sealed record UserSummaryDto
    {
        public required uint UserId
        {
            get;
            init;
        }

        public required string Username
        {
            get;
            init;
        }

        public required string Email
        {
            get;
            init;
        }

        public required UserRole Role
        {
            get;
            init;
        }

        public required DateTime CreatedAt
        {
            get;
            init;
        }

        public required DateTime UpdatedAt
        {
            get;
            init;
        }

        public required string InvitedBy
        {
            get;
            init;
        }

        public required bool TwoFactorEnabled
        {
            get;
            init;
        }
    }
}
