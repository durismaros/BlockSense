namespace BlockSense.Desktop.Models.Services
{
    /// <summary>
    /// Represents a simplified, UI-friendly response produced by application services.
    /// </summary>
    public sealed record ServiceResponse
    {
        /// <summary>
        /// A machine-readable identifier describing the type of problem or outcome.
        /// </summary>
        public required string ProblemType
        {
            get;
            init;
        }

        /// <summary>
        /// A human-readable message describing the outcome or error.
        /// Intended for display in the user interface.
        /// </summary>
        public required string Message
        {
            get;
            init;
        }
    }
}
