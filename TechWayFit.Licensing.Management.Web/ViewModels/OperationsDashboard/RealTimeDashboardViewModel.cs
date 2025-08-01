using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.OperationsDashboard;
using TechWayFit.Licensing.Management.Infrastructure.Models.OperationsDashboard;

namespace TechWayFit.Licensing.Management.Web.ViewModels.OperationsDashboard;

/// <summary>
/// View model for real-time dashboard
/// </summary>
public class RealTimeDashboardViewModel
{
    /// <summary>
    /// Current system health snapshot
    /// </summary>
    public SystemHealthSnapshotEntity? CurrentHealth { get; set; }

    /// <summary>
    /// System metrics overview (recent data)
    /// </summary>
    public IEnumerable<SystemMetricEntity> SystemMetricsOverview { get; set; } = new List<SystemMetricEntity>();

    /// <summary>
    /// Recent errors
    /// </summary>
    public IEnumerable<ErrorLogSummaryEntity> RecentErrors { get; set; } = new List<ErrorLogSummaryEntity>();

    /// <summary>
    /// Dashboard configuration
    /// </summary>
    public DashboardConfiguration Configuration { get; set; } = new DashboardConfiguration();
}
