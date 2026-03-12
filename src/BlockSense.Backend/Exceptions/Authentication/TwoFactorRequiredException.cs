using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Authentication
{
    /// <summary>
    /// Thrown when an account requires two-factor authentication to proceed but it has not been completed.
    /// </summary>
    public sealed class TwoFactorRequiredException : ApiException
    {
        /// <inheritdoc/>
        public override string Type => StandardizedCodes.Authentication.TwoFactorRequired;

        /// <inheritdoc/>
        public override string Title => "Two-Factor Authentication Required";

        /// <inheritdoc/>
        public override int Status => StatusCodes.Status401Unauthorized;

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoFactorRequiredException"/> class.
        /// </summary>
        public TwoFactorRequiredException()
            : base("This account requires two-factor authentication (2FA) to proceed. Please complete the 2FA verification to continue.") { }
    }
}