using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Generic
{
    public sealed class ExternalServiceException : ApiException
    {
        public override string Type
        {
            get
            {
                return StandardizedCodes.Generic.ExternalServiceError;
            }
        }

        public override string Title
        {
            get
            {
                return "External Service Error";
            }
        }

        public override int Status
        {
            get
            {
                return StatusCodes.Status502BadGateway;
            }
        }

        public ExternalServiceException()
            : base("n error occurred while processing your request with an external service. Please try again later.") { }
    }
}