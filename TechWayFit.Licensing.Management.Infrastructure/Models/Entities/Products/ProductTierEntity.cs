using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Infrastructure.Models.Entities.License;
using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Infrastructure.Models.Entities.Products;

/// <summary>
/// Database entity for ProductTier
/// </summary>
[Table("product_tiers")]
public class ProductTierEntity : BaseAuditEntity
{
    /// <summary>
    /// Unique identifier for the product tier
    /// </summary>
    public string TierId { get; set; } = string.Empty;

    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

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

    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to Product
    /// </summary>
    public virtual ProductEntity? Product { get; set; }

    public string Price { get; set; } = "USD 0.00"; // Assuming price is a string for currency formatting

    /// <summary>
    /// Navigation property to Product Features
    /// </summary>
    public virtual ICollection<ProductFeatureEntity> Features { get; set; } = new List<ProductFeatureEntity>();

    public static ProductTierEntity FromModel(ProductTier model)
    {
        return new ProductTierEntity
        {
            TierId = model.TierId,
            Name = model.Name,
            DisplayOrder = model.DisplayOrder,
            Description = model.Description,
            SupportSLAJson = ToJson(model.SupportSLA),
            ProductId = model.ProductId,
            IsActive = model.IsActive,
            CreatedBy = "system", // Assuming system created, adjust as needed
            CreatedOn = DateTime.UtcNow,
            UpdatedBy = "system", // Assuming system updated, adjust as needed
            UpdatedOn = DateTime.UtcNow,
            Price = model.Price,
            Features = model.Features.Select(ProductFeatureEntity.FromModel).ToList()
        };
    }
    public ProductTier ToModel()
    {
        return new ProductTier
        {
            TierId = TierId,
            Name = Name,
            Description = Description,
            SupportSLA = FromJson<ProductSupportSLA>(SupportSLAJson),
            ProductId = ProductId,
            IsActive = IsActive,
            Price = Price,
            DisplayOrder = DisplayOrder,
            Features = Features.Select(f => f.ToModel()).ToList()
        };
    }    

}
