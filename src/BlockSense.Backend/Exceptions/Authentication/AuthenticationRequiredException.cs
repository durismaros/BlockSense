using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Authentication
{
    public sealed class AuthenticationRequiredException : ApiException
    {
        public override string Type
        {
            get
            {
                return ApiProblemTypes.Authentication.AuthenticationRequired;
            }
        }

        public override string Title
        {
            get
            {
                return "Authentication Required";
            }
        }

        public override int Status
        {
            get
            {
                return StatusCodes.Status401Unauthorized;
            }
        }

        public AuthenticationRequiredException()
            : base("The provided JWT or Refresh token is invalid, expired, or missing required claims. Please authenticate again and repeat the request.") { }
    }
}
