namespace TechWayFit.Licensing.Management.Core.Models.Audit;

/// <summary>
/// Audit entry model
/// </summary>
public class AuditEntry
{
    public string EntryId { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; } 
    public string UserName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Reason { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}
