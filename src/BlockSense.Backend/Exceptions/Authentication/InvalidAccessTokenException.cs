using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Authentication
{
    public sealed class InvalidAccessTokenException : ApiException
    {
        public override string Type
        {
            get
            {
                return ApiProblemTypes.Authentication.InvalidAccessToken;
            }
        }

        public override string Title
        {
            get
            {
                return "Invalid Access Token";
            }
        }

        public override int Status
        {
            get
            {
                return StatusCodes.Status401Unauthorized;
            }
        }

        public InvalidAccessTokenException()
            : base("The provided JWT token is invalid, expired, or missing required claims. Please authenticate again and try.") { }
    }
}
