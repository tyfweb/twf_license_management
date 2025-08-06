using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.License;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Common;
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;

/// <summary>
/// Database entity for ProductFeature
/// </summary>
[Table("product_features")]
public class ProductFeatureEntity : AuditWorkflowEntity
{
    public Guid ProductId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key to Product Tier
    /// </summary>
    public Guid TierId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Name of the feature
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the feature
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Feature code or identifier
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Whether this feature is enabled by default
    /// </summary>
    public bool IsEnabled { get; set; } = true;
 

    /// <summary>
    /// Display order for this feature
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    public string SupportFromVersion { get; set; } = "1.0.0"; // Default version, adjust as needed
    public string SupportToVersion { get; set; } = "9999.0.0"; // Default to no end version
    public string FeatureUsageJson { get; set; } = "{}"; // Assuming usage is stored as JSON

    /// <summary>
    /// Navigation property to Product Tier
    /// </summary>
    public virtual ProductTierEntity? Tier { get; set; }

    /// <summary>
    /// Navigation property to License Features
    /// </summary>
    public virtual ICollection<ProductLicenseEntity> ProductLicenses { get; set; } = new List<ProductLicenseEntity>();
}
