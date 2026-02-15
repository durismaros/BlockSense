using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Authentication
{
    public sealed class TwoFactorRequiredException : ApiException
    {
        public override string Type
        {
            get
            {
                return StandardizedCodes.Authentication.TwoFactorRequired;
            }
        }

        public override string Title
        {
            get
            {
                return "Two-Factor Authentication Required";
            }
        }

        public override int Status
        {
            get
            {
                return StatusCodes.Status401Unauthorized;
            }
        }

        public TwoFactorRequiredException()
            : base("This account requires two-factor authentication (2FA) to proceed. Please complete the 2FA verification to continue.") { }
    }
}
