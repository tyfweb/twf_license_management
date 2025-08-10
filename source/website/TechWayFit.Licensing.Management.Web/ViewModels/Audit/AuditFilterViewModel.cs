namespace TechWayFit.Licensing.Management.Web.ViewModels.Audit;

/// <summary>
/// Filter criteria for audit searches
/// </summary>
public class AuditFilterViewModel
{
    public string? SearchTerm { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? ActionType { get; set; }
    public string? UserName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string SortBy { get; set; } = "Timestamp";
    public string SortOrder { get; set; } = "desc";
}
