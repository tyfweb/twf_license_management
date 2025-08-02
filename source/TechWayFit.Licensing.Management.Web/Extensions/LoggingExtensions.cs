using Microsoft.Extensions.Logging;

namespace TechWayFit.Licensing.Management.Web.Extensions
{
    /// <summary>
    /// Extension methods for structured logging with Microsoft.Extensions.Logging
    /// </summary>
    public static class LoggingExtensions
    {
        /// <summary>
        /// Log user authentication events
        /// </summary>
        public static void LogUserAuthentication(this Microsoft.Extensions.Logging.ILogger logger, string username, bool success, string? ipAddress = null)
        {
            if (success)
            {
                logger.LogInformation("User authentication successful for {Username} from {IpAddress}", 
                    username, ipAddress ?? "Unknown");
            }
            else
            {
                logger.LogWarning("User authentication failed for {Username} from {IpAddress}", 
                    username, ipAddress ?? "Unknown");
            }
        }

        /// <summary>
        /// Log license operations
        /// </summary>
        public static void LogLicenseOperation(this Microsoft.Extensions.Logging.ILogger logger, string operation, string licenseId, string? userId = null, bool success = true, Exception? exception = null)
        {
            if (exception != null)
            {
                logger.LogError(exception, "License operation {Operation} for license {LicenseId} by user {UserId} failed", 
                    operation, licenseId, userId ?? "System");
            }
            else if (success)
            {
                logger.LogInformation("License operation {Operation} for license {LicenseId} by user {UserId} succeeded", 
                    operation, licenseId, userId ?? "System");
            }
            else
            {
                logger.LogError("License operation {Operation} for license {LicenseId} by user {UserId} failed", 
                    operation, licenseId, userId ?? "System");
            }
        }

        /// <summary>
        /// Log database operations
        /// </summary>
        public static void LogDatabaseOperation(this Microsoft.Extensions.Logging.ILogger logger, string operation, string entityType, string? entityId = null, bool success = true, Exception? exception = null)
        {
            if (exception != null)
            {
                logger.LogError(exception, "Database operation {Operation} on {EntityType} {EntityId} failed", 
                    operation, entityType, entityId ?? "Unknown");
            }
            else if (success)
            {
                logger.LogInformation("Database operation {Operation} on {EntityType} {EntityId} succeeded", 
                    operation, entityType, entityId ?? "Unknown");
            }
            else
            {
                logger.LogError("Database operation {Operation} on {EntityType} {EntityId} failed", 
                    operation, entityType, entityId ?? "Unknown");
            }
        }

        /// <summary>
        /// Log API requests with performance metrics
        /// </summary>
        public static void LogApiRequest(this Microsoft.Extensions.Logging.ILogger logger, string method, string path, int statusCode, long elapsedMs, string? userId = null)
        {
            var logLevel = statusCode >= 500 ? LogLevel.Error : 
                          statusCode >= 400 ? LogLevel.Warning : 
                          LogLevel.Information;
            
            logger.Log(logLevel, "API {Method} {Path} returned {StatusCode} in {ElapsedMs}ms for user {UserId}", 
                method, path, statusCode, elapsedMs, userId ?? "Anonymous");
        }

        /// <summary>
        /// Log security events
        /// </summary>
        public static void LogSecurityEvent(this Microsoft.Extensions.Logging.ILogger logger, string eventType, string? userId = null, string? details = null, string? ipAddress = null)
        {
            logger.LogWarning("Security event {EventType} for user {UserId} from {IpAddress}: {Details}", 
                eventType, userId ?? "Unknown", ipAddress ?? "Unknown", details ?? "No additional details");
        }

        /// <summary>
        /// Log business rule violations
        /// </summary>
        public static void LogBusinessRuleViolation(this Microsoft.Extensions.Logging.ILogger logger, string rule, string? context = null, string? userId = null)
        {
            logger.LogWarning("Business rule violation: {Rule} in context {Context} by user {UserId}", 
                rule, context ?? "Unknown", userId ?? "Unknown");
        }

        /// <summary>
        /// Log system performance metrics
        /// </summary>
        public static void LogPerformanceMetric(this Microsoft.Extensions.Logging.ILogger logger, string operation, long elapsedMs, string? additionalInfo = null)
        {
            var logLevel = elapsedMs > 5000 ? LogLevel.Warning : LogLevel.Debug;
            
            logger.Log(logLevel, "Performance metric: {Operation} completed in {ElapsedMs}ms {AdditionalInfo}", 
                operation, elapsedMs, additionalInfo ?? "");
        }

        /// <summary>
        /// Log configuration changes
        /// </summary>
        public static void LogConfigurationChange(this Microsoft.Extensions.Logging.ILogger logger, string setting, string? oldValue, string? newValue, string? userId = null)
        {
            logger.LogInformation("Configuration changed: {Setting} from '{OldValue}' to '{NewValue}' by user {UserId}", 
                setting, oldValue ?? "null", newValue ?? "null", userId ?? "System");
        }

        /// <summary>
        /// Log external service calls
        /// </summary>
        public static void LogExternalServiceCall(this Microsoft.Extensions.Logging.ILogger logger, string serviceName, string operation, bool success, long elapsedMs, Exception? exception = null)
        {
            if (exception != null)
            {
                logger.LogError(exception, "External service call to {ServiceName} for {Operation} failed after {ElapsedMs}ms", 
                    serviceName, operation, elapsedMs);
            }
            else if (success)
            {
                logger.LogInformation("External service call to {ServiceName} for {Operation} succeeded in {ElapsedMs}ms", 
                    serviceName, operation, elapsedMs);
            }
            else
            {
                logger.LogError("External service call to {ServiceName} for {Operation} failed in {ElapsedMs}ms", 
                    serviceName, operation, elapsedMs);
            }
        }
    }
}
