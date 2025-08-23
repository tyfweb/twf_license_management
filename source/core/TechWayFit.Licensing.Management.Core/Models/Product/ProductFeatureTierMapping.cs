using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Core.Models.Product;

/// <summary>
/// Mapping entity to establish many-to-many relationship between ProductFeature and ProductTier
/// </summary>
public class ProductFeatureTierMapping
{
    /// <summary>
    /// Unique identifier for the mapping
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tenant identifier for multi-tenant isolation
    /// </summary>
    public Guid TenantId { get; set; } = Guid.Empty;

    /// <summary>
    /// Foreign key to ProductFeature
    /// </summary>
    public Guid ProductFeatureId { get; set; } = Guid.Empty;

    /// <summary>
    /// Foreign key to ProductTier
    /// </summary>
    public Guid ProductTierId { get; set; } = Guid.Empty;

    /// <summary>
    /// Indicates if this feature is enabled for this tier
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Display order for this feature within the tier
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Optional configuration specific to this feature-tier combination
    /// </summary>
    public string? Configuration { get; set; }

    /// <summary>
    /// Audit information for the mapping
    /// </summary>
    public AuditInfo Audit { get; set; } = new();

    /// <summary>
    /// Navigation property to ProductFeature
    /// </summary>
    public ProductFeature? ProductFeature { get; set; }

    /// <summary>
    /// Navigation property to ProductTier
    /// </summary>
    public ProductTier? ProductTier { get; set; }
}
