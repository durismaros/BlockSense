using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Authentication
{
    public sealed class InvalidDeviceContextException : ApiException
    {
        public override string Type
        {
            get
            {
                return ApiProblemTypes.Generic.BadRequest;
            }
        }

        public override string Title
        {
            get
            {
                return "Invalid Device Context";
            }
        }

        public override int Status
        {
            get
            {
                return StatusCodes.Status400BadRequest;
            }
        }

        public InvalidDeviceContextException(string key)
            : base($"The request is missing the required device header '{key}'. Please ensure your device is properly configured and try again.") { }
    }
}
