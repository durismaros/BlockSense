using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Authentication
{
    /// <summary>
    /// Thrown when a request requires authentication but the provided token is invalid,
    /// expired, or missing required claims.
    /// </summary>
    public sealed class AuthenticationRequiredException : ApiException
    {
        /// <inheritdoc/>
        public override string Type => StandardizedCodes.Authentication.AuthenticationRequired;

        /// <inheritdoc/>
        public override string Title => "Authentication Required";

        /// <inheritdoc/>
        public override int Status => StatusCodes.Status401Unauthorized;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationRequiredException"/> class.
        /// </summary>
        public AuthenticationRequiredException()
            : base("The provided JWT or Refresh token is invalid, expired, or missing required claims. Please authenticate again and repeat the request.") { }
    }
}