using Hangfire;
using Hangfire.MemoryStorage;
using TechWayFit.Licensing.Management.Infrastructure.Extensions;
using TechWayFit.Licensing.Management.Infrastructure.InMemory.Extensions;
using TechWayFit.Licensing.Management.Infrastructure.SqlServer.Extensions;

namespace TechWayFit.Licensing.Management.Web.Extensions;

/// <summary>
/// Extension methods for configuring infrastructure services (database, caching, background jobs)
/// </summary>
public static class InfrastructureExtensions
{
    /// <summary>
    /// Adds infrastructure services including database, caching, session, and background jobs
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    /// <param name="configuration">The application configuration</param>
    /// <returns>The configured service collection</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add memory cache
        services.AddMemoryCache();

        // Configure session
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite = SameSiteMode.Lax;
        });

        // Configure database infrastructure (SQLite for development, can be swapped for production)
       // services.AddSqliteInfrastructure("licensing.db");
        services.AddInMemoryInfrastructure();
        services.AddHttpContextAccessor();

        // Configure Hangfire for background job processing
        services.AddHangfire(hangfireConfig => hangfireConfig
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseMemoryStorage());

        services.AddHangfireServer();

        // Register seeding services
        services.AddSeedingServices();

        return services;
    }
}
