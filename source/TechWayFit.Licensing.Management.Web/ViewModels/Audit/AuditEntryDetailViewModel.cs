namespace TechWayFit.Licensing.Management.Web.ViewModels.Audit;

/// <summary>
/// Detailed audit entry view model
/// </summary>
public class AuditEntryDetailViewModel
{
    public AuditEntryItemViewModel Entry { get; set; } = new();
    public IEnumerable<AuditEntryItemViewModel> RelatedEntries { get; set; } = new List<AuditEntryItemViewModel>();
    public string? FormattedOldValue { get; set; }
    public string? FormattedNewValue { get; set; }
    public IEnumerable<AuditChangeViewModel> Changes { get; set; } = new List<AuditChangeViewModel>();
    public bool HasRelatedEntries => RelatedEntries.Any();
    public string EntityDisplayName => GetEntityDisplayName();

    private string GetEntityDisplayName()
    {
        if (string.IsNullOrEmpty(Entry.EntityType) || string.IsNullOrEmpty(Entry.EntityId))
            return "Unknown Entity";

        return $"{Entry.EntityType} ({Entry.EntityId})";
    }
}
