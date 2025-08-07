using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Data;

namespace TechWayFit.Licensing.Management.Infrastructure.SqlServer.Helpers;

/// <summary>
/// Helper class for SQL Server database operations and migrations
/// </summary>
public static class SqlServerDatabaseHelper
{
    /// <summary>
    /// Ensures the database is created and applies pending migrations
    /// </summary>
    /// <param name="serviceProvider">Service provider for resolving dependencies</param>
    /// <param name="logger">Logger for diagnostics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    public static async Task<bool> EnsureDatabaseCreatedAsync(
        IServiceProvider serviceProvider,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LicensingDbContext>();

            logger?.LogInformation("Ensuring SQL Server database is created...");

            // Create database if it doesn't exist
            var created = await context.Database.EnsureCreatedAsync(cancellationToken);
            if (created)
            {
                logger?.LogInformation("SQL Server database was created successfully");
            }
            else
            {
                logger?.LogInformation("SQL Server database already exists");
            }

            return true;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to ensure SQL Server database is created");
            return false;
        }
    }

    /// <summary>
    /// Applies pending migrations to the database
    /// </summary>
    /// <param name="serviceProvider">Service provider for resolving dependencies</param>
    /// <param name="logger">Logger for diagnostics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    public static async Task<bool> ApplyMigrationsAsync(
        IServiceProvider serviceProvider,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LicensingDbContext>();

            logger?.LogInformation("Checking for pending SQL Server migrations...");

            var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
            var pendingMigrationsList = pendingMigrations.ToList();

            if (pendingMigrationsList.Any())
            {
                logger?.LogInformation("Applying {Count} pending migrations: {Migrations}", 
                    pendingMigrationsList.Count, string.Join(", ", pendingMigrationsList));

                await context.Database.MigrateAsync(cancellationToken);
                
                logger?.LogInformation("Successfully applied all pending migrations");
            }
            else
            {
                logger?.LogInformation("No pending migrations found");
            }

            return true;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to apply SQL Server migrations");
            return false;
        }
    }

    /// <summary>
    /// Gets information about database migrations
    /// </summary>
    /// <param name="serviceProvider">Service provider for resolving dependencies</param>
    /// <param name="logger">Logger for diagnostics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Migration information or null if failed</returns>
    public static async Task<MigrationInfo?> GetMigrationInfoAsync(
        IServiceProvider serviceProvider,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LicensingDbContext>();

            var appliedMigrations = await context.Database.GetAppliedMigrationsAsync(cancellationToken);
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);

            var info = new MigrationInfo
            {
                AppliedMigrations = appliedMigrations.ToList(),
                PendingMigrations = pendingMigrations.ToList(),
                CanConnect = await context.Database.CanConnectAsync(cancellationToken)
            };

            logger?.LogInformation("Migration info - Applied: {Applied}, Pending: {Pending}, Can Connect: {CanConnect}",
                info.AppliedMigrations.Count, info.PendingMigrations.Count, info.CanConnect);

            return info;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to get migration information");
            return null;
        }
    }

    /// <summary>
    /// Seeds initial data into the database
    /// </summary>
    /// <param name="serviceProvider">Service provider for resolving dependencies</param>
    /// <param name="logger">Logger for diagnostics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    public static async Task<bool> SeedDataAsync(
        IServiceProvider serviceProvider,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LicensingDbContext>();

            logger?.LogInformation("Starting SQL Server database seeding...");

            // Check if data already exists
            var hasData = await context.ProductFeatures.AnyAsync(cancellationToken);
            if (hasData)
            {
                logger?.LogInformation("Database already contains data, skipping seeding");
                return true;
            }

            // TODO: Implement actual seeding logic based on business requirements
            // This is a placeholder for future implementation
            logger?.LogWarning("Database seeding logic not implemented yet");

            return true;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to seed SQL Server database");
            return false;
        }
    }

    /// <summary>
    /// Initializes the database with migrations and seeding
    /// </summary>
    /// <param name="serviceProvider">Service provider for resolving dependencies</param>
    /// <param name="applyMigrations">Whether to apply migrations</param>
    /// <param name="seedData">Whether to seed initial data</param>
    /// <param name="logger">Logger for diagnostics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    public static async Task<bool> InitializeDatabaseAsync(
        IServiceProvider serviceProvider,
        bool applyMigrations = true,
        bool seedData = false,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("Initializing SQL Server database...");

        try
        {
            // Ensure database exists
            if (!await EnsureDatabaseCreatedAsync(serviceProvider, logger, cancellationToken))
            {
                return false;
            }

            // Apply migrations if requested
            if (applyMigrations)
            {
                if (!await ApplyMigrationsAsync(serviceProvider, logger, cancellationToken))
                {
                    return false;
                }
            }

            // Seed data if requested
            if (seedData)
            {
                if (!await SeedDataAsync(serviceProvider, logger, cancellationToken))
                {
                    return false;
                }
            }

            logger?.LogInformation("SQL Server database initialization completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "SQL Server database initialization failed");
            return false;
        }
    }
}

/// <summary>
/// Information about database migrations
/// </summary>
public class MigrationInfo
{
    /// <summary>
    /// List of applied migrations
    /// </summary>
    public List<string> AppliedMigrations { get; set; } = new();

    /// <summary>
    /// List of pending migrations
    /// </summary>
    public List<string> PendingMigrations { get; set; } = new();

    /// <summary>
    /// Whether the database can be connected to
    /// </summary>
    public bool CanConnect { get; set; }

    /// <summary>
    /// Whether there are pending migrations
    /// </summary>
    public bool HasPendingMigrations => PendingMigrations.Any();

    /// <summary>
    /// Whether any migrations have been applied
    /// </summary>
    public bool HasAppliedMigrations => AppliedMigrations.Any();
}
