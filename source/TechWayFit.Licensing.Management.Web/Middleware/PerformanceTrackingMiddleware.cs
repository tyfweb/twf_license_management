using System.Diagnostics;
using TechWayFit.Licensing.Management.Web.Services;

namespace TechWayFit.Licensing.Management.Web.Middleware
{
    /// <summary>
    /// Middleware to track HTTP request performance metrics for operations dashboard
    /// </summary>
    public class PerformanceTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceTrackingMiddleware> _logger;
        private readonly IServiceProvider _serviceProvider;

        public PerformanceTrackingMiddleware(
            RequestDelegate next, 
            ILogger<PerformanceTrackingMiddleware> logger,
            IServiceProvider serviceProvider)
        {
            _next = next;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip tracking for static files, health checks, and operations dashboard API calls
            if (ShouldSkipTracking(context))
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var originalBodyStream = context.Response.Body;

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                
                // Record performance metrics asynchronously
                _ = Task.Run(async () => await RecordPerformanceMetricAsync(context, stopwatch.Elapsed));
            }
        }

        private static bool ShouldSkipTracking(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";
            
            // Skip static files
            if (path.Contains("/css/") || path.Contains("/js/") || path.Contains("/images/") || 
                path.Contains("/fonts/") || path.Contains("/favicon") || path.Contains(".css") ||
                path.Contains(".js") || path.Contains(".png") || path.Contains(".jpg") ||
                path.Contains(".jpeg") || path.Contains(".gif") || path.Contains(".ico"))
            {
                return true;
            }

            // Skip health check endpoints
            if (path.Contains("/health") || path.Contains("/ping"))
            {
                return true;
            }

            // Skip operations dashboard data recording calls to prevent recursion
            if (path.Contains("/operationsdashboard/record") || path.Contains("/api/metrics"))
            {
                return true;
            }

            return false;
        }

        private async Task RecordPerformanceMetricAsync(HttpContext context, TimeSpan duration)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var metricsBuffer = scope.ServiceProvider
                    .GetRequiredService<MetricsBufferService>();

                var controller = context.Request.RouteValues["controller"]?.ToString() ?? "Unknown";
                var action = context.Request.RouteValues["action"]?.ToString() ?? "Unknown";
                var path = context.Request.Path.Value ?? "/";
                var method = context.Request.Method;

                await metricsBuffer.AddPerformanceMetricAsync(new
                {
                    controller = controller,
                    action = action,
                    path = path,
                    method = method,
                    responseTimeMs = (int)duration.TotalMilliseconds,
                    statusCode = context.Response.StatusCode,
                    userAgent = context.Request.Headers["User-Agent"].FirstOrDefault(),
                    ipAddress = GetClientIpAddress(context),
                    timestamp = DateTime.UtcNow
                });

                // Also record system metric for overall performance tracking
                await metricsBuffer.AddSystemMetricAsync(new
                {
                    metricName = "RequestResponseTime",
                    metricValue = duration.TotalMilliseconds,
                    metricUnit = "ms",
                    category = "Performance",
                    timestamp = DateTime.UtcNow
                });

                _logger.LogDebug("Performance metrics buffered for {Method} {Path}: {Duration}ms", 
                    method, path, duration.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error buffering performance metrics");
            }
        }

        private static string GetClientIpAddress(HttpContext context)
        {
            // Check for forwarded IP first (useful when behind proxy/load balancer)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }

    /// <summary>
    /// Extension methods for performance tracking middleware
    /// </summary>
    public static class PerformanceTrackingMiddlewareExtensions
    {
        /// <summary>
        /// Add performance tracking middleware to the pipeline
        /// </summary>
        public static IApplicationBuilder UsePerformanceTracking(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PerformanceTrackingMiddleware>();
        }
    }
}
