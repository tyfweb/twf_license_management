namespace TechWayFit.Licensing.Management.Web.Models;

/// <summary>
/// Request model for bulk license export operations
/// </summary>
public class BulkExportRequest
{
    /// <summary>
    /// List of license IDs to export
    /// </summary>
    public List<Guid> LicenseIds { get; set; } = new();

    /// <summary>
    /// Export format: "json", "xml", "zip" (default)
    /// </summary>
    public string? Format { get; set; } = "zip";

    /// <summary>
    /// Optional filter for license status
    /// </summary>
    public string? StatusFilter { get; set; }

    /// <summary>
    /// Include additional metadata in export
    /// </summary>
    public bool IncludeMetadata { get; set; } = true;

    /// <summary>
    /// Include download statistics in export
    /// </summary>
    public bool IncludeStats { get; set; } = false;
}
