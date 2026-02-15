using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Authentication
{
    public sealed class InvalidCredentialsException : ApiException
    {
        public override string Type
        {
            get
            {
                return StandardizedCodes.Authentication.InvalidCredentials;
            }
        }

        public override string Title
        {
            get
            {
                return "Invalid Credentials";
            }
        }

        public override int Status
        {
            get
            {
                return StatusCodes.Status401Unauthorized;
            }
        }

        public InvalidCredentialsException()
            : base("The provided email/username or password you entered is incorrect. Please check your credentials and try again.") { }
    }
}
