namespace TechWayFit.Licensing.Management.Core.Contracts.Services;

/// <summary>
/// License activation information
/// </summary>
public class ActivationInfo
{
    public string? MachineId { get; set; }
    public string? ActivatedBy { get; set; }
    public DateTime ActivationDate { get; set; }
    public Dictionary<string, object> ActivationMetadata { get; set; } = new();
}
