using System.Diagnostics.CodeAnalysis;

namespace BlockSense.Desktop.Models.Api
{
    /// <summary>
    /// Represents the result of an API operation, encapsulating either a successful response with data or a failure with standardized problem details.
    /// </summary>
    /// <typeparam name="T">The type of the successful response payload.</typeparam>
    public sealed record ApiResult<T>
    {
        /// <summary>
        /// Indicates whether the API operation completed successfully.
        /// </summary>
        public required bool IsSuccess
        {
            get;
            init;
        }

        /// <summary>
        /// The response payload returned by a successful API operation.
        /// This value is <c>null</c> when <see cref="IsSuccess"/> is <c>false</c>.
        /// </summary>
        public T? Data
        {
            get;
            init;
        }

        /// <summary>
        /// Problem details describing the error when the API operation fails.
        /// This value is <c>null</c> when <see cref="IsSuccess"/> is <c>true</c>.
        /// </summary>
        public ProblemDetails? ProblemDetails
        {
            get;
            init;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ApiResult{T}"/> with explicit success and payload state.
        /// </summary>
        /// <param name="isSuccess">Indicates whether the operation was successful.</param>
        /// <param name="data">The response payload, if successful.</param>
        /// <param name="problemDetails">The problem details, if the operation failed.</param>
        [SetsRequiredMembers]
        private ApiResult(bool isSuccess, T? data, ProblemDetails? problemDetails)
        {
            IsSuccess = isSuccess;
            Data = data;
            ProblemDetails = problemDetails;
        }

        /// <summary>
        /// Creates a successful API result containing the specified data.
        /// </summary>
        /// <param name="data">The response payload.</param>
        /// <returns>An <see cref="ApiResult{T}"/> representing a successful operation.</returns>
        public static ApiResult<T> Success(T data)
            => new(true, data, null);

        /// <summary>
        /// Creates a failed API result containing the specified problem details.
        /// </summary>
        /// <param name="problemDetails">The problem details describing the failure.</param>
        /// <returns>An <see cref="ApiResult{T}"/> representing a failed operation.</returns>
        public static ApiResult<T> Failure(ProblemDetails problemDetails)
            => new(false, default, problemDetails);
    }
}
