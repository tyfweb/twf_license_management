using Serilog;
using TechWayFit.Licensing.Management.Web.Middleware;

namespace TechWayFit.Licensing.Management.Web.Extensions;

/// <summary>
/// Extension methods for configuring Serilog logging services
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    /// Configures Serilog bootstrap logging for startup
    /// </summary>
    /// <returns>A configured LoggerConfiguration for bootstrap logging</returns>
    public static LoggerConfiguration ConfigureBootstrapLogging(this LoggerConfiguration loggerConfiguration)
    {
        return loggerConfiguration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {NewLine}{Exception}");
    }

    /// <summary>
    /// Configures comprehensive Serilog logging for the application
    /// </summary>
    /// <param name="builder">The web application builder</param>
    /// <returns>The configured web application builder</returns>
    public static WebApplicationBuilder ConfigureSerilog(this WebApplicationBuilder builder)
    {
        // Clear default logging providers and use Serilog
        builder.Logging.ClearProviders();
        
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "TechWayFit.Licensing.Management")
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j} {NewLine}{Exception}")
            .WriteTo.File("logs/app-.log", 
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j} {NewLine}{Exception}"));

        return builder;
    }

    /// <summary>
    /// Configures Serilog request logging with correlation ID support
    /// </summary>
    /// <param name="app">The web application</param>
    /// <returns>The configured web application</returns>
    public static WebApplication ConfigureSerilogRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.GetLevel = (httpContext, elapsed, ex) => ex != null
                ? Serilog.Events.LogEventLevel.Error
                : elapsed > 1000
                    ? Serilog.Events.LogEventLevel.Warning
                    : Serilog.Events.LogEventLevel.Information;
            
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                
                if (httpContext.Request.Headers.ContainsKey("X-Correlation-ID"))
                {
                    diagnosticContext.Set("CorrelationId", httpContext.Request.Headers["X-Correlation-ID"].ToString());
                }
            };
        });

        return app;
    }
}
