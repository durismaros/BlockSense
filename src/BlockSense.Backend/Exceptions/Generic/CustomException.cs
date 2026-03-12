namespace BlockSense.Backend.Exceptions.Generic
{
    /// <summary>
    /// A general-purpose API exception that allows callers to specify the error type,
    /// title, status code, and message at the call site.
    /// </summary>
    public class CustomException : ApiException
    {
        /// <inheritdoc/>
        public override string Type
        {
            get;
        }

        /// <inheritdoc/>
        public override string Title
        {
            get;
        }

        /// <inheritdoc/>
        public override int Status
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomException"/> class.
        /// </summary>
        /// <param name="type">A machine-readable error type identifier.</param>
        /// <param name="title">A short, human-readable summary of the error.</param>
        /// <param name="status">The HTTP status code associated with this error.</param>
        /// <param name="message">A detailed, human-readable explanation of the error.</param>
        public CustomException(string type, string title, int status, string message) : base(message)
        {
            Type = type;
            Title = title;
            Status = status;
        }
    }
}