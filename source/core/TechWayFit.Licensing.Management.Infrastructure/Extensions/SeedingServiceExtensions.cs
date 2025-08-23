using Microsoft.Extensions.DependencyInjection;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Seeding;
using TechWayFit.Licensing.Management.Infrastructure.Seeding;
using TechWayFit.Licensing.Management.Infrastructure.Seeding.Seeders;

namespace TechWayFit.Licensing.Management.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering seeding services
/// </summary>
public static class SeedingServiceExtensions
{
    /// <summary>
    /// Registers seeding services and all available seeders
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddSeedingServices(this IServiceCollection services)
    {
        // Register the main seeding service
        services.AddScoped<ISeedingService, SeedingService>();

        // Register all available seeders
        services.AddScoped<IDataSeeder, TenantSeeder>();
        services.AddScoped<IDataSeeder, UserProfileSeeder>();
        services.AddScoped<IDataSeeder, UserRoleSeeder>();
        services.AddScoped<IDataSeeder, SettingsSeeder>();
        services.AddScoped<IDataSeeder, ConsumerAccountSeeder>();
        services.AddScoped<IDataSeeder, EnterpriseProductSeeder>();

        return services;
    }

    /// <summary>
    /// Executes all registered seeders
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of seeders executed</returns>
    public static async Task<int> SeedDatabaseAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var seedingService = scope.ServiceProvider.GetRequiredService<ISeedingService>();
        return await seedingService.SeedAllAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the seeding status for all registered seeders
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of seeder names and their seeded status</returns>
    public static async Task<Dictionary<string, bool>> GetSeedingStatusAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var seedingService = scope.ServiceProvider.GetRequiredService<ISeedingService>();
        return await seedingService.GetSeedingStatusAsync(cancellationToken);
    }

    /// <summary>
    /// Clears seeding history for all or specific seeders
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    /// <param name="seederName">Optional seeder name, if null clears all</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static async Task ClearSeedingHistoryAsync(this IServiceProvider serviceProvider, string? seederName = null, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var seedingService = scope.ServiceProvider.GetRequiredService<ISeedingService>();
        await seedingService.ClearSeedingHistoryAsync(seederName, cancellationToken);
    }
}
