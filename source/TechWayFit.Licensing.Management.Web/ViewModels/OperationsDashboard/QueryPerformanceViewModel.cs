using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.OperationsDashboard;
using TechWayFit.Licensing.Management.Infrastructure.Models.OperationsDashboard;

namespace TechWayFit.Licensing.Management.Web.ViewModels.OperationsDashboard;

/// <summary>
/// View model for query performance analysis
/// </summary>
public class QueryPerformanceViewModel
{
    /// <summary>
    /// Start time for analysis
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// End time for analysis
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Time range in hours
    /// </summary>
    public int Hours { get; set; }

    /// <summary>
    /// All query performance metrics for the time range
    /// </summary>
    public IEnumerable<QueryPerformanceMetricEntity> QueryMetrics { get; set; } = new List<QueryPerformanceMetricEntity>();

    /// <summary>
    /// Slowest queries identified
    /// </summary>
    public IEnumerable<QueryPerformanceMetricEntity> SlowestQueries { get; set; } = new List<QueryPerformanceMetricEntity>();

    /// <summary>
    /// Dashboard configuration
    /// </summary>
    public DashboardConfiguration Configuration { get; set; } = new DashboardConfiguration();

    /// <summary>
    /// Total number of queries executed
    /// </summary>
    public int TotalQueries => QueryMetrics?.Sum(q => q.ExecutionCount) ?? 0;

    /// <summary>
    /// Total slow queries
    /// </summary>
    public int TotalSlowQueries => QueryMetrics?.Sum(q => q.SlowQueryCount) ?? 0;

    /// <summary>
    /// Average execution time across all queries
    /// </summary>
    public double AverageExecutionTime => QueryMetrics?.Any() == true 
        ? QueryMetrics.Average(q => q.AvgExecutionTimeMs) : 0;

    /// <summary>
    /// Queries that need optimization
    /// </summary>
    public IEnumerable<QueryPerformanceMetricEntity> QueriesNeedingOptimization => 
        QueryMetrics?.Where(q => q.NeedsOptimization) ?? new List<QueryPerformanceMetricEntity>();
}
