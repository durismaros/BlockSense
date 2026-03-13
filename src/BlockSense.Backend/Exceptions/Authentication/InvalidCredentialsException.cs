using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Authentication
{
    /// <summary>
    /// Thrown when the provided login credentials are incorrect.
    /// </summary>
    public sealed class InvalidCredentialsException : ApiException
    {
        /// <inheritdoc/>
        public override string Type => StandardizedCodes.Authentication.InvalidCredentials;

        /// <inheritdoc/>
        public override string Title => "Invalid Credentials";

        /// <inheritdoc/>
        public override int Status => StatusCodes.Status401Unauthorized;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCredentialsException"/> class.
        /// </summary>
        public InvalidCredentialsException()
            : base("The provided email/username or password you entered is incorrect. Please check your credentials and try again.") { }
    }
}