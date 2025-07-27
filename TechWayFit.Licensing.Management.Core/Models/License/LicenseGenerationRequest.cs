namespace TechWayFit.Licensing.Management.Core.Models.License;

/// <summary>
/// License generation request
/// </summary>
public class LicenseGenerationRequest
{
    public string ProductId { get; set; } = string.Empty;
    public string ConsumerId { get; set; } = string.Empty;
    public string? TierId { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? MaxUsers { get; set; }
    public int? MaxDevices { get; set; }
    public bool AllowOfflineUsage { get; set; } = false;
    public bool AllowVirtualization { get; set; } = false;
    public string? Notes { get; set; }
    public Dictionary<string, object> CustomProperties { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}
