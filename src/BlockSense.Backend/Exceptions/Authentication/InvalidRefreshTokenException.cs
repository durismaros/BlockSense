using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Authentication
{
    public class InvalidRefreshTokenException : ApiException
    {
        public override string Type
        {
            get
            {
                return ApiProblemTypes.Authentication.InvalidRefreshToken;
            }
        }

        public override string Title
        {
            get
            {
                return "Invalid Refresh Token";
            }
        }

        public override int Status
        {
            get
            {
                return StatusCodes.Status401Unauthorized;
            }
        }

        public InvalidRefreshTokenException()
            : base("The provided Refresh token is invalid, expired, or revoked. Please authenticate again and repeat the request.") { }
    }
}
