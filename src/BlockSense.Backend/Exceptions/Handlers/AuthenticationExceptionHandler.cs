using BlockSense.Backend.Exceptions.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BlockSense.Backend.Exceptions.Handlers
{
    public class AuthenticationExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not AuthenticationException authException)
            {
                return false;
            }

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = authException.Status;

            var problemDetails = new ProblemDetails
            {
                Status = authException.Status,
                Title = authException.Title,
                Detail = authException.Message,
                Instance = httpContext.Request.Path
            };

            problemDetails.Extensions["errorCode"] = authException.ErrorCode;
            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
