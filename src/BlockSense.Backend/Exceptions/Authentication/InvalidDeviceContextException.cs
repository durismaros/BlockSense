using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Authentication
{
    public sealed class InvalidDeviceContextException : AppException
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
        public override string ErrorCode
        {
            get
            {
                return ErrorCodes.Generic.BadRequest;
            }
        }

        public InvalidDeviceContextException(string key)
            : base($"Missing required header: {key}") { }
    }
}
