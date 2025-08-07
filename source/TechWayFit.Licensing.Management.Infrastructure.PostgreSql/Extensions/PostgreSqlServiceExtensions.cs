using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Data;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Configuration;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Extensions;

/// <summary>
/// Service collection extensions for PostgreSQL provider
/// </summary>
public static class PostgreSqlServiceExtensions
{
    /// <summary>
    /// Add PostgreSQL infrastructure services
    /// </summary>
    public static IServiceCollection AddPostgreSqlInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Configure database options
        var databaseConfig = configuration.GetSection("Database").Get<DatabaseConfiguration>() 
                            ?? new DatabaseConfiguration();
        
        var connectionString = configuration.GetConnectionString("PostgreSQL") 
                              ?? databaseConfig.ConnectionString;

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("PostgreSQL connection string is not configured.");
        }

        // TODO: Add connection string security validation

        // Add DbContext with performance optimizations
        services.AddDbContext<PostgreSqlPostgreSqlLicensingDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                // Configure PostgreSQL-specific options
                if (!databaseConfig.Options.EnableMigrations)
                {
                    npgsqlOptions.MigrationsAssembly((string?)null);
                }
                
                if (databaseConfig.Options.CommandTimeout > 0)
                {
                    npgsqlOptions.CommandTimeout(databaseConfig.Options.CommandTimeout);
                }

                // Enable retry on failure for resilience
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            })
            // Use snake_case naming convention for PostgreSQL
            .UseSnakeCaseNamingConvention()
            // Enable performance optimizations
            .EnableServiceProviderCaching()
            .EnableSensitiveDataLogging(databaseConfig.Options.EnableSensitiveDataLogging);

            // Development-specific configurations
            if (databaseConfig.Options.EnableLogging)
            {
                options.EnableDetailedErrors();
                // TODO: Configure logging properly via Microsoft.Extensions.Logging
                options.LogTo(Console.WriteLine);
            }
        });

        // TODO: Add health checks, resilience policies, and query caching
        // These require additional packages and configuration

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, PostgreSqlUnitOfWork>();

        return services;
    }

    /// <summary>
    /// Add PostgreSQL infrastructure services with custom connection string
    /// </summary>
    public static IServiceCollection AddPostgreSqlInfrastructure(
        this IServiceCollection services, 
        string connectionString,
        DatabaseOptions? options = null)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
        }

        options ??= new DatabaseOptions();

        // Add DbContext
        services.AddDbContext<PostgreSqlPostgreSqlLicensingDbContext>(dbOptions =>
        {
            dbOptions.UseNpgsql(connectionString, npgsqlOptions =>
            {
                if (!options.EnableMigrations)
                {
                    npgsqlOptions.MigrationsAssembly((string?)null);
                }
                
                if (options.CommandTimeout > 0)
                {
                    npgsqlOptions.CommandTimeout(options.CommandTimeout);
                }
            })
            .UseSnakeCaseNamingConvention();

            if (options.EnableSensitiveDataLogging)
            {
                dbOptions.EnableSensitiveDataLogging();
            }

            if (options.EnableLogging)
            {
                dbOptions.EnableDetailedErrors();
            }
        });

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, PostgreSqlUnitOfWork>();

        return services;
    }
}
