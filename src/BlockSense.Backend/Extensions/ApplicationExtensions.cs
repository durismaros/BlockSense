namespace BlockSense.Backend.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IApplicationBuilder"/> that configure
    /// the HTTP request pipeline.
    /// </summary>
    public static class ApplicationExtensions
    {
        /// <summary>
        /// Adds a middleware that appends essential HTTP security headers to every response.
        /// </summary>
        /// <param name="app">The application's request pipeline builder.</param>
        /// <returns>The same <see cref="IApplicationBuilder"/> instance for chaining.</returns>
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                var headers = context.Response.Headers;

                headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
                headers["X-Content-Type-Options"] = "nosniff";
                headers["Referrer-Policy"] = "no-referrer";
                headers["Cache-Control"] = "no-store";
                headers["Pragma"] = "no-cache";

                await next();
            });
        }
    }
}