using BlockSense.Backend.Exceptions.Authentication;
using BlockSense.Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace BlockSense.Backend.Middleware
{
    public sealed class RequireDeviceHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public RequireDeviceHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                // Apply only to login endpoints
                if (httpContext.Request.Path.StartsWithSegments("/api/auth/login"))
                {
                    var deviceContext = DeviceContext.FromHttpContext(httpContext);
                    httpContext.Items["DeviceContext"] = deviceContext;
                }

                await _next(httpContext);
            }
            catch (InvalidDeviceContextException ex)
            {
                httpContext.Response.ContentType = "application/json";
                httpContext.Response.StatusCode = ex.Status;

                var problemDetails = new ProblemDetails
                {
                    Status = ex.Status,
                    Title = ex.Title,
                    Detail = ex.Message,
                    Instance = httpContext.Request.Path,
                };

                problemDetails.Extensions["errorCode"] = ex.ErrorCode;
                problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

                await httpContext.Response.WriteAsJsonAsync(problemDetails);
            }
        }
    }
}
