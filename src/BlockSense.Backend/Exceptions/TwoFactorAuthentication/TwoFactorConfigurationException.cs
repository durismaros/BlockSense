using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.TwoFactorAuthentication
{
    public sealed class TwoFactorConfigurationException : ApiException
    {
        public override string Type
        {
            get
            {
                return StandardizedCodes.TwoFactorAuthentication.ConfigurationConflict;
            }
        }

        public override string Title
        {
            get
            {
                return "Two-Factor Authentication Configuration Conflict";
            }
        }

        public override int Status
        {
            get
            {
                return StatusCodes.Status409Conflict;
            }
        }

        public TwoFactorConfigurationException()
            : base("The requested operation conflicts with the current two-factor authentication configuration.")
        { }
    }
}
