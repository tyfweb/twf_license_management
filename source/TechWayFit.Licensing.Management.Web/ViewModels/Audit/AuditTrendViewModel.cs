namespace TechWayFit.Licensing.Management.Web.ViewModels.Audit;

/// <summary>
/// Activity trend data for charts
/// </summary>
public class AuditTrendViewModel
{
    public DateTime Date { get; set; }
    public int EntryCount { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string DateLabel => Date.ToString("MMM dd");
}
