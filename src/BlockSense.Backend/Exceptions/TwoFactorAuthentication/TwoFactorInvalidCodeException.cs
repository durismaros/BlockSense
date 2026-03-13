using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.TwoFactorAuthentication
{
    /// <summary>
    /// Thrown when the two-factor authentication code provided by the user is incorrect.
    /// </summary>
    public sealed class TwoFactorInvalidCodeException : ApiException
    {
        /// <inheritdoc/>
        public override string Type => StandardizedCodes.TwoFactorAuthentication.Invalid;

        /// <inheritdoc/>
        public override string Title => "Invalid 2FA Code";

        /// <inheritdoc/>
        public override int Status => StatusCodes.Status401Unauthorized;

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoFactorInvalidCodeException"/> class.
        /// </summary>
        public TwoFactorInvalidCodeException()
            : base("The two-factor authentication (2FA) code you entered is incorrect. Please verify your code and try again.") { }
    }
}