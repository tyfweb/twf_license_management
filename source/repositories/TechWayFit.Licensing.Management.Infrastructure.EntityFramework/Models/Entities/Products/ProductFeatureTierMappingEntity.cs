using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Common;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Products;

/// <summary>
/// Database entity for ProductFeatureTierMapping
/// </summary>
[Table("product_feature_tier_mappings")]
public class ProductFeatureTierMappingEntity : BaseEntity, IEntityMapper<ProductFeatureTierMapping, ProductFeatureTierMappingEntity>
{
    /// <summary>
    /// Foreign key to ProductFeature
    /// </summary>
    public Guid ProductFeatureId { get; set; }

    /// <summary>
    /// Foreign key to ProductTier
    /// </summary>
    public Guid ProductTierId { get; set; }

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
    /// Navigation property to ProductFeature
    /// </summary>
    public virtual ProductFeatureEntity? ProductFeature { get; set; }

    /// <summary>
    /// Navigation property to ProductTier
    /// </summary>
    public virtual ProductTierEntity? ProductTier { get; set; }

    #region IEntityMapper Implementation

    public ProductFeatureTierMappingEntity Map(ProductFeatureTierMapping model)
    {
        if (model == null) return null!;

        Id = model.Id;
        TenantId = model.TenantId;
        ProductFeatureId = model.ProductFeatureId;
        ProductTierId = model.ProductTierId;
        IsEnabled = model.IsEnabled;
        DisplayOrder = model.DisplayOrder;
        Configuration = model.Configuration;
        IsActive = model.Audit.IsActive;
        IsDeleted = model.Audit.IsDeleted;
        CreatedBy = model.Audit.CreatedBy;
        CreatedOn = model.Audit.CreatedOn;
        UpdatedBy = model.Audit.UpdatedBy;
        UpdatedOn = model.Audit.UpdatedOn;
        DeletedBy = model.Audit.DeletedBy;
        DeletedOn = model.Audit.DeletedOn;
        RowVersion = model.Audit.RowVersion;

        return this;
    }

    public ProductFeatureTierMapping Map()
    {
        return new ProductFeatureTierMapping
        {
            Id = this.Id,
            TenantId = this.TenantId,
            ProductFeatureId = this.ProductFeatureId,
            ProductTierId = this.ProductTierId,
            IsEnabled = this.IsEnabled,
            DisplayOrder = this.DisplayOrder,
            Configuration = this.Configuration,
            Audit = new AuditInfo
            {
                IsActive = this.IsActive,
                IsDeleted = this.IsDeleted,
                CreatedBy = this.CreatedBy,
                CreatedOn = this.CreatedOn,
                UpdatedBy = this.UpdatedBy,
                UpdatedOn = this.UpdatedOn,
                DeletedBy = this.DeletedBy,
                DeletedOn = this.DeletedOn,
                RowVersion = this.RowVersion
            }
        };
    }

    #endregion
}
