using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.License;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Common;
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;

/// <summary>
/// Database entity for ProductFeature
/// </summary>
[Table("product_features")]
public class ProductFeatureEntity : AuditWorkflowEntity, IEntityMapper<ProductFeature, ProductFeatureEntity>
{

    /// <summary>
    /// Foreign key to Product
    /// </summary>

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

    #region IEntityMapper Implementation
     public ProductFeatureEntity Map(ProductFeature model)
    {
        if (model == null) return null!;

        return new ProductFeatureEntity
        {
            Id = model.FeatureId,
            ProductId = model.ProductId,
            TierId = model.TierId,
            Code = model.Code,
            Name = model.Name,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
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
    /// Converts ProductFeatureEntity to ProductFeature core model
    /// </summary>
    public ProductFeature Map()
    { 

        return new ProductFeature
        {
            FeatureId = this.Id,
            ProductId = this.ProductId,
            TierId = this.TierId,
            Code = this.Code,
            Name = this.Name,
            Description = this.Description,
            IsEnabled = this.IsEnabled,
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
