using TechWayFit.Licensing.Management.Web.Services;

namespace TechWayFit.Licensing.Management.Web.Middleware
{
    /// <summary>
    /// Middleware to capture and track application errors for operations dashboard
    /// </summary>
    public class ErrorTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorTrackingMiddleware> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ErrorTrackingMiddleware(
            RequestDelegate next, 
            ILogger<ErrorTrackingMiddleware> logger,
            IServiceProvider serviceProvider)
        {
            _next = next;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
                
                // Track 4xx and 5xx responses as errors
                if (context.Response.StatusCode >= 400)
                {
                    await RecordHttpErrorAsync(context, context.Response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                // Record the exception
                await RecordExceptionAsync(context, ex);
                
                // Re-throw the exception to maintain normal error handling flow
                throw;
            }
        }

        private async Task RecordHttpErrorAsync(HttpContext context, int statusCode)
        {
            try
            {
                // Skip static files and health checks to avoid noise
                if (IsStaticFileOrHealthCheck(context.Request.Path))
                    return;

                using var scope = _serviceProvider.CreateScope();
                var metricsBuffer = scope.ServiceProvider
                    .GetService<MetricsBufferService>();

                if (metricsBuffer != null)
                {
                    var controller = context.Request.RouteValues["controller"]?.ToString() ?? "Unknown";
                    var action = context.Request.RouteValues["action"]?.ToString() ?? "Unknown";
                    var path = context.Request.Path.Value ?? "/";
                    var method = context.Request.Method;

                    var errorMessage = GetHttpErrorMessage(statusCode);
                    var errorLevel = GetErrorLevel(statusCode);

                    // Add to in-memory buffer instead of immediate DB write
                    await metricsBuffer.AddErrorMetricAsync(new
                    {
                        errorMessage = $"HTTP {statusCode}: {errorMessage} - {method} {path}",
                        errorLevel = errorLevel,
                        controller = controller,
                        action = action,
                        path = path,
                        method = method,
                        statusCode = statusCode,
                        userAgent = context.Request.Headers["User-Agent"].FirstOrDefault(),
                        ipAddress = GetClientIpAddress(context),
                        timestamp = DateTime.UtcNow
                    });

                    _logger.LogWarning("HTTP {StatusCode} error buffered for {Method} {Path}", 
                        statusCode, method, path);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error buffering HTTP error for status code {StatusCode}", statusCode);
            }
        }

        private async Task RecordExceptionAsync(HttpContext context, Exception exception)
        {
            try
            {
                // Skip static files and health checks to avoid noise
                if (IsStaticFileOrHealthCheck(context.Request.Path))
                    return;

                using var scope = _serviceProvider.CreateScope();
                var metricsBuffer = scope.ServiceProvider
                    .GetService<MetricsBufferService>();

                if (metricsBuffer != null)
                {
                    var controller = context.Request.RouteValues["controller"]?.ToString() ?? "Unknown";
                    var action = context.Request.RouteValues["action"]?.ToString() ?? "Unknown";
                    var path = context.Request.Path.Value ?? "/";
                    var method = context.Request.Method;

                    // Add to in-memory buffer instead of immediate DB write
                    await metricsBuffer.AddErrorMetricAsync(new
                    {
                        errorMessage = $"Exception in {controller}/{action}: {exception.Message}",
                        errorLevel = "Error",
                        controller = controller,
                        action = action,
                        path = path,
                        method = method,
                        exceptionType = exception.GetType().Name,
                        stackTrace = exception.StackTrace?.Substring(0, Math.Min(exception.StackTrace?.Length ?? 0, 2000)), // Limit stack trace length
                        innerException = exception.InnerException?.Message,
                        userAgent = context.Request.Headers["User-Agent"].FirstOrDefault(),
                        ipAddress = GetClientIpAddress(context),
                        timestamp = DateTime.UtcNow
                    });

                    _logger.LogError(exception, "Exception buffered for {Controller}/{Action}", controller, action);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error buffering exception: {OriginalException}", exception.Message);
            }
        }

        private static string GetHttpErrorMessage(int statusCode)
        {
            return statusCode switch
            {
                400 => "Bad Request",
                401 => "Unauthorized",
                403 => "Forbidden",
                404 => "Not Found",
                405 => "Method Not Allowed",
                408 => "Request Timeout",
                409 => "Conflict",
                422 => "Unprocessable Entity",
                429 => "Too Many Requests",
                500 => "Internal Server Error",
                501 => "Not Implemented",
                502 => "Bad Gateway",
                503 => "Service Unavailable",
                504 => "Gateway Timeout",
                _ => "HTTP Error"
            };
        }

        private static string GetErrorLevel(int statusCode)
        {
            return statusCode switch
            {
                >= 500 => "Error",
                >= 400 => "Warning",
                _ => "Information"
            };
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

        private static bool IsStaticFileOrHealthCheck(string path)
        {
            return path.Contains(".css") || path.Contains(".js") || path.Contains(".ico") ||
                   path.Contains(".png") || path.Contains(".jpg") || path.Contains(".gif") ||
                   path.Contains(".svg") || path.Contains(".woff") || path.Contains(".ttf") ||
                   path.Contains("/health") || path.Contains("/favicon") || 
                   path.Contains("/_framework") || path.Contains("/lib/");
        }
    }

    /// <summary>
    /// Extension methods for error tracking middleware
    /// </summary>
    public static class ErrorTrackingMiddlewareExtensions
    {
        /// <summary>
        /// Add error tracking middleware to the pipeline
        /// </summary>
        public static IApplicationBuilder UseErrorTracking(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorTrackingMiddleware>();
        }
    }
}
