using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Registration
{
    /// <summary>
    /// Thrown when a registration attempt uses an email address already associated with an existing account.
    /// </summary>
    public sealed class EmailTakenException : ApiException
    {
        /// <inheritdoc/>
        public override string Type => StandardizedCodes.Registration.EmailTaken;

        /// <inheritdoc/>
        public override string Title => "Email Already Registered";

        /// <inheritdoc/>
        public override int Status => StatusCodes.Status409Conflict;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailTakenException"/> class.
        /// </summary>
        public EmailTakenException()
            : base("The email address you entered is already associated with an existing account. Please use a different email or sign in if you already have an account.") { }
    }
}