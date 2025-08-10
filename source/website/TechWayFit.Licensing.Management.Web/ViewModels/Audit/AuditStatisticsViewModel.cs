namespace TechWayFit.Licensing.Management.Web.ViewModels.Audit;

/// <summary>
/// Audit statistics overview
/// </summary>
public class AuditStatisticsViewModel
{
    public int TotalEntries { get; set; }
    public int UniqueUsers { get; set; }
    public int UniqueEntities { get; set; }
    public IEnumerable<KeyValuePair<string, int>> TopActions { get; set; } = new List<KeyValuePair<string, int>>();
    public IEnumerable<KeyValuePair<string, int>> TopUsers { get; set; } = new List<KeyValuePair<string, int>>();
    public IEnumerable<KeyValuePair<string, int>> TopEntityTypes { get; set; } = new List<KeyValuePair<string, int>>();
    public IEnumerable<KeyValuePair<DateTime, int>> DailyActivity { get; set; } = new List<KeyValuePair<DateTime, int>>();
    public DateTime? DateRangeStart { get; set; }
    public DateTime? DateRangeEnd { get; set; }
}
