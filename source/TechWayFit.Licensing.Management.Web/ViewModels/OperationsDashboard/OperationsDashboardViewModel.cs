using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.OperationsDashboard;
using TechWayFit.Licensing.Management.Infrastructure.Models.OperationsDashboard;

namespace TechWayFit.Licensing.Management.Web.ViewModels.OperationsDashboard;

/// <summary>
/// View model for the main operations dashboard
/// </summary>
public class OperationsDashboardViewModel
{
    /// <summary>
    /// Current system health snapshot
    /// </summary>
    public SystemHealthSnapshotEntity? CurrentHealth { get; set; }

    /// <summary>
    /// System metrics overview
    /// </summary>
    public IEnumerable<SystemMetricEntity> SystemMetricsOverview { get; set; } = new List<SystemMetricEntity>();

    /// <summary>
    /// Error summary
    /// </summary>
    public IEnumerable<ErrorLogSummaryEntity> ErrorSummary { get; set; } = new List<ErrorLogSummaryEntity>();

    /// <summary>
    /// Performance metrics
    /// </summary>
    public IEnumerable<PagePerformanceMetricEntity> PerformanceMetrics { get; set; } = new List<PagePerformanceMetricEntity>();

    /// <summary>
    /// Slowest endpoints (top 5)
    /// </summary>
    public IEnumerable<PagePerformanceMetricEntity> SlowestEndpoints { get; set; } = new List<PagePerformanceMetricEntity>();

    /// <summary>
    /// Top errors (top 5)
    /// </summary>
    public IEnumerable<ErrorLogSummaryEntity> TopErrors { get; set; } = new List<ErrorLogSummaryEntity>();

    /// <summary>
    /// Dashboard configuration
    /// </summary>
    public DashboardConfiguration Configuration { get; set; } = new DashboardConfiguration();
}
