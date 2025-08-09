namespace TechWayFit.Licensing.Management.Web.ViewModels.Audit;

/// <summary>
/// Audit entries list view model with pagination
/// </summary>
public class AuditListViewModel
{
    public IEnumerable<AuditEntryItemViewModel> Entries { get; set; } = new List<AuditEntryItemViewModel>();
    public AuditFilterViewModel Filter { get; set; } = new();
    public PaginationViewModel Pagination { get; set; } = new();
    public IEnumerable<string> AvailableEntityTypes { get; set; } = new List<string>();
    public IEnumerable<string> AvailableActions { get; set; } = new List<string>();
    public int TotalEntries { get; set; }
    public bool HasResults => Entries.Any();
}
