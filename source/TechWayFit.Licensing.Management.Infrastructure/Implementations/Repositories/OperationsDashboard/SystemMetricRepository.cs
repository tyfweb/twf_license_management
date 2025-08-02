using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.OperationsDashboard;
using TechWayFit.Licensing.Management.Infrastructure.Data.Context;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.OperationsDashboard;

namespace TechWayFit.Licensing.Management.Infrastructure.Implementations.Repositories.OperationsDashboard;

/// <summary>
/// Repository implementation for system metrics operations
/// </summary>
public class SystemMetricRepository : OperationsBaseRepository<SystemMetricEntity>, ISystemMetricRepository
{
    public SystemMetricRepository(LicensingDbContext context) : base(context)
    {
    }

    protected override string GetIdPropertyName()
    {
        return nameof(SystemMetricEntity.MetricId);
    }

    public async Task<IEnumerable<SystemMetricEntity>> GetByControllerActionAsync(string controller, string action, DateTime startTime, DateTime endTime)
    {
        if (string.IsNullOrWhiteSpace(controller))
            throw new ArgumentException("Controller cannot be null or empty", nameof(controller));
        
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be null or empty", nameof(action));

        return await _dbSet
            .Where(e => e.IsActive)
            .Where(m => m.Controller == controller && m.Action == action)
            .Where(m => m.TimestampHour >= startTime && m.TimestampHour <= endTime)
            .OrderByDescending(m => m.TimestampHour)
            .ToListAsync();
    }

    public async Task<IEnumerable<SystemMetricEntity>> GetTopByResponseTimeAsync(int topCount, DateTime startTime, DateTime endTime)
    {
        if (topCount <= 0)
            throw new ArgumentException("Top count must be greater than zero", nameof(topCount));

        return await _dbSet
            .Where(e => e.IsActive)
            .Where(m => m.TimestampHour >= startTime && m.TimestampHour <= endTime)
            .OrderByDescending(m => m.AvgResponseTimeMs)
            .Take(topCount)
            .ToListAsync();
    }

    public async Task<decimal> GetAverageResponseTimeAsync(DateTime startTime, DateTime endTime)
    {
        var metrics = await _dbSet
            .Where(e => e.IsActive)
            .Where(m => m.TimestampHour >= startTime && m.TimestampHour <= endTime)
            .Where(m => m.RequestCount > 0)
            .ToListAsync();

        if (!metrics.Any())
            return 0;

        // Calculate weighted average based on request counts
        var totalWeightedTime = metrics.Sum(m => (long)m.AvgResponseTimeMs * m.RequestCount);
        var totalRequests = metrics.Sum(m => m.RequestCount);

        return totalRequests > 0 ? (decimal)totalWeightedTime / totalRequests : 0;
    }

    public async Task<IEnumerable<SystemMetricEntity>> GetTopByRequestCountAsync(int topCount, DateTime startTime, DateTime endTime)
    {
        if (topCount <= 0)
            throw new ArgumentException("Top count must be greater than zero", nameof(topCount));

        return await _dbSet
            .Where(e => e.IsActive)
            .Where(m => m.TimestampHour >= startTime && m.TimestampHour <= endTime)
            .OrderByDescending(m => m.RequestCount)
            .Take(topCount)
            .ToListAsync();
    }

    public async Task<IEnumerable<SystemMetricEntity>> GetByResponseTimeThresholdAsync(int thresholdMs, DateTime startTime, DateTime endTime)
    {
        if (thresholdMs <= 0)
            throw new ArgumentException("Threshold must be greater than zero", nameof(thresholdMs));

        return await _dbSet
            .Where(e => e.IsActive)
            .Where(m => m.TimestampHour >= startTime && m.TimestampHour <= endTime)
            .Where(m => m.AvgResponseTimeMs >= thresholdMs)
            .OrderByDescending(m => m.AvgResponseTimeMs)
            .ToListAsync();
    }

    public async Task<int> GetTotalRequestCountAsync(DateTime startTime, DateTime endTime)
    {
        return await _dbSet
            .Where(e => e.IsActive)
            .Where(m => m.TimestampHour >= startTime && m.TimestampHour <= endTime)
            .SumAsync(m => m.RequestCount);
    }

    public async Task<decimal> GetAverageErrorRateAsync(DateTime startTime, DateTime endTime)
    {
        var totalRequests = await GetTotalRequestCountAsync(startTime, endTime);
        var totalErrors = await GetTotalErrorCountAsync(startTime, endTime);

        return totalRequests > 0 ? (decimal)totalErrors / totalRequests * 100 : 0;
    }

    public async Task<int> GetTotalErrorCountAsync(DateTime startTime, DateTime endTime)
    {
        return await _dbSet
            .Where(e => e.IsActive)
            .Where(m => m.TimestampHour >= startTime && m.TimestampHour <= endTime)
            .SumAsync(m => m.ErrorCount);
    }
}
