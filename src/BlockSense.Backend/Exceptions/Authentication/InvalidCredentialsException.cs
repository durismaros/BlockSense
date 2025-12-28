using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Authentication
{
    public sealed class InvalidCredentialsException : ApiException
    {
        public override int Status
        {
            get
            {
                return StatusCodes.Status401Unauthorized;
            }
        }

        public override string Title
        {
            get
            {
                return "Authentication failed";
            }
        }

        public override string ResultCode
        {
            get
            {
                return ResultCodes.Authentication.InvalidCredentials;
            }
        }

        public InvalidCredentialsException()
            : base("The provided username/email or password is invalid.") { }
    }
}
