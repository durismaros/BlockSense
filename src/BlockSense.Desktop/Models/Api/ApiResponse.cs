using System.Diagnostics.CodeAnalysis;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BlockSense.Desktop.Models.Api
{
    public sealed record ApiResult<T>
    {
        public required bool IsSuccess
        {
            get;
            init;
        }

        public T? Data
        {
            get;
            init;
        }

        public ProblemDetails? ProblemDetails
        {
            get;
            init;
        }

        [SetsRequiredMembers]
        private ApiResult(bool isSuccess, T? data, ProblemDetails? problemDetails)
        {
            IsSuccess = isSuccess;
            Data = data;
            ProblemDetails = problemDetails;
        }

        public static ApiResult<T> Success(T data)
            => new(true, data, null);

        public static ApiResult<T> Failure(ProblemDetails problemDetails)
            => new(false, default, problemDetails);
    }
}
