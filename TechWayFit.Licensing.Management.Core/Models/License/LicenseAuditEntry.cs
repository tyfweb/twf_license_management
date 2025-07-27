namespace TechWayFit.Licensing.Management.Core.Models.License;

/// <summary>
/// License audit entry
/// </summary>
public class LicenseAuditEntry
{
    public string EntryId { get; set; } = string.Empty;
    public string LicenseId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime ModifiedDate { get; set; }
    public string? Reason { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
