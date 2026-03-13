using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Generic
{
    /// <summary>
    /// Thrown when a downstream external service fails or returns an unexpected error.
    /// </summary>
    public sealed class ExternalServiceException : ApiException
    {
        /// <inheritdoc/>
        public override string Type => StandardizedCodes.Generic.ExternalServiceError;

        /// <inheritdoc/>
        public override string Title => "External Service Error";

        /// <inheritdoc/>
        public override int Status => StatusCodes.Status502BadGateway;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalServiceException"/> class.
        /// </summary>
        public ExternalServiceException()
            : base("An error occurred while processing your request with an external service. Please try again later.") { }
    }
}