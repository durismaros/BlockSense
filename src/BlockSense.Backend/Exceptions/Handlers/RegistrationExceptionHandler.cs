using BlockSense.Backend.Exceptions.Registration;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BlockSense.Backend.Exceptions.Handlers
{
    public sealed class RegistrationExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not RegistationException regException)
            {
                return false;
            }

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = regException.Status;

            var problemDetails = new ProblemDetails
            {
                Status = regException.Status,
                Title = regException.Title,
                Detail = regException.Message,
                Instance = httpContext.Request.Path
            };

            problemDetails.Extensions["errorCode"] = regException.ErrorCode;
            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
