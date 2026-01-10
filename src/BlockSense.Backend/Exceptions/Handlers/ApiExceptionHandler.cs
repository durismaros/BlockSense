using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BlockSense.Backend.Exceptions.Handlers
{
    /// <summary>
    /// Exception handler responsible for processing application-specific <see cref="ApiException"/> instances and converting them into <see cref="ProblemDetails"/> responses.
    /// </summary>
    public sealed class ApiExceptionHandler : IExceptionHandler
    {
        /// <summary>
        /// Attempts to handle an exception thrown during request processing.
        /// </summary>
        /// <param name="httpContext">The current HTTP context for the request.</param>
        /// <param name="exception">The exception thrown during execution.</param>
        /// <param name="cancellationToken">Token used to cancel the operation if the request is aborted.</param>
        /// <returns><c>true</c> if the exception was handled and a response was written; otherwise, <c>false</c> if the exception is not an <see cref="ApiException"/>.</returns>
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not ApiException apiException)
            {
                return false;
            }

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = apiException.Status;

            var problemDetails = new ProblemDetails
            {
                Type = apiException.Type,
                Title = apiException.Title,
                Status = apiException.Status,
                Detail = apiException.Message,
                Instance = httpContext.Request.Path,
                Extensions =
                {
                    ["traceId"] = httpContext.TraceIdentifier
                }
            };

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
