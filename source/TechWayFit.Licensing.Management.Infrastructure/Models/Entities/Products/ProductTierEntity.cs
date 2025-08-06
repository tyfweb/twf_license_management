using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.License;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Common;
using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;

/// <summary>
/// Database entity for ProductTier
/// </summary>
[Table("product_tiers")]
public class ProductTierEntity : AuditWorkflowEntity
{

    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Name of the product tier
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the product tier
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Support SLA for this product tier
    /// </summary>
    public string SupportSLAJson { get; set; } = "{}";

    public Guid ProductId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Navigation property to Product
    /// </summary>
    public virtual ProductEntity? Product { get; set; }

    public string Price { get; set; } = "USD 0.00"; // Assuming price is a string for currency formatting    /// <summary>
    /// Navigation property to Product Features
    /// </summary>
    public virtual ICollection<ProductFeatureEntity> Features { get; set; } = new List<ProductFeatureEntity>();

    /// <summary>
    /// Navigation property to Product Licenses using this tier
    /// </summary>
    public virtual ICollection<ProductLicenseEntity> Licenses { get; set; } = new List<ProductLicenseEntity>();
}
