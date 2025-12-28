using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Authentication
{
    public sealed class InvalidDeviceContextException : ApiException
    {
        public override int Status
        {
            get
            {
                return StatusCodes.Status400BadRequest;
            }
        }
        public override string Title
        {
            get
            {
                return "Invalid device context";
            }
        }
        public override string ResultCode
        {
            get
            {
                return ResultCodes.Generic.BadRequest;
            }
        }

        public InvalidDeviceContextException(string key)
            : base($"Missing required header: {key}") { }
    }
}
