namespace BlockSense.Desktop.Models.Api
{
    /// <summary>
    /// Represents the result of an API operation, encapsulating either a successful response with data or a failure with standardized problem details.
    /// </summary>
    /// <typeparam name="T">The type of the successful response payload.</typeparam>
    public abstract record ApiResult<T>
    {
        /// <summary>
        /// Indicates whether the API operation completed successfully.
        /// </summary>
        public abstract bool IsSuccess
        {
            get;
        }

        private ApiResult() { }

        public sealed record Success(T Data) : ApiResult<T>
        {
            public override bool IsSuccess => true;
        }

        public sealed record Failure(ProblemDetails ProblemDetails) : ApiResult<T>
        {
            public override bool IsSuccess => false;
        }
    }
}
