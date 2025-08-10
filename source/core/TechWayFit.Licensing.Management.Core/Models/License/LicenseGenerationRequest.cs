namespace TechWayFit.Licensing.Management.Core.Models.License;

/// <summary>
/// License generation request
/// </summary>
public class LicenseGenerationRequest
{    public Guid ProductId { get; set; } = Guid.Empty;
    public Guid ConsumerId { get; set; } = Guid.Empty;
    public Guid? TierId { get; set; } 
    public string? ProductName { get; set; }
    public string? ConsumerName { get; set; }
    public string? LicenseCode { get; set; }
    public string? ProductTier{ get; set; }
    public IEnumerable<string> Features { get; set; } = new List<string>();
    public DateTime? ValidFrom { get; set; } = DateTime.UtcNow; 
    public string ValidProductVersionFrom { get; set; } = "1.0.0";
    public string? ValidProductVersionTo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? MaxUsers { get; set; }
    public int? MaxDevices { get; set; }
    public string? Notes { get; set; }
    public Dictionary<string, object> CustomProperties { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}
