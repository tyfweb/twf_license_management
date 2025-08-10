using TechWayFit.Licensing.Management.Core.Models.Audit;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Audit;

/// <summary>
/// Main dashboard view model for audit overview
/// </summary>
public class AuditDashboardViewModel
{
    public AuditStatistics Statistics { get; set; } = new();
    public IEnumerable<AuditEntryItemViewModel> RecentEntries { get; set; } = new List<AuditEntryItemViewModel>();
    public IEnumerable<string> AvailableEntityTypes { get; set; } = new List<string>();
    public IEnumerable<string> AvailableActions { get; set; } = new List<string>();
    public DateTime? DateRangeStart { get; set; }
    public DateTime? DateRangeEnd { get; set; }
    public int TotalEntriesToday { get; set; }
    public int TotalEntriesWeek { get; set; }
    public int TotalEntriesMonth { get; set; }
    public IEnumerable<AuditTrendViewModel> ActivityTrends { get; set; } = new List<AuditTrendViewModel>();
}
