using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.OperationsDashboard;
using TechWayFit.Licensing.Management.Infrastructure.Models.OperationsDashboard;

namespace TechWayFit.Licensing.Management.Web.ViewModels.OperationsDashboard;

/// <summary>
/// View model for detailed reports
/// </summary>
public class DetailedReportsViewModel
{
    /// <summary>
    /// Start date for the report
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date for the report
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Type of report
    /// </summary>
    public string ReportType { get; set; } = string.Empty;

    /// <summary>
    /// Detailed metrics data
    /// </summary>
    public IEnumerable<SystemMetricEntity> DetailedMetrics { get; set; } = new List<SystemMetricEntity>();

    /// <summary>
    /// Detailed errors data
    /// </summary>
    public IEnumerable<ErrorLogSummaryEntity> DetailedErrors { get; set; } = new List<ErrorLogSummaryEntity>();

    /// <summary>
    /// Detailed performance data
    /// </summary>
    public IEnumerable<PagePerformanceMetricEntity> DetailedPerformance { get; set; } = new List<PagePerformanceMetricEntity>();

    /// <summary>
    /// Detailed query performance data
    /// </summary>
    public IEnumerable<QueryPerformanceMetricEntity> DetailedQueries { get; set; } = new List<QueryPerformanceMetricEntity>();

    /// <summary>
    /// Dashboard configuration
    /// </summary>
    public DashboardConfiguration Configuration { get; set; } = new DashboardConfiguration();
}
