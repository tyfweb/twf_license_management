using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.License;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Common;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Products;

/// <summary>
/// Database entity for ProductTier
/// </summary>
[Table("product_tiers")]
public class ProductTierEntity : AuditWorkflowEntity, IEntityMapper<ProductTier, ProductTierEntity>
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
    /// Maximum number of users allowed for this tier
    /// </summary>
    public int? MaxUsers { get; set; }
    
    /// <summary>
    /// Maximum number of devices allowed for this tier
    /// </summary>
    public int? MaxDevices { get; set; }

    /// <summary>
    /// Navigation property to Product
    /// </summary>
    public virtual ProductEntity? Product { get; set; }

    /// <summary>
    /// Navigation property to Product Tier Prices
    /// </summary>
    public virtual ICollection<ProductTierPriceEntity> Prices { get; set; } = new List<ProductTierPriceEntity>();

    /// <summary>
    /// Navigation property to Product Feature Tier Mappings
    /// </summary>
    public virtual ICollection<ProductFeatureTierMappingEntity> FeatureMappings { get; set; } = new List<ProductFeatureTierMappingEntity>();

    /// <summary>
    /// Navigation property to Product Licenses using this tier
    /// </summary>
    public virtual ICollection<ProductLicenseEntity> Licenses { get; set; } = new List<ProductLicenseEntity>();

    #region IEntityMapper Implementation
     public   ProductTierEntity Map(ProductTier model)
    {
        if (model == null) return null!;

        Id = model.TierId;
        ProductId = model.ProductId;
        Name = model.Name;
        Description = model.Description;
        SupportSLAJson = JsonSerializer.Serialize(model.SupportSLA); // Serialize SupportSLA object to JSON
        DisplayOrder = model.DisplayOrder;
        MaxUsers = model.MaxUsers;
        MaxDevices = model.MaxDevices;
        IsActive = model.Audit.IsActive;
        IsDeleted = model.Audit.IsDeleted;
        CreatedBy = model.Audit.CreatedBy;
        CreatedOn = model.Audit.CreatedOn;
        UpdatedBy = model.Audit.UpdatedBy;
        UpdatedOn = model.Audit.UpdatedOn;
        DeletedBy = model.Audit.DeletedBy;
        DeletedOn = model.Audit.DeletedOn;
        EntityStatus = (int)model.Workflow.Status;
        SubmittedBy = model.Workflow.SubmittedBy;
        SubmittedOn = model.Workflow.SubmittedOn;
        ReviewedBy = model.Workflow.ReviewedBy;
        ReviewedOn = model.Workflow.ReviewedOn;
        ReviewComments = model.Workflow.ReviewComments;
        RowVersion = model.Audit.RowVersion;

        return this;
    }

    /// <summary>
    /// Converts ProductTierEntity to ProductTier core model
    /// </summary>
    public ProductTier Map()
    { 

        return new ProductTier
        {
            TierId = this.Id,
            ProductId = this.ProductId,
            Name = this.Name,
            Description = this.Description,
            SupportSLA = string.IsNullOrEmpty(this.SupportSLAJson) 
                ? ProductSupportSLA.NoSLA 
                : JsonSerializer.Deserialize<ProductSupportSLA>(this.SupportSLAJson) ?? ProductSupportSLA.NoSLA, // Deserialize JSON to SupportSLA object
            DisplayOrder = this.DisplayOrder,
            Prices = this.Prices?.Select(p => p.Map()).ToList() ?? new List<ProductTierPrice>(),
            FeatureMappings = this.FeatureMappings?.Select(fm => fm.Map()).ToList() ?? new List<ProductFeatureTierMapping>(),
            MaxUsers = this.MaxUsers ?? 0,
            MaxDevices = this.MaxDevices ?? 0,
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
            },
            Workflow = new WorkflowInfo
            {
                Status = (EntityStatus)this.EntityStatus,
                SubmittedBy = this.SubmittedBy,
                SubmittedOn = this.SubmittedOn,
                ReviewedBy = this.ReviewedBy,
                ReviewedOn = this.ReviewedOn,
                ReviewComments = this.ReviewComments
            }
        };
    }
    #endregion
}
