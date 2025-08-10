namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Seeding;

/// <summary>
/// Service for coordinating database seeding operations
/// </summary>
public interface ISeedingService
{
    /// <summary>
    /// Runs all registered seeders in order
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of seeders that were executed</returns>
    Task<int> SeedAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs a specific seeder by name
    /// </summary>
    /// <param name="seederName">Name of the seeder to run</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the seeder was found and executed</returns>
    Task<bool> SeedAsync(string seederName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the status of all registered seeders
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of seeder names and their seeded status</returns>
    Task<Dictionary<string, bool>> GetSeedingStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Forces re-seeding by clearing seeding history
    /// </summary>
    /// <param name="seederName">Optional seeder name, if null clears all</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ClearSeedingHistoryAsync(string? seederName = null, CancellationToken cancellationToken = default);
}
