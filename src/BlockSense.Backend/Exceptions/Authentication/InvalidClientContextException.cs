using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Authentication
{
    public sealed class InvalidClientContextException : ApiException
    {
        public override string Type
        {
            get
            {
                return ApiProblemTypes.Authentication.InvalidClientContext;
            }
        }

        public override string Title
        {
            get
            {
                return "Invalid Client Context";
            }
        }

        public override int Status
        {
            get
            {
                return StatusCodes.Status401Unauthorized;
            }
        }

        public InvalidClientContextException()
            : base("The client context provided with this request is invalid or incomplete. Please ensure your device is properly configured and try again.") { }
    }
}
