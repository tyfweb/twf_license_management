using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.OperationsDashboard;

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.OperationsDashboard;

/// <summary>
/// Repository interface for managing page performance metrics
/// </summary>
public interface IPagePerformanceMetricRepository : IOperationsDashboardBaseRepository<PagePerformanceMetricEntity>
{
    // Specific query operations for PagePerformanceMetric
    Task<IEnumerable<PagePerformanceMetricEntity>> GetByControllerActionAsync(string controller, string action, DateTime startTime, DateTime endTime);
    Task<IEnumerable<PagePerformanceMetricEntity>> GetByRouteTemplateAsync(string routeTemplate, DateTime startTime, DateTime endTime);
    Task<IEnumerable<PagePerformanceMetricEntity>> GetByStatusCodeAsync(int statusCode, DateTime startTime, DateTime endTime);
    Task<IEnumerable<PagePerformanceMetricEntity>> GetSlowestEndpointsAsync(int topCount, DateTime startTime, DateTime endTime);
    Task<IEnumerable<PagePerformanceMetricEntity>> GetHighestTrafficEndpointsAsync(int topCount, DateTime startTime, DateTime endTime);
    Task<IEnumerable<PagePerformanceMetricEntity>> GetHighestErrorRateEndpointsAsync(int topCount, DateTime startTime, DateTime endTime);
    Task<IEnumerable<PagePerformanceMetricEntity>> GetByResponseTimeThresholdAsync(int thresholdMs, DateTime startTime, DateTime endTime);

    // Analytics operations specific to PagePerformanceMetric
    Task<decimal> GetAverageResponseTimeAsync(DateTime startTime, DateTime endTime);
    Task<int> GetTotalRequestCountAsync(DateTime startTime, DateTime endTime);
    Task<int> GetTotalErrorCountAsync(DateTime startTime, DateTime endTime);
    Task<decimal> GetAverageErrorRateAsync(DateTime startTime, DateTime endTime);
}
