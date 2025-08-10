using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Core.Models.Tenant;

/// <summary>
/// Core model for tenant representing a client organization
/// </summary>
public class Tenant
{
    public Guid TenantId { get; set; } = Guid.NewGuid();
    public string TenantName { get; set; } = string.Empty;
    public string? TenantCode { get; set; }
    public string? Description { get; set; }
    public string? Website { get; set; }
}
