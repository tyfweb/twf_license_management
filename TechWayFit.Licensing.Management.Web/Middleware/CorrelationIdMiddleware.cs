using Serilog.Context;

namespace TechWayFit.Licensing.Management.Web.Middleware
{
    /// <summary>
    /// Middleware to add correlation ID to all requests for tracking across log files
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeaderName = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = GetOrGenerateCorrelationId(context);
            
            // Add correlation ID to response headers
            context.Response.Headers[CorrelationIdHeaderName] = correlationId;
            
            // Add correlation ID to Serilog LogContext for all subsequent logs
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }

        private static string GetOrGenerateCorrelationId(HttpContext context)
        {
            // Check if correlation ID is provided in request headers
            if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId) &&
                !string.IsNullOrWhiteSpace(correlationId))
            {
                return correlationId.ToString();
            }

            // Generate new correlation ID
            return Guid.NewGuid().ToString("D");
        }
    }

    /// <summary>
    /// Extension methods for correlation ID middleware
    /// </summary>
    public static class CorrelationIdMiddlewareExtensions
    {
        /// <summary>
        /// Add correlation ID middleware to the pipeline
        /// </summary>
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}
