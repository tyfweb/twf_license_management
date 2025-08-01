using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.OperationsDashboard;

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.OperationsDashboard;

/// <summary>
/// Repository interface for managing query performance metrics
/// </summary>
public interface IQueryPerformanceMetricRepository : IOperationsDashboardBaseRepository<QueryPerformanceMetricEntity>
{
    // Specific query operations for QueryPerformanceMetric
    Task<QueryPerformanceMetricEntity?> GetByQueryHashAsync(string queryHash);
    Task<IEnumerable<QueryPerformanceMetricEntity>> GetByQueryTypeAsync(string queryType, DateTime startTime, DateTime endTime);
    Task<IEnumerable<QueryPerformanceMetricEntity>> GetByTableNamesAsync(string tableNames, DateTime startTime, DateTime endTime);
    Task<IEnumerable<QueryPerformanceMetricEntity>> GetSlowestQueriesAsync(int topCount, DateTime startTime, DateTime endTime);
    Task<IEnumerable<QueryPerformanceMetricEntity>> GetMostFrequentQueriesAsync(int topCount, DateTime startTime, DateTime endTime);
    Task<IEnumerable<QueryPerformanceMetricEntity>> GetHighestErrorRateQueriesAsync(int topCount, DateTime startTime, DateTime endTime);
    Task<IEnumerable<QueryPerformanceMetricEntity>> GetByExecutionTimeThresholdAsync(int thresholdMs, DateTime startTime, DateTime endTime);

    // Analytics operations specific to QueryPerformanceMetric
    Task<decimal> GetAverageExecutionTimeAsync(DateTime startTime, DateTime endTime);
    Task<int> GetTotalExecutionCountAsync(DateTime startTime, DateTime endTime);
    Task<int> GetTotalErrorCountAsync(DateTime startTime, DateTime endTime);
    Task<decimal> GetAverageErrorRateAsync(DateTime startTime, DateTime endTime);
    Task<long> GetTotalRowsAffectedAsync(DateTime startTime, DateTime endTime);
}
