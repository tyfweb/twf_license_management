using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.OperationsDashboard;
using TechWayFit.Licensing.Management.Infrastructure.Models.OperationsDashboard;

namespace TechWayFit.Licensing.Management.Web.ViewModels.OperationsDashboard;

/// <summary>
/// View model for system health monitoring
/// </summary>
public class SystemHealthViewModel
{
    /// <summary>
    /// Time range in hours
    /// </summary>
    public int TimeRange { get; set; }

    /// <summary>
    /// Current system health snapshot
    /// </summary>
    public SystemHealthSnapshotEntity? CurrentHealth { get; set; }

    /// <summary>
    /// Health trends data
    /// </summary>
    public IEnumerable<HealthTrend> HealthTrends { get; set; } = new List<HealthTrend>();

    /// <summary>
    /// Dashboard configuration
    /// </summary>
    public DashboardConfiguration Configuration { get; set; } = new DashboardConfiguration();
}
