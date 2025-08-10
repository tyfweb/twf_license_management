using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Extensions;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Configuration;

namespace TechWayFit.Licensing.Management.Infrastructure.InMemory.Extensions;

/// <summary>
/// Service collection extensions for InMemory database provider
/// </summary>
public static class InMemoryServiceExtensions
{
    /// <summary>
    /// Add InMemory database infrastructure services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <param name="databaseName">Optional database name (defaults to "LicensingDb")</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddInMemoryInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration,
        string databaseName = "LicensingDb")
    {
        // Configure database options
        var databaseConfig = configuration.GetSection("Database").Get<DatabaseConfiguration>() 
                            ?? new DatabaseConfiguration();

        return services.AddEfCoreInfrastructureBase(options =>
        {
            options.UseInMemoryDatabase(databaseName);

            // InMemory-specific configurations
            if (databaseConfig.Options.EnableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
            }

            if (databaseConfig.Options.EnableLogging)
            {
                options.EnableDetailedErrors();
                options.LogTo(Console.WriteLine);
            }
        });
    }

    /// <summary>
    /// Add InMemory database infrastructure services with custom database name
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="databaseName">Database name for the InMemory provider</param>
    /// <param name="options">Database options</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddInMemoryInfrastructure(
        this IServiceCollection services, 
        string databaseName = "LicensingDb",
        DatabaseOptions? options = null)
    {
        options ??= new DatabaseOptions();

        return services.AddEfCoreInfrastructureBase(dbOptions =>
        {
            dbOptions.UseInMemoryDatabase(databaseName);

            // Apply common options
            if (options.EnableSensitiveDataLogging)
            {
                dbOptions.EnableSensitiveDataLogging();
            }

            if (options.EnableLogging)
            {
                dbOptions.EnableDetailedErrors();
                dbOptions.LogTo(Console.WriteLine);
            }
        });
    }

    /// <summary>
    /// Add InMemory database infrastructure for testing with a unique database name
    /// This ensures each test gets its own isolated database
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="testName">Optional test name for unique database naming</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddInMemoryInfrastructureForTesting(
        this IServiceCollection services,
        string? testName = null)
    {
        // Generate unique database name for test isolation
        var databaseName = testName != null 
            ? $"LicensingDb_Test_{testName}_{Guid.NewGuid():N}"
            : $"LicensingDb_Test_{Guid.NewGuid():N}";

        var options = new DatabaseOptions
        {
            EnableSensitiveDataLogging = true,
            EnableLogging = false // Typically don't want verbose logging in tests
        };

        return services.AddInMemoryInfrastructure(databaseName, options);
    }

    /// <summary>
    /// Add InMemory infrastructure for integration testing with seeded data
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="seedData">Action to seed test data</param>
    /// <param name="testName">Optional test name for unique database naming</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddInMemoryInfrastructureWithSeededData(
        this IServiceCollection services,
        Action<IServiceProvider> seedData,
        string? testName = null)
    {
        services.AddInMemoryInfrastructureForTesting(testName);
        
        // Add a hosted service to seed data after the context is created
        services.AddSingleton(seedData);
        
        return services;
    }
}
