using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Data;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Configuration;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Extensions;

/// <summary>
/// Base service collection extensions for Entity Framework providers
/// Provider-specific implementations should call AddEfCoreInfrastructureBase with their own provider configuration
/// </summary>
public static class EfCoreServiceExtensions
{
    /// <summary>
    /// Add Entity Framework infrastructure services (provider-agnostic base)
    /// Use this method from provider-specific extensions
    /// </summary>
    public static IServiceCollection AddEfCoreInfrastructureBase(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        if (configureDbContext == null)
        {
            throw new ArgumentNullException(nameof(configureDbContext));
        }

        // Add DbContext with provider-specific configuration
        services.AddDbContext<EfCoreLicensingDbContext>((serviceProvider, options) =>
        {
            // Apply provider-specific configuration
            configureDbContext(options);

            // Enable performance optimizations
            options.EnableServiceProviderCaching();
        });

        // TODO: Add health checks configuration when package is referenced
        // services.AddHealthChecks().AddDbContextCheck<EfCoreLicensingDbContext>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, EfCoreUnitOfWork>();

        return services;
    }

    /// <summary>
    /// Add Entity Framework infrastructure with configuration-based setup
    /// Provider-specific implementations should override this
    /// </summary>
    public static IServiceCollection AddEfCoreInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration,
        string connectionStringKey = "Default")
    {
        var connectionString = configuration.GetConnectionString(connectionStringKey);
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{connectionStringKey}' is not configured.");
        }

        // Configure database options
        var databaseConfig = configuration.GetSection("Database").Get<DatabaseConfiguration>() 
                            ?? new DatabaseConfiguration();

        return services.AddEfCoreInfrastructureBase(options =>
        {
            // Base configuration - provider implementations should override this method
            // and call AddEfCoreInfrastructureBase with their specific provider setup
            throw new NotImplementedException(
                "This method should be overridden by provider-specific implementations. " +
                "Use AddEfCoreInfrastructureBase with proper provider configuration instead.");
        });
    }

    /// <summary>
    /// Add Entity Framework infrastructure with direct connection string
    /// Provider-specific implementations should override this
    /// </summary>
    public static IServiceCollection AddEfCoreInfrastructure(
        this IServiceCollection services, 
        string connectionString,
        DatabaseOptions? options = null)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
        }

        options ??= new DatabaseOptions();

        return services.AddEfCoreInfrastructureBase(dbOptions =>
        {
            // Base configuration - provider implementations should override this method
            throw new NotImplementedException(
                "This method should be overridden by provider-specific implementations. " +
                "Use AddEfCoreInfrastructureBase with proper provider configuration instead.");
        });
    }
}
