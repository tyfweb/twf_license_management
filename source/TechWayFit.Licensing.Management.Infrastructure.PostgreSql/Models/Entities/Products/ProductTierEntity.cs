using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.License;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.Common;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.Products;

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

    #region IEntityMapper Implementation
     public   ProductTierEntity Map(ProductTier model)
    {
        if (model == null) return null!;

        return new ProductTierEntity
        {
            Id = model.TierId,
            ProductId = model.ProductId,
            Name = model.Name,
            Description = model.Description,
            DisplayOrder = model.DisplayOrder,
            IsActive = model.Audit.IsActive,
            IsDeleted = model.Audit.IsDeleted,
            CreatedBy = model.Audit.CreatedBy,
            CreatedOn = model.Audit.CreatedOn,
            UpdatedBy = model.Audit.UpdatedBy,
            UpdatedOn = model.Audit.UpdatedOn,
            DeletedBy = model.Audit.DeletedBy,
            DeletedOn = model.Audit.DeletedOn,
            EntityStatus = (int)model.Workflow.Status,
            SubmittedBy = model.Workflow.SubmittedBy,
            SubmittedOn = model.Workflow.SubmittedOn,
            ReviewedBy = model.Workflow.ReviewedBy,
            ReviewedOn = model.Workflow.ReviewedOn,
            ReviewComments = model.Workflow.ReviewComments,
            RowVersion = model.Audit.RowVersion
        };
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
            DisplayOrder = this.DisplayOrder,
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
