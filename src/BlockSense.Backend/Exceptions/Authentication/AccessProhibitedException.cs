using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Authentication
{
    public sealed class AccessProhibitedException : ApiException
    {
        public override string Type
        {
            get
            {
                return ApiProblemTypes.Authentication.AccountBanned;
            }
        }

        public override string Title
        {
            get
            {
                return "Access Prohibited";
            }
        }

        public override int Status
        {
            get
            {
                return StatusCodes.Status403Forbidden;
            }
        }

        public AccessProhibitedException()
            : base("Your account is currently restricted and cannot access this service. If you believe this is a mistake or need assistance, please contact support.") { }
    }
}
