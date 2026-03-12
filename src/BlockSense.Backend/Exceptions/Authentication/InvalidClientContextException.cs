using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Authentication
{
    /// <summary>
    /// Thrown when the client context provided with a request is invalid or incomplete.
    /// </summary>
    public sealed class InvalidClientContextException : ApiException
    {
        /// <inheritdoc/>
        public override string Type => StandardizedCodes.Authentication.InvalidClientContext;

        /// <inheritdoc/>
        public override string Title => "Invalid Client Context";

        /// <inheritdoc/>
        public override int Status => StatusCodes.Status401Unauthorized;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidClientContextException"/> class.
        /// </summary>
        public InvalidClientContextException()
            : base("The client context provided with this request is invalid or incomplete. Please ensure your device is properly configured and try again.") { }
    }
}