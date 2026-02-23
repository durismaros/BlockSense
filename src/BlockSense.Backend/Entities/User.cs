using BlockSense.Contracts.Enums;

namespace BlockSense.Backend.Entities
{
    public sealed class User
    {
        public required uint Id
        {
            get;
            init;
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

        public required UserRole Role
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
            init;
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

        public bool IsActive
            => DeletedAt is null && Role != UserRole.Banned;

        public bool IsDeleted
            => DeletedAt.HasValue;
    }
}
