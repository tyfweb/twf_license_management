using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Seeding;

namespace TechWayFit.Licensing.Management.Infrastructure.Seeding;

/// <summary>
/// Base class for data seeders
/// </summary>
public abstract class BaseDataSeeder : IDataSeeder
{
    protected readonly IUnitOfWork _unitOfWork;
    protected readonly ILogger<BaseDataSeeder> _logger;

    protected BaseDataSeeder(IUnitOfWork unitOfWork, ILogger<BaseDataSeeder> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Name of the seeder
    /// </summary>
    public abstract string SeederName { get; }

    /// <summary>
    /// Order in which this seeder should run
    /// </summary>
    public abstract int Order { get; }

    /// <summary>
    /// Checks if the seeder has already been executed
    /// </summary>
    public virtual async Task<bool> IsSeededAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _unitOfWork.SeedingHistory.IsSeederExecutedAsync(SeederName, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not check seeding status for {SeederName}, assuming not seeded", SeederName);
            return false;
        }
    }

    /// <summary>
    /// Performs the seeding operation
    /// </summary>
    public async Task<bool> SeedAsync(CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var recordsCreated = 0;
        string? errorMessage = null;

        try
        {
            _logger.LogInformation("Starting seeding for {SeederName}", SeederName);

            // Check if already seeded
            if (await IsSeededAsync(cancellationToken))
            {
                _logger.LogInformation("Seeder {SeederName} has already been executed, skipping", SeederName);
                return true;
            }

            // Perform the actual seeding
            recordsCreated = await ExecuteSeedingAsync(cancellationToken);

            // Record successful execution
            var duration = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
            await _unitOfWork.SeedingHistory.RecordExecutionAsync(
                SeederName, 
                true, 
                recordsCreated, 
                duration, 
                null, 
                GetMetadata(),
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully completed seeding for {SeederName}, created {RecordsCreated} records in {Duration}ms", 
                SeederName, recordsCreated, duration);

            return true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            _logger.LogError(ex, "Failed to seed data for {SeederName}", SeederName);

            try
            {
                // Record failed execution
                var duration = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
                await _unitOfWork.SeedingHistory.RecordExecutionAsync(
                    SeederName, 
                    false, 
                    recordsCreated, 
                    duration, 
                    errorMessage, 
                    GetMetadata(),
                    cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception recordEx)
            {
                _logger.LogError(recordEx, "Failed to record seeding failure for {SeederName}", SeederName);
            }

            return false;
        }
    }

    /// <summary>
    /// Executes the actual seeding logic
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of records created</returns>
    protected abstract Task<int> ExecuteSeedingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets metadata about this seeding operation
    /// </summary>
    /// <returns>Metadata dictionary</returns>
    protected virtual Dictionary<string, object> GetMetadata()
    {
        return new Dictionary<string, object>
        {
            ["SeederType"] = GetType().Name,
            ["Order"] = Order,
            ["Timestamp"] = DateTime.UtcNow
        };
    }
}
