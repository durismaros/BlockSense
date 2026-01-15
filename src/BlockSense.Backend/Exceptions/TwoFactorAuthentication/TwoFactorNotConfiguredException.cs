using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.TwoFactorAuthentication
{
    public sealed class TwoFactorNotConfiguredException : ApiException
    {
        public override string Type
        {
            get
            {
                return ApiProblemTypes.TwoFactorAuthentication.NotConfigured;
            }
        }

        public override string Title
        {
            get
            {
                return "2FA Not Configured";
            }
        }

        public override int Status
        {
            get
            {
                return StatusCodes.Status400BadRequest;
            }
        }

        public TwoFactorNotConfiguredException()
            : base("Two-factor authentication (2FA) is not configured for this account. Please set up 2FA to use this feature.") { }
    }
}
