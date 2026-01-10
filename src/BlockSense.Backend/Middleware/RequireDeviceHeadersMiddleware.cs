using BlockSense.Backend.Exceptions.Authentication;
using BlockSense.Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace BlockSense.Backend.Middleware
{
    /// <summary>
    /// Middleware that ensures required device headers are present on incoming authentication requests.
    /// </summary>
    public sealed class RequireDeviceHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of <see cref="RequireDeviceHeadersMiddleware"/>.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        public RequireDeviceHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invokes the middleware to validate device headers for authentication requests.
        /// </summary>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                // Apply only to login endpoints
                if (httpContext.Request.Path.StartsWithSegments("/api/auth"))
                {
                    var deviceContext = DeviceContext.FromHttpContext(httpContext);
                    httpContext.Items["DeviceContext"] = deviceContext;
                }

                await _next(httpContext);
            }
            catch (InvalidDeviceContextException exception)
            {
                httpContext.Response.ContentType = "application/json";
                httpContext.Response.StatusCode = exception.Status;

                var problemDetails = new ProblemDetails
                {
                    Type = exception.Type,
                    Title = exception.Title,
                    Status = exception.Status,
                    Detail = exception.Message,
                    Instance = httpContext.Request.Path,
                    Extensions =
                    {
                        ["traceId"] = httpContext.TraceIdentifier
                    }
                };

                await httpContext.Response.WriteAsJsonAsync(problemDetails);
            }
        }
    }
}
