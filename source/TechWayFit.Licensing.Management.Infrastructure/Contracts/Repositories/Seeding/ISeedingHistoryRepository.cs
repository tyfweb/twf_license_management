using TechWayFit.Licensing.Management.Core.Models.Seeding;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Seeding;

/// <summary>
/// Repository interface for seeding history tracking
/// </summary>
public interface ISeedingHistoryRepository : IDataRepository<SeedingHistory>
{
    /// <summary>
    /// Checks if a seeder has been executed successfully
    /// </summary>
    /// <param name="seederName">Name of the seeder</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the seeder has been executed successfully</returns>
    Task<bool> IsSeederExecutedAsync(string seederName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest execution record for a seeder
    /// </summary>
    /// <param name="seederName">Name of the seeder</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Latest seeding history record or null</returns>
    Task<SeedingHistory?> GetLatestExecutionAsync(string seederName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all execution records for a seeder
    /// </summary>
    /// <param name="seederName">Name of the seeder</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of seeding history records</returns>
    Task<IEnumerable<SeedingHistory>> GetExecutionHistoryAsync(string seederName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a seeding execution
    /// </summary>
    /// <param name="seederName">Name of the seeder</param>
    /// <param name="isSuccessful">Whether the seeding was successful</param>
    /// <param name="recordsCreated">Number of records created</param>
    /// <param name="durationMs">Duration in milliseconds</param>
    /// <param name="errorMessage">Error message if failed</param>
    /// <param name="metadata">Additional metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created seeding history record</returns>
    Task<SeedingHistory> RecordExecutionAsync(
        string seederName, 
        bool isSuccessful, 
        int recordsCreated = 0, 
        long durationMs = 0, 
        string? errorMessage = null, 
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears seeding history for a specific seeder or all seeders
    /// </summary>
    /// <param name="seederName">Name of the seeder, null for all</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ClearHistoryAsync(string? seederName = null, CancellationToken cancellationToken = default);
}
