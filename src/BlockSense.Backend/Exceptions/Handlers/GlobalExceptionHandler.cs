using BlockSense.Contracts.Definitions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BlockSense.Backend.Exceptions.Handlers
{
    /// <summary>
    /// Global exception handler responsible for catching unhandled exceptions and returning a <see cref="ProblemDetails"/> response.
    /// </summary>
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalExceptionHandler"/> class.
        /// </summary>
        /// <param name="logger">Logger used to record unhandled exceptions and pipeline failures.</param>
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Attempts to handle an unhandled exception that occurred during request processing.
        /// </summary>
        /// <param name="httpContext">The current HTTP context associated with the request.</param>
        /// <param name="exception">The unhandled exception thrown during request execution.</param>
        /// <param name="cancellationToken">Token used to cancel the operation if the request is aborted.</param>
        /// <returns><c>true</c> if the exception was successfully handled and a response was written, 
        /// <c>false</c> if the response has already started and handling is not possible.</returns>
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (httpContext.Response.HasStarted)
            {
                return false;
            }

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var problemDetails = new ProblemDetails
            {
                Type = ApiProblemTypes.Generic.InternalServerError,
                Title = "Internal Server Error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An unexpected server error occurred while processing your request. " +
                         "Please try again later or contact support with the trace ID.",
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
