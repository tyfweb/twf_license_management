using Serilog;
using Serilog.Core;
using Serilog.Events;
using Microsoft.AspNetCore.Builder;
using Serilog.Filters;

namespace TechWayFit.Licensing.Management.Web.Extensions
{
    /// <summary>
    /// Extension methods for configuring Serilog request logging
    /// </summary>
    public static class SerilogRequestLoggingExtensions
    {
        /// <summary>
        /// Configure Serilog request logging with correlation ID and detailed context
        /// </summary>
        public static void ConfigureSerilogRequestLogging(this IApplicationBuilder app)
        {
            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
                options.GetLevel = (httpContext, elapsed, ex) => ex != null
                    ? LogEventLevel.Error
                    : httpContext.Response.StatusCode > 499
                        ? LogEventLevel.Error
                        : LogEventLevel.Information;
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                    diagnosticContext.Set("RequestPath", httpContext.Request.Path.Value ?? "/");
                    diagnosticContext.Set("RequestMethod", httpContext.Request.Method);
                    var userAgent = httpContext.Request.Headers["User-Agent"].FirstOrDefault();
                    diagnosticContext.Set("UserAgent", userAgent ?? "Unknown");
                    var correlationId = httpContext.Response.Headers["X-Correlation-ID"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(correlationId))
                    {
                        diagnosticContext.Set("CorrelationId", correlationId);
                    }
                };
            });
        }
    }
    
    /// <summary>
    /// Extension methods for configuring Serilog with separate log files and correlation IDs
    /// </summary>
    public static class SerilogConfigurationExtensions
    {
        static string TimeStampFolder => DateTime.UtcNow.ToString("yyyy-MM-dd");
        /// <summary>
        /// Configure Serilog with separate log files for SQL queries, requests, and general application logs
        /// </summary>
        public static LoggerConfiguration ConfigureStructuredLogging(this LoggerConfiguration loggerConfiguration, bool isDevelopment = false)
        {
            var baseOutputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}";
            var consoleOutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}";

            return loggerConfiguration
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "TechWayFit.Licensing.Management")

                // Console output with correlation ID
                .WriteTo.Console(outputTemplate: consoleOutputTemplate)

                // SQL queries log file - only SQL interceptor and EF Core database logs
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e =>
                        e.Properties.ContainsKey("SourceContext") &&
                        (e.Properties["SourceContext"].ToString().Contains("SqlLoggingInterceptor") ||
                         e.Properties["SourceContext"].ToString().Contains("Microsoft.EntityFrameworkCore.Database.Command") ||
                         e.Properties["SourceContext"].ToString().Contains("Microsoft.EntityFrameworkCore.Model.Validation")))
                    .WriteTo.File($"Logs/{TimeStampFolder}/sql-queries-.log",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30,
                        outputTemplate: baseOutputTemplate))

                // Request logs file - only request logging middleware
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e =>
                        e.Properties.ContainsKey("SourceContext") &&
                        (e.Properties["SourceContext"].ToString().Contains("Serilog.AspNetCore.RequestLoggingMiddleware") ||
                         e.Properties["SourceContext"].ToString().Contains("Microsoft.AspNetCore.Hosting.Diagnostics")))
                    .WriteTo.File($"Logs/{TimeStampFolder}/requests-.log",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30,
                        outputTemplate: baseOutputTemplate))

                // Application logs - exclude SQL, requests, and Microsoft framework logs
                .WriteTo.Logger(lc => lc
                    .Filter.ByExcluding(e =>
                        e.Properties.ContainsKey("SourceContext") &&
                        (e.Properties["SourceContext"].ToString().Contains("SqlLoggingInterceptor") ||
                         e.Properties["SourceContext"].ToString().Contains("Microsoft.EntityFrameworkCore") ||
                         e.Properties["SourceContext"].ToString().Contains("Serilog.AspNetCore.RequestLoggingMiddleware") ||
                         e.Properties["SourceContext"].ToString().Contains("Microsoft.AspNetCore.Hosting.Diagnostics") ||
                         e.Properties["SourceContext"].ToString().Contains("Microsoft.AspNetCore.Authentication") ||
                         e.Properties["SourceContext"].ToString().Contains("Microsoft.AspNetCore.Authorization") ||
                         e.Properties["SourceContext"].ToString().Contains("Microsoft.AspNetCore.Routing") ||
                         e.Properties["SourceContext"].ToString().Contains("Microsoft.AspNetCore.Mvc") ||
                         e.Properties["SourceContext"].ToString().Contains("Microsoft.AspNetCore.StaticFiles") ||
                         e.Properties["SourceContext"].ToString().Contains("Microsoft.AspNetCore.DataProtection") ||
                         e.Properties["SourceContext"].ToString().Contains("Microsoft.Hosting.Lifetime") ||
                         e.Properties["SourceContext"].ToString().Contains("Microsoft.AspNetCore.Server.Kestrel")))
                    .WriteTo.File($"Logs/{TimeStampFolder}/application-.log",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30,
                        outputTemplate: baseOutputTemplate))

                // Error logs file (all errors from any source)
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Error)
                    .WriteTo.File($"Logs/{TimeStampFolder}/errors-.log",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 90,
                        outputTemplate: baseOutputTemplate));
        }

        /// <summary>
        /// Configure structured logging for startup/bootstrap phase
        /// </summary>
        public static LoggerConfiguration ConfigureBootstrapLogging(this LoggerConfiguration loggerConfiguration)
        {
            var outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}";
            var consoleOutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";

            return loggerConfiguration
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: consoleOutputTemplate)
                .WriteTo.File($"Logs/{TimeStampFolder}/startup-.log",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: outputTemplate);
        }
    }

    /// <summary>
    /// Custom enricher to add SQL-specific properties to log events
    /// </summary>
    public class SqlLogEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent.Properties.ContainsKey("SourceContext"))
            {
                var sourceContext = logEvent.Properties["SourceContext"].ToString();
                if (sourceContext.Contains("SqlLoggingInterceptor") || sourceContext.Contains("Microsoft.EntityFrameworkCore"))
                {
                    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("LogCategory", "SQL"));
                }
            }
        }
    }
}
