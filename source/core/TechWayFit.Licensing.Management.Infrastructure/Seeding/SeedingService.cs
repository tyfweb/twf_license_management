using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Seeding;

namespace TechWayFit.Licensing.Management.Infrastructure.Seeding;

/// <summary>
/// Service for coordinating database seeding operations
/// </summary>
public class SeedingService : ISeedingService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SeedingService> _logger;
    private readonly List<IDataSeeder> _seeders;

    public SeedingService(IServiceProvider serviceProvider, IEnumerable<IDataSeeder> seeders, ILogger<SeedingService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _seeders = seeders.OrderBy(s => s.Order).ToList();
    }

    public async Task<int> SeedAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting database seeding process with {SeederCount} seeders", _seeders.Count);

        var executedCount = 0;
        var startTime = DateTime.UtcNow;

        try
        {
            foreach (var seeder in _seeders)
            {
                await SeedData(seeder, cancellationToken);
                executedCount++;
            }

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("Completed database seeding process. Executed {ExecutedCount}/{TotalCount} seeders in {Duration}ms",
                executedCount, _seeders.Count, duration);

            return executedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error during database seeding process");
            throw;
        }
    }

    public async Task<bool> SeedData(IDataSeeder seeder,
                                    CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing seeder: {SeederName} (Order: {Order})", seeder.SeederName, seeder.Order);

        try
        {
            if (await seeder.IsSeededAsync(cancellationToken))
            {
                _logger.LogInformation("Seeder {SeederName} has already been executed, skipping", seeder.SeederName);
                return true;
            }

            var success = await seeder.SeedAsync(cancellationToken);
            if (success)
            {
                _logger.LogInformation("Successfully executed seeder: {SeederName}", seeder.SeederName);
            }
            else
            {
                _logger.LogWarning("Seeder {SeederName} reported failure", seeder.SeederName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing seeder {SeederName}", seeder.SeederName);
            // Continue with next seeder even if one fails
        }
        return true;
    }

    public async Task<bool> SeedAsync(string seederName, CancellationToken cancellationToken = default)
    {
        var seeder = _seeders.FirstOrDefault(s => s.SeederName.Equals(seederName, StringComparison.OrdinalIgnoreCase));
        if (seeder == null)
        {
            _logger.LogWarning("Seeder {SeederName} not found", seederName);
            return false;
        }

        _logger.LogInformation("Executing specific seeder: {SeederName}", seederName);

        try
        {
            return await seeder.SeedAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing seeder {SeederName}", seederName);
            return false;
        }
    }

    public async Task<Dictionary<string, bool>> GetSeedingStatusAsync(CancellationToken cancellationToken = default)
    {
        var status = new Dictionary<string, bool>();

        foreach (var seeder in _seeders)
        {
            try
            {
                status[seeder.SeederName] = await seeder.IsSeededAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking seeding status for {SeederName}", seeder.SeederName);
                status[seeder.SeederName] = false;
            }
        }

        return status;
    }

    public async Task ClearSeedingHistoryAsync(string? seederName = null, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        try
        {
            _logger.LogInformation("Clearing seeding history for {SeederName}", seederName ?? "all seeders");

            await unitOfWork.SeedingHistory.ClearHistoryAsync(seederName, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully cleared seeding history for {SeederName}", seederName ?? "all seeders");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing seeding history for {SeederName}", seederName ?? "all seeders");
            throw;
        }
    }
}
