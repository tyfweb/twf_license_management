using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Common;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;

/// <summary>
/// Database entity for ProductVersion
/// </summary>
[Table("product_versions")]
public class ProductVersionEntity : AuditWorkflowEntity, IEntityMapper<ProductVersion, ProductVersionEntity>
{

    /// <summary>
    /// Foreign key to Product
    /// </summary>
    public Guid ProductId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Version number
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Version name or codename
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Release date of this version
    /// </summary>
    public DateTime ReleaseDate { get; set; }

    /// <summary>
    /// End of life date for this version
    /// </summary>
    public DateTime? EndOfLifeDate { get; set; }

    public DateTime? SupportEndDate { get; set; }

    /// <summary>
    /// Release notes for this version
    /// </summary>
    public string ReleaseNotes { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is the current version
    /// </summary>
    public bool IsCurrent { get; set; } = false;


    /// <summary>
    /// Navigation property to Product
    /// </summary>
    public virtual ProductEntity? Product { get; set; }

    #region IEntityMapper Implementation
     /// <summary>
    /// Converts ProductVersion core model to ProductVersionEntity
    /// </summary>
    public ProductVersionEntity Map(ProductVersion model)
    {
        if (model == null) return null!;

        return new ProductVersionEntity
        {
            Id = model.VersionId,
            ProductId = model.ProductId,
            Name = model.Name,
            IsCurrent = model.IsCurrent,
            Version = model.Version.ToString(),
            ReleaseDate = model.ReleaseDate,
            EndOfLifeDate = model.EndOfLifeDate,
            SupportEndDate = model.SupportEndDate,
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
    /// Converts ProductVersionEntity to ProductVersion core model
    /// </summary>
    public ProductVersion Map()
    {
        return new ProductVersion
        {
            VersionId = this.Id,
            ProductId = this.ProductId,
            Name = this.Name,
            IsCurrent = this.IsCurrent,
            Version = Core.Models.Common.SemanticVersion.Parse(this.Version),
            ReleaseDate = this.ReleaseDate,
            EndOfLifeDate = this.EndOfLifeDate,
            SupportEndDate = this.SupportEndDate,
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
