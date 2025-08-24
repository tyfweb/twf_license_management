using StackExchange.Profiling;
using StackExchange.Profiling.Storage;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Data.Sqlite;

namespace TechWayFit.Licensing.Management.Web.Extensions;

/// <summary>
/// Extension methods for configuring application profiling and performance monitoring
/// </summary>
public static class ProfilingExtensions
{
    /// <summary>
    /// Adds MiniProfiler services for performance profiling in development
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    /// <param name="environment">The hosting environment</param>
    /// <returns>The configured service collection</returns>
    public static IServiceCollection AddProfilingServices(this IServiceCollection services, IWebHostEnvironment environment)
    {
        // Only enable profiling in development environment
        if (environment.IsDevelopment())
        {
            services.AddMiniProfiler(options =>
            {
                // Set the route base path for accessing profiler results
                options.RouteBasePath = "/profiler";
                
                // Configure popup position (BottomLeft, BottomRight, TopLeft, TopRight)
                options.PopupRenderPosition = RenderPosition.BottomLeft;
                
                // Configure color scheme (Auto, Light, Dark)
                options.ColorScheme = ColorScheme.Auto;
                
                // Configure popup behavior - Note: PopupToggleKeyboard is deprecated in newer versions
                // options.PopupToggleKeyboard = "Alt+P";
                options.PopupStartHidden = false;
                options.PopupDecimalPlaces = 1;
                
                // Configure storage settings - use default memory storage
                // options.Storage will use default MemoryCacheStorage if not specified
                
                // Configure sampling settings for high-traffic scenarios
                options.ShouldProfile = request =>
                {
                    // Always profile in development
                    if (environment.IsDevelopment())
                        return true;
                    
                    // Could add conditional logic for production sampling here
                    return false;
                };
                
                // Configure what to track
                options.TrackConnectionOpenClose = true;
                options.StackMaxLength = 5000;
                
                // Configure ignored paths to reduce noise
                options.IgnoredPaths.Add("/favicon.ico");
                options.IgnoredPaths.Add("/profiler");
                options.IgnoredPaths.Add("/css");
                options.IgnoredPaths.Add("/js");
                options.IgnoredPaths.Add("/images");
                options.IgnoredPaths.Add("/lib");
            })
            .AddEntityFramework(); // Enable Entity Framework profiling
        }

        return services;
    }

    /// <summary>
    /// Configures the MiniProfiler middleware in the request pipeline
    /// </summary>
    /// <param name="app">The web application</param>
    /// <param name="environment">The hosting environment</param>
    /// <returns>The configured web application</returns>
    public static WebApplication UseProfilingMiddleware(this WebApplication app, IWebHostEnvironment environment)
    {
        // Only use profiling middleware in development
        if (environment.IsDevelopment())
        {
            // Add MiniProfiler middleware early in pipeline but after developer exception page
            app.UseMiniProfiler();
        }

        return app;
    }
}
