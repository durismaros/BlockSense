using BlockSense.Backend.Exceptions.Registration;
using BlockSense.Contracts.Errors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BlockSense.Backend.Exceptions.Handlers
{
    public sealed class DuplicateUserExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not UsernameTakenException &&
                exception is not EmailTakenException)
            {
                return false;
            }

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Duplicate resource",
                Detail = exception.Message,
                Instance = httpContext.Request.Path,
            };
            problemDetails.Extensions["errorCode"] = exception is UsernameTakenException ? ErrorCodes.Registration.UsernameTaken : ErrorCodes.Registration.EmailTaken;
            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
