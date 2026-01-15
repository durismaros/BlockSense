using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.TwoFactorAuthentication
{
    public sealed class TwoFactorInvalidCodeException : ApiException
    {
        public override string Type
        {
            get
            {
                return ApiProblemTypes.TwoFactorAuthentication.InvalidCode;
            }
        }

        public override string Title
        {
            get
            {
                return "Invalid 2FA Code";
            }
        }

        public override int Status
        {
            get
            {
                return StatusCodes.Status401Unauthorized;
            }
        }

        public TwoFactorInvalidCodeException()
            : base("The two-factor authentication (2FA) code you entered is incorrect. Please verify your code and try again.") { }
    }
}
