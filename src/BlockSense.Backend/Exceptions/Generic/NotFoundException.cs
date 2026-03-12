using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Generic
{
    /// <summary>
    /// Thrown when the requested resource does not exist or is no longer available.
    /// </summary>
    public sealed class NotFoundException : ApiException
    {
        /// <inheritdoc/>
        public override string Type => StandardizedCodes.Generic.NotFound;

        /// <inheritdoc/>
        public override string Title => "Not Found";

        /// <inheritdoc/>
        public override int Status => StatusCodes.Status404NotFound;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class.
        /// </summary>
        public NotFoundException()
            : base("The requested resource does not exist or is no longer available.") { }
    }
}