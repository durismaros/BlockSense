using BlockSense.Contracts.Errors;

namespace BlockSense.Backend.Exceptions.Authentication
{
    public sealed class InvalidCredentialsException : AuthenticationException
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

        public override string ErrorCode
        {
            get
            {
                return ErrorCodes.Authentication.InvalidCredentials;
            }
        }

        public InvalidCredentialsException()
            : base("The provided username/email or password is invalid.") { }
    }
}
