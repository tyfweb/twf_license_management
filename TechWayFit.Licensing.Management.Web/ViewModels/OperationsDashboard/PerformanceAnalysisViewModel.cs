using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.OperationsDashboard;
using TechWayFit.Licensing.Management.Infrastructure.Models.OperationsDashboard;

namespace TechWayFit.Licensing.Management.Web.ViewModels.OperationsDashboard;

/// <summary>
/// View model for performance analysis
/// </summary>
public class PerformanceAnalysisViewModel
{
    /// <summary>
    /// Time range in hours
    /// </summary>
    public int TimeRange { get; set; }

    /// <summary>
    /// Performance metrics for the time range
    /// </summary>
    public IEnumerable<PagePerformanceMetricEntity> PerformanceMetrics { get; set; } = new List<PagePerformanceMetricEntity>();

    /// <summary>
    /// Slowest endpoints
    /// </summary>
    public IEnumerable<PagePerformanceMetricEntity> SlowestEndpoints { get; set; } = new List<PagePerformanceMetricEntity>();

    /// <summary>
    /// Slowest queries
    /// </summary>
    public IEnumerable<QueryPerformanceMetricEntity> SlowestQueries { get; set; } = new List<QueryPerformanceMetricEntity>();

    /// <summary>
    /// Performance trends data
    /// </summary>
    public IEnumerable<PerformanceTrend> PerformanceTrends { get; set; } = new List<PerformanceTrend>();

    /// <summary>
    /// Dashboard configuration
    /// </summary>
    public DashboardConfiguration Configuration { get; set; } = new DashboardConfiguration();
}
