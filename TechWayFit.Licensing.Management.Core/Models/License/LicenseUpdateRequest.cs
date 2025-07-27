namespace TechWayFit.Licensing.Management.Core.Models.License;

/// <summary>
/// License update request
/// </summary>
public class LicenseUpdateRequest
{
    public DateTime? ExpiryDate { get; set; }
    public int? MaxUsers { get; set; }
    public int? MaxDevices { get; set; }
    public bool? AllowOfflineUsage { get; set; }
    public bool? AllowVirtualization { get; set; }
    public string? Notes { get; set; }
    public Dictionary<string, object>? CustomProperties { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
