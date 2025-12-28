using BlockSense.Contracts.DTOs.Utilities;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BlockSense.Backend.Exceptions.Handlers
{
    public sealed class ApiExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not ApiException apiException)
            {
                return false;
            }

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = apiException.Status;

            var problemDetails = new ApiProblemDetails
            {
                Title = apiException.Title,
                Status = apiException.Status,
                Detail = apiException.Message,
                Instance = httpContext.Request.Path,
                ResultCode = apiException.ResultCode,
                TraceId = httpContext.TraceIdentifier
            };

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
