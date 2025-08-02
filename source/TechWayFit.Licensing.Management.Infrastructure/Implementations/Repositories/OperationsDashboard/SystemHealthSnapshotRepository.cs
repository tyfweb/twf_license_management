using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.OperationsDashboard;
using TechWayFit.Licensing.Management.Infrastructure.Data;
using TechWayFit.Licensing.Management.Infrastructure.Data.Context;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.OperationsDashboard;

namespace TechWayFit.Licensing.Management.Infrastructure.Implementations.Repositories.OperationsDashboard;

/// <summary>
/// Repository implementation for system health snapshot operations
/// </summary>
public class SystemHealthSnapshotRepository : OperationsBaseRepository<SystemHealthSnapshotEntity>, ISystemHealthSnapshotRepository
{
    public SystemHealthSnapshotRepository(LicensingDbContext context) : base(context)
    {
    }

    protected override string GetIdPropertyName()
    {
        return nameof(SystemHealthSnapshotEntity.SnapshotId);
    }

    public async Task<SystemHealthSnapshotEntity?> GetLatestAsync()
    {
        return await _dbSet
            .Where(e => e.IsActive)
            .OrderByDescending(s => s.SnapshotTimestamp)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<SystemHealthSnapshotEntity>> GetByHealthStatusAsync(string healthStatus, DateTime startTime, DateTime endTime)
    {
        if (string.IsNullOrWhiteSpace(healthStatus))
            throw new ArgumentException("Health status cannot be null or empty", nameof(healthStatus));

        return await _dbSet
            .Where(e => e.IsActive)
            .Where(s => s.OverallHealthStatus == healthStatus)
            .Where(s => s.SnapshotTimestamp >= startTime && s.SnapshotTimestamp <= endTime)
            .OrderByDescending(s => s.SnapshotTimestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<SystemHealthSnapshotEntity>> GetByCpuUsageThresholdAsync(decimal thresholdPercentage, DateTime startTime, DateTime endTime)
    {
        if (thresholdPercentage < 0 || thresholdPercentage > 100)
            throw new ArgumentException("Threshold percentage must be between 0 and 100", nameof(thresholdPercentage));

        return await _dbSet
            .Where(e => e.IsActive)
            .Where(s => s.CpuUsagePercent.HasValue && s.CpuUsagePercent >= thresholdPercentage)
            .Where(s => s.SnapshotTimestamp >= startTime && s.SnapshotTimestamp <= endTime)
            .OrderByDescending(s => s.CpuUsagePercent)
            .ToListAsync();
    }

    public async Task<IEnumerable<SystemHealthSnapshotEntity>> GetByMemoryUsageThresholdAsync(decimal thresholdPercentage, DateTime startTime, DateTime endTime)
    {
        if (thresholdPercentage < 0)
            throw new ArgumentException("Threshold percentage must be greater than or equal to 0", nameof(thresholdPercentage));

        return await _dbSet
            .Where(e => e.IsActive)
            .Where(s => s.MemoryUsageMb.HasValue && s.MemoryUsageMb >= thresholdPercentage)
            .Where(s => s.SnapshotTimestamp >= startTime && s.SnapshotTimestamp <= endTime)
            .OrderByDescending(s => s.MemoryUsageMb)
            .ToListAsync();
    }

    public async Task<IEnumerable<SystemHealthSnapshotEntity>> GetByDiskUsageThresholdAsync(decimal thresholdPercentage, DateTime startTime, DateTime endTime)
    {
        if (thresholdPercentage < 0 || thresholdPercentage > 100)
            throw new ArgumentException("Threshold percentage must be between 0 and 100", nameof(thresholdPercentage));

        return await _dbSet
            .Where(e => e.IsActive)
            .Where(s => s.DiskUsagePercent.HasValue && s.DiskUsagePercent >= thresholdPercentage)
            .Where(s => s.SnapshotTimestamp >= startTime && s.SnapshotTimestamp <= endTime)
            .OrderByDescending(s => s.DiskUsagePercent)
            .ToListAsync();
    }

    public async Task<IEnumerable<SystemHealthSnapshotEntity>> GetByActiveConnectionsThresholdAsync(int thresholdCount, DateTime startTime, DateTime endTime)
    {
        if (thresholdCount < 0)
            throw new ArgumentException("Threshold count must be greater than or equal to 0", nameof(thresholdCount));

        return await _dbSet
            .Where(e => e.IsActive)
            .Where(s => s.ActiveDbConnections.HasValue && s.ActiveDbConnections >= thresholdCount)
            .Where(s => s.SnapshotTimestamp >= startTime && s.SnapshotTimestamp <= endTime)
            .OrderByDescending(s => s.ActiveDbConnections)
            .ToListAsync();
    }

    public async Task<IEnumerable<SystemHealthSnapshotEntity>> GetUnhealthySnapshotsAsync(DateTime startTime, DateTime endTime)
    {
        return await _dbSet
            .Where(e => e.IsActive)
            .Where(s => s.OverallHealthStatus != "Healthy")
            .Where(s => s.SnapshotTimestamp >= startTime && s.SnapshotTimestamp <= endTime)
            .OrderByDescending(s => s.SnapshotTimestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<SystemHealthSnapshotEntity>> GetRecentSnapshotsAsync(int count)
    {
        if (count <= 0)
            throw new ArgumentException("Count must be greater than zero", nameof(count));

        return await _dbSet
            .Where(e => e.IsActive)
            .OrderByDescending(s => s.SnapshotTimestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<decimal> GetAverageCpuUsageAsync(DateTime startTime, DateTime endTime)
    {
        var snapshots = await _dbSet
            .Where(e => e.IsActive)
            .Where(s => s.SnapshotTimestamp >= startTime && s.SnapshotTimestamp <= endTime)
            .Where(s => s.CpuUsagePercent.HasValue)
            .Select(s => s.CpuUsagePercent!.Value)
            .ToListAsync();

        return snapshots.Any() ? snapshots.Average() : 0;
    }

    public async Task<decimal> GetAverageMemoryUsageAsync(DateTime startTime, DateTime endTime)
    {
        var snapshots = await _dbSet
            .Where(e => e.IsActive)
            .Where(s => s.SnapshotTimestamp >= startTime && s.SnapshotTimestamp <= endTime)
            .Where(s => s.MemoryUsageMb.HasValue)
            .Select(s => s.MemoryUsageMb!.Value)
            .ToListAsync();

        return snapshots.Any() ? snapshots.Average() : 0;
    }

    public async Task<decimal> GetAverageDiskUsageAsync(DateTime startTime, DateTime endTime)
    {
        var snapshots = await _dbSet
            .Where(e => e.IsActive)
            .Where(s => s.SnapshotTimestamp >= startTime && s.SnapshotTimestamp <= endTime)
            .Where(s => s.DiskUsagePercent.HasValue)
            .Select(s => s.DiskUsagePercent!.Value)
            .ToListAsync();

        return snapshots.Any() ? snapshots.Average() : 0;
    }

    public async Task<decimal> GetAverageActiveConnectionsAsync(DateTime startTime, DateTime endTime)
    {
        var snapshots = await _dbSet
            .Where(e => e.IsActive)
            .Where(s => s.SnapshotTimestamp >= startTime && s.SnapshotTimestamp <= endTime)
            .Where(s => s.ActiveDbConnections.HasValue)
            .Select(s => (decimal)s.ActiveDbConnections!.Value)
            .ToListAsync();

        return snapshots.Any() ? snapshots.Average() : 0;
    }

    public async Task<decimal> GetMaxCpuUsageAsync(DateTime startTime, DateTime endTime)
    {
        var maxValue = await _dbSet
            .Where(e => e.IsActive)
            .Where(s => s.SnapshotTimestamp >= startTime && s.SnapshotTimestamp <= endTime)
            .Where(s => s.CpuUsagePercent.HasValue)
            .MaxAsync(s => (decimal?)s.CpuUsagePercent);

        return maxValue ?? 0;
    }

    public async Task<decimal> GetMaxMemoryUsageAsync(DateTime startTime, DateTime endTime)
    {
        var maxValue = await _dbSet
            .Where(e => e.IsActive)
            .Where(s => s.SnapshotTimestamp >= startTime && s.SnapshotTimestamp <= endTime)
            .Where(s => s.MemoryUsageMb.HasValue)
            .MaxAsync(s => (decimal?)s.MemoryUsageMb);

        return maxValue ?? 0;
    }

    public async Task<decimal> GetMaxDiskUsageAsync(DateTime startTime, DateTime endTime)
    {
        var maxValue = await _dbSet
            .Where(e => e.IsActive)
            .Where(s => s.SnapshotTimestamp >= startTime && s.SnapshotTimestamp <= endTime)
            .Where(s => s.DiskUsagePercent.HasValue)
            .MaxAsync(s => (decimal?)s.DiskUsagePercent);

        return maxValue ?? 0;
    }

    public async Task<int> GetMaxActiveConnectionsAsync(DateTime startTime, DateTime endTime)
    {
        var maxValue = await _dbSet
            .Where(e => e.IsActive)
            .Where(s => s.SnapshotTimestamp >= startTime && s.SnapshotTimestamp <= endTime)
            .Where(s => s.ActiveDbConnections.HasValue)
            .MaxAsync(s => (int?)s.ActiveDbConnections);

        return maxValue ?? 0;
    }

    public async Task<int> GetHealthySnapshotCountAsync(DateTime startTime, DateTime endTime)
    {
        return await _dbSet
            .Where(e => e.IsActive)
            .Where(s => s.SnapshotTimestamp >= startTime && s.SnapshotTimestamp <= endTime)
            .Where(s => s.OverallHealthStatus == "Healthy")
            .CountAsync();
    }

    public async Task<int> GetUnhealthySnapshotCountAsync(DateTime startTime, DateTime endTime)
    {
        return await _dbSet
            .Where(e => e.IsActive)
            .Where(s => s.SnapshotTimestamp >= startTime && s.SnapshotTimestamp <= endTime)
            .Where(s => s.OverallHealthStatus != "Healthy")
            .CountAsync();
    }

    public async Task<decimal> GetHealthPercentageAsync(DateTime startTime, DateTime endTime)
    {
        var totalSnapshots = await _dbSet
            .Where(e => e.IsActive)
            .Where(s => s.SnapshotTimestamp >= startTime && s.SnapshotTimestamp <= endTime)
            .CountAsync();

        if (totalSnapshots == 0)
            return 100;

        var healthySnapshots = await GetHealthySnapshotCountAsync(startTime, endTime);

        return (decimal)healthySnapshots / totalSnapshots * 100;
    }
}
