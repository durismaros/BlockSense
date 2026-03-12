using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Registration
{
    /// <summary>
    /// Thrown when a registration attempt uses a username that is already taken.
    /// </summary>
    public sealed class UsernameTakenException : ApiException
    {
        /// <inheritdoc/>
        public override string Type => StandardizedCodes.Registration.UsernameTaken;

        /// <inheritdoc/>
        public override string Title => "Username Already Taken";

        /// <inheritdoc/>
        public override int Status => StatusCodes.Status409Conflict;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsernameTakenException"/> class.
        /// </summary>
        public UsernameTakenException()
            : base("The username you selected is not available. Please choose a different username.") { }
    }
}