using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.OperationsDashboard;

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.OperationsDashboard;

/// <summary>
/// Repository interface for managing system metrics
/// </summary>
public interface ISystemMetricRepository : IOperationsDashboardBaseRepository<SystemMetricEntity>
{
    // Specific query operations for SystemMetric
    Task<IEnumerable<SystemMetricEntity>> GetByControllerActionAsync(string controller, string action, DateTime startTime, DateTime endTime);
    Task<IEnumerable<SystemMetricEntity>> GetTopByResponseTimeAsync(int topCount, DateTime startTime, DateTime endTime);
    Task<IEnumerable<SystemMetricEntity>> GetTopByRequestCountAsync(int topCount, DateTime startTime, DateTime endTime);
    Task<IEnumerable<SystemMetricEntity>> GetByResponseTimeThresholdAsync(int thresholdMs, DateTime startTime, DateTime endTime);

    // Analytics operations specific to SystemMetric
    Task<decimal> GetAverageResponseTimeAsync(DateTime startTime, DateTime endTime);
    Task<int> GetTotalRequestCountAsync(DateTime startTime, DateTime endTime);
    Task<decimal> GetAverageErrorRateAsync(DateTime startTime, DateTime endTime);
    Task<int> GetTotalErrorCountAsync(DateTime startTime, DateTime endTime);
}
