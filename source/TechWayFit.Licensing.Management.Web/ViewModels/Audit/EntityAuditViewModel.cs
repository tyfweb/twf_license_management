namespace TechWayFit.Licensing.Management.Web.ViewModels.Audit;

/// <summary>
/// Entity-specific audit view model
/// </summary>
public class EntityAuditViewModel
{
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string EntityDisplayName { get; set; } = string.Empty;
    public IEnumerable<AuditEntryItemViewModel> Entries { get; set; } = new List<AuditEntryItemViewModel>();
    public PaginationViewModel Pagination { get; set; } = new();
    public AuditStatisticsViewModel Statistics { get; set; } = new();
    public DateTime? DateRangeStart { get; set; }
    public DateTime? DateRangeEnd { get; set; }
}
