using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Authentication
{
    public sealed class TwoFactorRequiredException : ApiException
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
                return "Two-factor authentication required";
            }
        }

        public override string ResultCode
        {
            get
            {
                return ResultCodes.Authentication.TwoFactorRequired;
            }
        }

        public TwoFactorRequiredException()
            : base("Two-factor authentication is required or the provided code is invalid.") { }
    }
}
