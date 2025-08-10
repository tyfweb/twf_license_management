namespace TechWayFit.Licensing.Management.Web.ViewModels.Audit;

/// <summary>
/// Represents a change within an audit entry
/// </summary>
public class AuditChangeViewModel
{
    public string PropertyName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string ChangeType { get; set; } = string.Empty; // Added, Modified, Removed
    public string PropertyDisplayName => PropertyName.ToTitleCase();
}
