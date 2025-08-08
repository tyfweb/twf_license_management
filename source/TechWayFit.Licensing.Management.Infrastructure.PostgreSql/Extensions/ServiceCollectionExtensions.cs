using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Data;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Extensions;

/// <summary>
/// Extension methods for configuring PostgreSQL database services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds PostgreSQL database services to the service collection using configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <param name="configureOptions">Optional action to configure PostgreSQL options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddPostgreSqlDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<PostgreSqlOptions>? configureOptions = null)
    {
        // Configure PostgreSQL options
        var optionsBuilder = services.AddOptions<PostgreSqlOptions>()
            .Bind(configuration.GetSection(PostgreSqlOptions.SectionName));

        if (configureOptions != null)
        {
            optionsBuilder.Configure(configureOptions);
        }

        // Register the PostgreSQL DbContext
        services.AddDbContext<PostgreSqlLicensingDbContext>((serviceProvider, options) =>
        {
            var postgresOptions = serviceProvider.GetRequiredService<IOptions<PostgreSqlOptions>>().Value;
            
            options.UseNpgsql(postgresOptions.ConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: postgresOptions.MaxRetryCount,
                    maxRetryDelay: TimeSpan.FromSeconds(postgresOptions.MaxRetryDelaySeconds),
                    errorCodesToAdd: null);
                
                npgsqlOptions.CommandTimeout(postgresOptions.CommandTimeoutSeconds);
                npgsqlOptions.MigrationsHistoryTable(postgresOptions.MigrationsTableName, postgresOptions.SchemaName);
            });

            // Configure logging and debugging options
            options.EnableSensitiveDataLogging(postgresOptions.EnableSensitiveDataLogging);
            options.EnableDetailedErrors(postgresOptions.EnableDetailedErrors);

            if (postgresOptions.EnableConsoleLogging)
            {
                options.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
            }
        });

        // Register the Unit of Work
        services.AddScoped<IUnitOfWork, PostgreSqlUnitOfWork>();

        return services;
    }

    /// <summary>
    /// Adds PostgreSQL database services to the service collection with explicit connection string
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">The PostgreSQL connection string</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddPostgreSqlDatabase(
        this IServiceCollection services,
        string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
        }

        // Register the PostgreSQL DbContext
        services.AddDbContext<PostgreSqlLicensingDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
                
                npgsqlOptions.CommandTimeout(30);
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "licensing");
            });

            // Enable sensitive data logging in development
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
        });

        // Register the Unit of Work
        services.AddScoped<IUnitOfWork, PostgreSqlUnitOfWork>();

        return services;
    }

    /// <summary>
    /// Adds PostgreSQL database services for development/testing with additional logging
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">The PostgreSQL connection string</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddPostgreSqlDatabaseForDevelopment(
        this IServiceCollection services,
        string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
        }

        // Register the PostgreSQL DbContext with development settings
        services.AddDbContext<PostgreSqlLicensingDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
                
                npgsqlOptions.CommandTimeout(30);
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "licensing");
            });

            // Enable additional logging and debugging in development
            options.EnableSensitiveDataLogging(true);
            options.EnableDetailedErrors(true);
            options.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
        });

        // Register the Unit of Work
        services.AddScoped<IUnitOfWork, PostgreSqlUnitOfWork>();

        return services;
    }
}
