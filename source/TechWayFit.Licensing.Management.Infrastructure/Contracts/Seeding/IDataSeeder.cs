namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Seeding;

/// <summary>
/// Interface for data seeding operations
/// </summary>
public interface IDataSeeder
{
    /// <summary>
    /// Gets the name of the seeder
    /// </summary>
    string SeederName { get; }

    /// <summary>
    /// Gets the order in which this seeder should run (lower numbers run first)
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Checks if the seeder has already been executed
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if seeding has already been performed</returns>
    Task<bool> IsSeededAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs the seeding operation
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if seeding was successful</returns>
    Task<bool> SeedAsync(CancellationToken cancellationToken = default);
}
