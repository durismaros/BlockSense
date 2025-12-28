using BlockSense.Contracts.DTOs.Utilities;

namespace BlockSense.Desktop.Models.Api
{
    public sealed record ApiResponse<T>
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

        public ApiProblemDetails? ProblemDetails
        {
            get;
            init;
        }
    }
}
