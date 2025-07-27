namespace TechWayFit.Licensing.Management.Core.Models.License;

/// <summary>
/// License validation result
/// </summary>
public class LicenseValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public ProductLicense? License { get; set; }
    public Dictionary<string, object> ValidationDetails { get; set; } = new();
}
