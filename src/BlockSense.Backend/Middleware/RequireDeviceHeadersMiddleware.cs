using BlockSense.Backend.Exceptions.Authentication;
using BlockSense.Backend.Models;
using BlockSense.Contracts.DTOs.Utilities;
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

                var problemDetails = new ApiProblemDetails
                {
                    Title = exception.Title,
                    Status = exception.Status,
                    Detail = exception.Message,
                    Instance = httpContext.Request.Path,
                    ResultCode = exception.ResultCode,
                    TraceId = httpContext.TraceIdentifier
                };

                await httpContext.Response.WriteAsJsonAsync(problemDetails);
            }
        }
    }
}
