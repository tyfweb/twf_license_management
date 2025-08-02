using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.OperationsDashboard;
using TechWayFit.Licensing.Management.Infrastructure.Data;
using TechWayFit.Licensing.Management.Infrastructure.Data.Context;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.OperationsDashboard;

namespace TechWayFit.Licensing.Management.Infrastructure.Implementations.Repositories.OperationsDashboard;

/// <summary>
/// Repository implementation for page performance metrics operations
/// </summary>
public class PagePerformanceMetricRepository : OperationsBaseRepository<PagePerformanceMetricEntity>, IPagePerformanceMetricRepository
{
    public PagePerformanceMetricRepository(LicensingDbContext context) : base(context)
    {
    }

    protected override string GetIdPropertyName()
    {
        return nameof(PagePerformanceMetricEntity.PerformanceId);
    }

    public async Task<decimal> GetAverageErrorRateAsync(DateTime startTime, DateTime endTime)
    {
        var totalRequests = await GetTotalRequestCountAsync(startTime, endTime);
        var totalErrors = await GetTotalErrorCountAsync(startTime, endTime);

        return totalRequests > 0 ? (decimal)totalErrors / totalRequests * 100 : 0;
    }

    public async Task<decimal> GetAverageResponseTimeAsync(DateTime startTime, DateTime endTime)
    {
        var metrics = await _dbSet
            .Where(e => e.IsActive)
            .Where(p => p.TimestampHour >= startTime && p.TimestampHour <= endTime)
            .Where(p => p.RequestCount > 0)
            .ToListAsync();

        if (!metrics.Any())
            return 0;

        // Calculate weighted average based on request counts
        var totalWeightedTime = metrics.Sum(p => (long)p.AvgResponseTimeMs * p.RequestCount);
        var totalRequests = metrics.Sum(p => p.RequestCount);

        return totalRequests > 0 ? (decimal)totalWeightedTime / totalRequests : 0;
    }

    public async Task<IEnumerable<PagePerformanceMetricEntity>> GetByControllerActionAsync(string controller, string action, DateTime startTime, DateTime endTime)
    {
        if (string.IsNullOrWhiteSpace(controller))
            throw new ArgumentException("Controller cannot be null or empty", nameof(controller));
        
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be null or empty", nameof(action));

        return await _dbSet
            .Where(e => e.IsActive)
            .Where(p => p.Controller == controller && p.Action == action)
            .Where(p => p.TimestampHour >= startTime && p.TimestampHour <= endTime)
            .OrderByDescending(p => p.TimestampHour)
            .ToListAsync();
    }

    public async Task<IEnumerable<PagePerformanceMetricEntity>> GetByResponseTimeThresholdAsync(int thresholdMs, DateTime startTime, DateTime endTime)
    {
        if (thresholdMs <= 0)
            throw new ArgumentException("Threshold must be greater than zero", nameof(thresholdMs));

        return await _dbSet
            .Where(e => e.IsActive)
            .Where(p => p.TimestampHour >= startTime && p.TimestampHour <= endTime)
            .Where(p => p.AvgResponseTimeMs >= thresholdMs)
            .OrderByDescending(p => p.AvgResponseTimeMs)
            .ToListAsync();
    }

    public async Task<IEnumerable<PagePerformanceMetricEntity>> GetByRouteTemplateAsync(string routeTemplate, DateTime startTime, DateTime endTime)
    {
        if (string.IsNullOrWhiteSpace(routeTemplate))
            throw new ArgumentException("Route template cannot be null or empty", nameof(routeTemplate));

        return await _dbSet
            .Where(e => e.IsActive)
            .Where(p => p.RouteTemplate == routeTemplate)
            .Where(p => p.TimestampHour >= startTime && p.TimestampHour <= endTime)
            .OrderByDescending(p => p.TimestampHour)
            .ToListAsync();
    }

    public async Task<IEnumerable<PagePerformanceMetricEntity>> GetByStatusCodeAsync(int statusCode, DateTime startTime, DateTime endTime)
    {
        return await _dbSet
            .Where(e => e.IsActive)
            .Where(p => p.TimestampHour >= startTime && p.TimestampHour <= endTime)
            .Where(p => (statusCode >= 200 && statusCode < 300 && p.SuccessCount > 0) ||
                       (statusCode >= 400 && statusCode < 500 && p.ClientErrorCount > 0) ||
                       (statusCode >= 500 && p.ServerErrorCount > 0))
            .OrderByDescending(p => p.TimestampHour)
            .ToListAsync();
    }

    public async Task<IEnumerable<PagePerformanceMetricEntity>> GetHighestErrorRateEndpointsAsync(int topCount, DateTime startTime, DateTime endTime)
    {
        if (topCount <= 0)
            throw new ArgumentException("Top count must be greater than zero", nameof(topCount));

        return await _dbSet
            .Where(e => e.IsActive)
            .Where(p => p.TimestampHour >= startTime && p.TimestampHour <= endTime)
            .Where(p => p.RequestCount > 0)
            .OrderByDescending(p => (p.ClientErrorCount + p.ServerErrorCount) / (double)p.RequestCount)
            .Take(topCount)
            .ToListAsync();
    }

    public async Task<IEnumerable<PagePerformanceMetricEntity>> GetHighestTrafficEndpointsAsync(int topCount, DateTime startTime, DateTime endTime)
    {
        if (topCount <= 0)
            throw new ArgumentException("Top count must be greater than zero", nameof(topCount));

        return await _dbSet
            .Where(e => e.IsActive)
            .Where(p => p.TimestampHour >= startTime && p.TimestampHour <= endTime)
            .OrderByDescending(p => p.RequestCount)
            .Take(topCount)
            .ToListAsync();
    }

    public async Task<IEnumerable<PagePerformanceMetricEntity>> GetSlowestEndpointsAsync(int topCount, DateTime startTime, DateTime endTime)
    {
        if (topCount <= 0)
            throw new ArgumentException("Top count must be greater than zero", nameof(topCount));

        return await _dbSet
            .Where(e => e.IsActive)
            .Where(p => p.TimestampHour >= startTime && p.TimestampHour <= endTime)
            .OrderByDescending(p => p.AvgResponseTimeMs)
            .Take(topCount)
            .ToListAsync();
    }

    public async Task<int> GetTotalErrorCountAsync(DateTime startTime, DateTime endTime)
    {
        return await _dbSet
            .Where(e => e.IsActive)
            .Where(p => p.TimestampHour >= startTime && p.TimestampHour <= endTime)
            .SumAsync(p => p.ClientErrorCount + p.ServerErrorCount);
    }

    public async Task<int> GetTotalRequestCountAsync(DateTime startTime, DateTime endTime)
    {
        return await _dbSet
            .Where(e => e.IsActive)
            .Where(p => p.TimestampHour >= startTime && p.TimestampHour <= endTime)
            .SumAsync(p => p.RequestCount);
    }
}
