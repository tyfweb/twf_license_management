using Hangfire;
using Hangfire.Dashboard;
using TechWayFit.Licensing.Management.Web.Middleware;

namespace TechWayFit.Licensing.Management.Web.Extensions;

/// <summary>
/// Extension methods for configuring application middleware pipeline
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Configures the complete middleware pipeline for the application
    /// </summary>
    /// <param name="app">The web application</param>
    /// <param name="environment">The hosting environment</param>
    /// <returns>The configured web application</returns>
    public static WebApplication ConfigureMiddlewarePipeline(this WebApplication app, IWebHostEnvironment environment)
    {
        // Add correlation ID middleware first to ensure all logs have correlation ID
        app.UseCorrelationId();

        // Use Serilog for request logging with correlation ID support
        app.ConfigureSerilogRequestLogging();

        // Configure the HTTP request pipeline
        if (environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TechWayFit Licensing Management API v1");
                c.RoutePrefix = "swagger";
            });
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        // Configure Hangfire dashboard (restrict to authorized users in production)
        var hangfireOptions = new DashboardOptions
        {
            Authorization = environment.IsDevelopment() 
                ? new[] { new HangfireAuthorizationFilter() } 
                : new[] { new HangfireAuthorizationFilter() }, // Add proper authorization in production
            DisplayStorageConnectionString = false,
            DarkModeEnabled = false,
            DefaultRecordsPerPage = 20,
            StatsPollingInterval = 5000 // 5 second refresh
        };
        
        app.UseHangfireDashboard("/hangfire", hangfireOptions);

        // Add session middleware (must be before authentication)
        app.UseSession();

        // Add authentication middleware
        app.UseAuthentication();
        app.UseAuthorization();

        // Configure routing
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        return app;
    }
}

/// <summary>
/// Simple authorization filter for Hangfire dashboard in development
/// In production, implement proper authorization based on your requirements
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // In development, allow all access
        // In production, implement proper authorization logic
        return true;
    }
}
