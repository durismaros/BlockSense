using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Registration
{
    /// <summary>
    /// Thrown when the invitation code provided during registration is not valid.
    /// </summary>
    public sealed class InvalidInvitationCodeException : ApiException
    {
        /// <inheritdoc/>
        public override string Type => StandardizedCodes.Registration.InvalidInvitation;

        /// <inheritdoc/>
        public override string Title => "Invalid Invitation Code";

        /// <inheritdoc/>
        public override int Status => StatusCodes.Status401Unauthorized;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidInvitationCodeException"/> class.
        /// </summary>
        public InvalidInvitationCodeException()
            : base("The invitation code you entered is not valid. Please check the code and try again, or contact support if you believe this is an error.") { }
    }
}