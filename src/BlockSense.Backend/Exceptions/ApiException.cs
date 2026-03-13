namespace BlockSense.Backend.Exceptions
{
    /// <summary>
    /// Base class for all API-related exceptions that are translated into standardized
    /// ProblemDetails responses.
    /// </summary>
    public abstract class ApiException : Exception
    {
        /// <summary>
        /// A machine-readable error type identifier.
        /// Typically maps to a constant defined in <c>StandardizedCodes</c>.
        /// </summary>
        public abstract string Type
        {
            get;
        }

        /// <summary>
        /// A short, human-readable summary of the error.
        /// Intended to be displayed as the primary error title.
        /// </summary>
        public abstract string Title
        {
            get;
        }

        /// <summary>
        /// The HTTP status code associated with this exception.
        /// </summary>
        public abstract int Status
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiException"/> class.
        /// </summary>
        /// <param name="message">
        /// A detailed, human-readable explanation of the error.
        /// This value is typically exposed as the <c>detail</c> field in a ProblemDetails response.
        /// </param>
        public ApiException(string message) : base(message) { }
    }
}