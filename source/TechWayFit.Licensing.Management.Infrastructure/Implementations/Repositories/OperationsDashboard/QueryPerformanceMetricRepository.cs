using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.OperationsDashboard;
using TechWayFit.Licensing.Management.Infrastructure.Data;
using TechWayFit.Licensing.Management.Infrastructure.Data.Context;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.OperationsDashboard;

namespace TechWayFit.Licensing.Management.Infrastructure.Implementations.Repositories.OperationsDashboard;

/// <summary>
/// Repository implementation for query performance metrics operations
/// </summary>
public class QueryPerformanceMetricRepository : OperationsBaseRepository<QueryPerformanceMetricEntity>, IQueryPerformanceMetricRepository
{
    public QueryPerformanceMetricRepository(LicensingDbContext context) : base(context)
    {
    }

    protected override string GetIdPropertyName()
    {
        return nameof(QueryPerformanceMetricEntity.QueryMetricId);
    }

    public async Task<QueryPerformanceMetricEntity?> GetByQueryHashAsync(string queryHash)
    {
        if (string.IsNullOrWhiteSpace(queryHash))
            throw new ArgumentException("Query hash cannot be null or empty", nameof(queryHash));

        return await _dbSet
            .Where(e => e.IsActive)
            .FirstOrDefaultAsync(q => q.QueryHash == queryHash);
    }

    public async Task<IEnumerable<QueryPerformanceMetricEntity>> GetByQueryTypeAsync(string queryType, DateTime startTime, DateTime endTime)
    {
        if (string.IsNullOrWhiteSpace(queryType))
            throw new ArgumentException("Query type cannot be null or empty", nameof(queryType));

        return await _dbSet
            .Where(e => e.IsActive)
            .Where(q => q.QueryType == queryType)
            .Where(q => q.TimestampHour >= startTime && q.TimestampHour <= endTime)
            .OrderByDescending(q => q.TimestampHour)
            .ToListAsync();
    }

    public async Task<IEnumerable<QueryPerformanceMetricEntity>> GetByTableNamesAsync(string tableNames, DateTime startTime, DateTime endTime)
    {
        if (string.IsNullOrWhiteSpace(tableNames))
            throw new ArgumentException("Table names cannot be null or empty", nameof(tableNames));

        return await _dbSet
            .Where(e => e.IsActive)
            .Where(q => q.TableNames != null && q.TableNames.Contains(tableNames))
            .Where(q => q.TimestampHour >= startTime && q.TimestampHour <= endTime)
            .OrderByDescending(q => q.TimestampHour)
            .ToListAsync();
    }

    public async Task<IEnumerable<QueryPerformanceMetricEntity>> GetSlowestQueriesAsync(int topCount, DateTime startTime, DateTime endTime)
    {
        if (topCount <= 0)
            throw new ArgumentException("Top count must be greater than zero", nameof(topCount));

        return await _dbSet
            .Where(e => e.IsActive)
            .Where(q => q.TimestampHour >= startTime && q.TimestampHour <= endTime)
            .OrderByDescending(q => q.AvgExecutionTimeMs)
            .Take(topCount)
            .ToListAsync();
    }

    public async Task<IEnumerable<QueryPerformanceMetricEntity>> GetMostFrequentQueriesAsync(int topCount, DateTime startTime, DateTime endTime)
    {
        if (topCount <= 0)
            throw new ArgumentException("Top count must be greater than zero", nameof(topCount));

        return await _dbSet
            .Where(e => e.IsActive)
            .Where(q => q.TimestampHour >= startTime && q.TimestampHour <= endTime)
            .OrderByDescending(q => q.ExecutionCount)
            .Take(topCount)
            .ToListAsync();
    }

    public async Task<IEnumerable<QueryPerformanceMetricEntity>> GetHighestErrorRateQueriesAsync(int topCount, DateTime startTime, DateTime endTime)
    {
        if (topCount <= 0)
            throw new ArgumentException("Top count must be greater than zero", nameof(topCount));

        return await _dbSet
            .Where(e => e.IsActive)
            .Where(q => q.TimestampHour >= startTime && q.TimestampHour <= endTime)
            .Where(q => q.ExecutionCount > 0)
            .OrderByDescending(q => q.TimeoutCount / (double)q.ExecutionCount)
            .Take(topCount)
            .ToListAsync();
    }

    public async Task<IEnumerable<QueryPerformanceMetricEntity>> GetByExecutionTimeThresholdAsync(int thresholdMs, DateTime startTime, DateTime endTime)
    {
        if (thresholdMs <= 0)
            throw new ArgumentException("Threshold must be greater than zero", nameof(thresholdMs));

        return await _dbSet
            .Where(e => e.IsActive)
            .Where(q => q.TimestampHour >= startTime && q.TimestampHour <= endTime)
            .Where(q => q.AvgExecutionTimeMs >= thresholdMs)
            .OrderByDescending(q => q.AvgExecutionTimeMs)
            .ToListAsync();
    }

    public async Task<decimal> GetAverageExecutionTimeAsync(DateTime startTime, DateTime endTime)
    {
        var metrics = await _dbSet
            .Where(e => e.IsActive)
            .Where(q => q.TimestampHour >= startTime && q.TimestampHour <= endTime)
            .Where(q => q.ExecutionCount > 0)
            .ToListAsync();

        if (!metrics.Any())
            return 0;

        // Calculate weighted average based on execution counts
        var totalWeightedTime = metrics.Sum(q => (long)q.AvgExecutionTimeMs * q.ExecutionCount);
        var totalExecutions = metrics.Sum(q => q.ExecutionCount);

        return totalExecutions > 0 ? (decimal)totalWeightedTime / totalExecutions : 0;
    }

    public async Task<int> GetTotalExecutionCountAsync(DateTime startTime, DateTime endTime)
    {
        return await _dbSet
            .Where(e => e.IsActive)
            .Where(q => q.TimestampHour >= startTime && q.TimestampHour <= endTime)
            .SumAsync(q => q.ExecutionCount);
    }

    public async Task<int> GetTotalErrorCountAsync(DateTime startTime, DateTime endTime)
    {
        return await _dbSet
            .Where(e => e.IsActive)
            .Where(q => q.TimestampHour >= startTime && q.TimestampHour <= endTime)
            .SumAsync(q => q.TimeoutCount);
    }

    public async Task<decimal> GetAverageErrorRateAsync(DateTime startTime, DateTime endTime)
    {
        var totalExecutions = await GetTotalExecutionCountAsync(startTime, endTime);
        var totalErrors = await GetTotalErrorCountAsync(startTime, endTime);

        return totalExecutions > 0 ? (decimal)totalErrors / totalExecutions * 100 : 0;
    }

    public async Task<long> GetTotalRowsAffectedAsync(DateTime startTime, DateTime endTime)
    {
        var metrics = await _dbSet
            .Where(e => e.IsActive)
            .Where(q => q.TimestampHour >= startTime && q.TimestampHour <= endTime)
            .ToListAsync();

        return metrics.Sum(q => (long)q.RowsAffectedAvg * q.ExecutionCount);
    }
}
