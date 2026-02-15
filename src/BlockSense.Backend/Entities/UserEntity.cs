using BlockSense.Contracts.Enums;

namespace BlockSense.Backend.Entities
{
    public sealed class UserEntity
    {
        public required uint UserId
        {
            get;
            set;
        }

        public required string Username
        {
            get;
            set;
        }

        public required string Email
        {
            get;
            set;
        }

        public required UserType UserType
        {
            get;
            set;
        }

        public required byte[] PasswordHash
        {
            get;
            set;
        }

        public required byte[] PasswordSalt
        {
            get;
            set;
        }

        public required DateTime CreatedAt
        {
            get;
            set;
        }

        public required DateTime UpdatedAt
        {
            get;
            set;
        }

        public DateTime? DeletedAt
        {
            get;
            set;
        }

        public bool IsDeleted
            => DeletedAt.HasValue;
    }
}
