using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.TwoFactorAuthentication
{
    public sealed class TwoFactorAlreadyConfiguredException : ApiException
    {
        public override string Type
        {
            get
            {
                return ApiProblemTypes.TwoFactorAuthentication.AlreadyConfigured;
            }
        }

        public override string Title
        {
            get
            {
                return "2FA Already Configured";
            }
        }

        public override int Status
        {
            get
            {
                return StatusCodes.Status409Conflict;
            }
        }

        public TwoFactorAlreadyConfiguredException()
            : base("Two-factor authentication (2FA) is already enabled for this account. You do not need to set it up again.") { }
    }
}
