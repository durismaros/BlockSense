using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Authentication
{
    public class InvalidHardwareFingerprintException : ApiException
    {
        public override string Type
        {
            get
            {
                return ApiProblemTypes.Authentication.InvalidHardwareFingerprint;
            }
        }

        public override string Title
        {
            get
            {
                return "Invalid Hardware Fingerprint";
            }
        }

        public override int Status
        {
            get
            {
                return StatusCodes.Status401Unauthorized;
            }
        }

        public InvalidHardwareFingerprintException()
            : base("The provided Hardware fingerprint is invalid. If you believe this is a mistake or need assistance, please contact support.") { }
    }
}
