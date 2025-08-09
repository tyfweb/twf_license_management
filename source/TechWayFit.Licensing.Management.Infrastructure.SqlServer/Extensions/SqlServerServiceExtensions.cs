using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Extensions;

namespace TechWayFit.Licensing.Management.Infrastructure.SqlServer.Extensions;

/// <summary>
/// Extension methods for configuring SQL Server infrastructure services
/// </summary>
public static class SqlServerServiceExtensions
{
    /// <summary>
    /// Adds SQL Server infrastructure services to the DI container using connection string
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">The SQL Server connection string</param>
    /// <param name="configureOptions">Optional configuration for additional DbContext options</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddSqlServerInfrastructure(
        this IServiceCollection services,
        string connectionString,
        Action<DbContextOptionsBuilder>? configureOptions = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

        // Add the EF Core base infrastructure with SQL Server provider
        services.AddEfCoreInfrastructureBase(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                // Configure SQL Server specific options
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
                
                // Set command timeout
                sqlOptions.CommandTimeout(30);
                
                // Enable sensitive data logging in development
                sqlOptions.MigrationsAssembly("TechWayFit.Licensing.Management.Infrastructure.SqlServer");
            });

            // Apply additional configuration if provided
            configureOptions?.Invoke(options);
        });

        return services;
    }

    /// <summary>
    /// Adds SQL Server infrastructure services to the DI container using configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <param name="connectionStringName">The name of the connection string in configuration (default: "DefaultConnection")</param>
    /// <param name="configureOptions">Optional configuration for additional DbContext options</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddSqlServerInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "DefaultConnection",
        Action<DbContextOptionsBuilder>? configureOptions = null)
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException($"Connection string '{connectionStringName}' not found in configuration");

        return services.AddSqlServerInfrastructure(connectionString, configureOptions);
    }

    /// <summary>
    /// Adds SQL Server infrastructure services with advanced configuration options
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">The SQL Server connection string</param>
    /// <param name="retryCount">Maximum number of retry attempts</param>
    /// <param name="maxRetryDelay">Maximum delay between retries</param>
    /// <param name="commandTimeout">Command timeout in seconds</param>
    /// <param name="enableSensitiveDataLogging">Enable sensitive data logging</param>
    /// <param name="enableDetailedErrors">Enable detailed error messages</param>
    /// <param name="configureOptions">Optional configuration for additional DbContext options</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddSqlServerInfrastructureWithOptions(
        this IServiceCollection services,
        string connectionString,
        int retryCount = 3,
        TimeSpan? maxRetryDelay = null,
        int commandTimeout = 30,
        bool enableSensitiveDataLogging = false,
        bool enableDetailedErrors = false,
        Action<DbContextOptionsBuilder>? configureOptions = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

        var retryDelay = maxRetryDelay ?? TimeSpan.FromSeconds(5);

        // Add the EF Core base infrastructure with SQL Server provider and advanced options
        services.AddEfCoreInfrastructureBase(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                // Configure retry policy
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: retryCount,
                    maxRetryDelay: retryDelay,
                    errorNumbersToAdd: null);
                
                // Set command timeout
                sqlOptions.CommandTimeout(commandTimeout);
                
                // Set migrations assembly
                sqlOptions.MigrationsAssembly("TechWayFit.Licensing.Management.Infrastructure.SqlServer");
            });

            // Configure logging and error handling
            if (enableSensitiveDataLogging)
                options.EnableSensitiveDataLogging();
                
            if (enableDetailedErrors)
                options.EnableDetailedErrors();

            // Apply additional configuration if provided
            configureOptions?.Invoke(options);
        });

        return services;
    }

    /// <summary>
    /// Adds SQL Server infrastructure services for testing with in-memory options when specified
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">The SQL Server connection string</param>
    /// <param name="useInMemoryForTesting">If true, uses in-memory database for testing</param>
    /// <param name="testDatabaseName">Name for the test database (when using in-memory)</param>
    /// <param name="configureOptions">Optional configuration for additional DbContext options</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddSqlServerInfrastructureForTesting(
        this IServiceCollection services,
        string connectionString,
        bool useTestDatabase = false,
        string testDatabaseName = "TestLicensingDb",
        Action<DbContextOptionsBuilder>? configureOptions = null)
    {
        if (useTestDatabase)
        {
            // Use separate SQL Server database for testing  
            var testConnectionString = connectionString.Replace("Initial Catalog=", $"Initial Catalog={testDatabaseName}_");
            services.AddEfCoreInfrastructureBase(options =>
            {
                options.UseSqlServer(testConnectionString);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
                configureOptions?.Invoke(options);
            });
        }
        else
        {
            // Use SQL Server with testing optimizations
            services.AddSqlServerInfrastructureWithOptions(
                connectionString,
                retryCount: 1,
                maxRetryDelay: TimeSpan.FromSeconds(1),
                commandTimeout: 10,
                enableSensitiveDataLogging: true,
                enableDetailedErrors: true,
                configureOptions);
        }

        return services;
    }

    /// <summary>
    /// Adds SQLite infrastructure services to the DI container for local development
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="databasePath">The path to the SQLite database file (optional, defaults to "licensing.db")</param>
    /// <param name="configureOptions">Optional configuration for additional DbContext options</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddSqliteInfrastructure(
        this IServiceCollection services,
        string? databasePath = null,
        Action<DbContextOptionsBuilder>? configureOptions = null)
    {
        // Use default path if not provided
        databasePath ??= "licensing.db";

        // Create the connection string
        var connectionString = $"Data Source={databasePath}";

        // Add the EF Core base infrastructure with SQLite provider
        services.AddEfCoreInfrastructureBase(options =>
        {
            options.UseSqlite(connectionString, sqliteOptions =>
            {
                // Configure SQLite specific options
                sqliteOptions.CommandTimeout(30);
                
                // Set migrations assembly
                sqliteOptions.MigrationsAssembly("TechWayFit.Licensing.Management.Infrastructure.SqlServer");
            });

            // Enable detailed logging for development
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            options.LogTo(Console.WriteLine, LogLevel.Information);

            // Apply additional configuration if provided
            configureOptions?.Invoke(options);
        });

        return services;
    }
}
