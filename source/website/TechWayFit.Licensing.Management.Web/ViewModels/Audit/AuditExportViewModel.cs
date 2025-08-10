namespace TechWayFit.Licensing.Management.Web.ViewModels.Audit;

/// <summary>
/// Export settings for audit data
/// </summary>
public class AuditExportViewModel
{
    public string Format { get; set; } = "csv"; // csv, json, xml
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? ActionType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IncludeMetadata { get; set; } = true;
    public string FileName => $"audit_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{Format}";
}
