using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Generic
{
    /// <summary>
    /// Thrown when the authenticated user does not have permission to access the requested resource.
    /// </summary>
    public sealed class ForbiddenException : ApiException
    {
        /// <inheritdoc/>
        public override string Type => StandardizedCodes.Generic.Forbidden;

        /// <inheritdoc/>
        public override string Title => "Access Prohibited";

        /// <inheritdoc/>
        public override int Status => StatusCodes.Status403Forbidden;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
        /// </summary>
        public ForbiddenException()
            : base("Your account is currently restricted and cannot access this service. If you believe this is a mistake or need assistance, please contact support.") { }
    }
}