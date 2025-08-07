using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Core.Helpers;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.License;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Common;
using TechWayFit.Licensing.Management.Core.Models.Product;
using System.Text.Json.Serialization;
using System.Text.Json;
using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;

/// <summary>
/// Database entity for EnterpriseProduct
/// </summary>
[Table("products")]
public class ProductEntity : AuditWorkflowEntity, IEntityMapper<EnterpriseProduct, ProductEntity>
{

    /// <summary>
    /// Name of the product
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the product
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Release date of the product
    /// </summary>
    public DateTime ReleaseDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Support contact email for the product
    /// </summary>
    public string SupportEmail { get; set; } = string.Empty;

    /// <summary>
    /// Support contact phone number for the product
    /// </summary>
    public string SupportPhone { get; set; } = string.Empty;

    /// <summary>
    /// Product decommission date, if applicable
    /// </summary>
    public DateTime? DecommissionDate { get; set; }

    /// <summary>
    /// Status of the product
    /// </summary>
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Additional metadata for the product (JSON)
    /// </summary>
    public string MetadataJson { get; set; } = "{}";

    /// <summary>
    /// Navigation property to Product Versions
    /// </summary>
    public virtual ICollection<ProductVersionEntity> Versions { get; set; } = new List<ProductVersionEntity>();

    /// <summary>
    /// Navigation property to Product Tiers
    /// </summary>
    public virtual ICollection<ProductTierEntity> Tiers { get; set; } = new List<ProductTierEntity>();

    /// <summary>
    /// Navigation property to Product Licenses
    /// </summary>
    public virtual ICollection<ProductLicenseEntity> Licenses { get; set; } = new List<ProductLicenseEntity>();

    public virtual ICollection<ProductConsumerEntity> ProductConsumers { get; set; } = new List<ProductConsumerEntity>();

    #region IEntityMapper Implementation
    public ProductEntity Map(EnterpriseProduct model)
    {
        if (model == null) return null!;

        return new ProductEntity
        {
            Id = model.ProductId,
            Name = model.Name,
            Description = model.Description,
            ReleaseDate = model.ReleaseDate,
            SupportEmail = model.SupportEmail,
            SupportPhone = model.SupportPhone,
            DecommissionDate = model.DecommissionDate,
            Status = model.Status.ToString(),
            MetadataJson = model.Metadata != null ? JsonSerializer.Serialize(model.Metadata) : "{}",
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
    /// Converts ProductEntity to EnterpriseProduct core model
    /// </summary>
    public EnterpriseProduct Map()
    {

        return new EnterpriseProduct
        {
            ProductId = this.Id,
            Name = this.Name,
            Description = this.Description,
            ReleaseDate = this.ReleaseDate,
            SupportEmail = this.SupportEmail,
            SupportPhone = this.SupportPhone,
            DecommissionDate = this.DecommissionDate,
            Status = Enum.TryParse<ProductStatus>(this.Status, out var status) ? status : ProductStatus.Active,
            Metadata = !string.IsNullOrEmpty(this.MetadataJson)
                        ? JsonSerializer.Deserialize<Dictionary<string, string>>(this.MetadataJson) ?? new Dictionary<string, string>()
                        : new Dictionary<string, string>(),
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
                Status = (Core.Models.Common.EntityStatus)this.EntityStatus,
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
