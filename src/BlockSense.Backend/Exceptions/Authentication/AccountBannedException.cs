using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Authentication
{
    public sealed class AccountBannedException : ApiException
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

        public override string ResultCode
        {
            get
            {
                return ResultCodes.Authentication.AccountBanned;
            }
        }

        public AccountBannedException()
            : base("Account prohibited from system access.") { }
    }
}
