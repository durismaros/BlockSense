using BlockSense.Contracts.Errors;

namespace BlockSense.Backend.Exceptions.Authentication
{
    public sealed class AccountBannedException : AuthenticationException
    {
        public override int Status
        {
            get
            {
                return StatusCodes.Status403Forbidden;
            }
        }

        public override string Title
        {
            get
            {
                return "Account banned";
            }
        }

        public override string ErrorCode
        {
            get
            {
                return ErrorCodes.Authentication.AccountBanned;
            }
        }

        public AccountBannedException()
            : base("Account prohibited from system access.") { }
    }
}
