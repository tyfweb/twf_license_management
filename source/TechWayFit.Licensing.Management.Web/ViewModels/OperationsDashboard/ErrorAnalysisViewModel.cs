using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.OperationsDashboard;
using TechWayFit.Licensing.Management.Infrastructure.Models.OperationsDashboard;

namespace TechWayFit.Licensing.Management.Web.ViewModels.OperationsDashboard;

/// <summary>
/// View model for error analysis
/// </summary>
public class ErrorAnalysisViewModel
{
    /// <summary>
    /// Time range in hours
    /// </summary>
    public int TimeRange { get; set; }

    /// <summary>
    /// Error summary for the time range
    /// </summary>
    public IEnumerable<ErrorLogSummaryEntity> ErrorSummary { get; set; } = new List<ErrorLogSummaryEntity>();

    /// <summary>
    /// Top errors
    /// </summary>
    public IEnumerable<ErrorLogSummaryEntity> TopErrors { get; set; } = new List<ErrorLogSummaryEntity>();

    /// <summary>
    /// Error trends data
    /// </summary>
    public IEnumerable<ErrorTrend> ErrorTrends { get; set; } = new List<ErrorTrend>();

    /// <summary>
    /// Dashboard configuration
    /// </summary>
    public DashboardConfiguration Configuration { get; set; } = new DashboardConfiguration();
}
