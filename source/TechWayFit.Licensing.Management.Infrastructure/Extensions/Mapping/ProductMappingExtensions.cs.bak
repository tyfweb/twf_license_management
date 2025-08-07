using System.Text.Json;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;

namespace TechWayFit.Licensing.Management.Infrastructure.Extensions.Mapping;

/// <summary>
/// Extension methods for mapping between EnterpriseProduct core model and ProductEntity
/// </summary>
public static class ProductMappingExtensions
{
    /// <summary>
    /// Converts EnterpriseProduct core model to ProductEntity
    /// </summary>
    public static ProductEntity ToEntity(this EnterpriseProduct model)
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
    public static EnterpriseProduct ToModel(this ProductEntity entity)
    {
        if (entity == null) return null!;

        return new EnterpriseProduct
        {
            ProductId = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            ReleaseDate = entity.ReleaseDate,
            SupportEmail = entity.SupportEmail,
            SupportPhone = entity.SupportPhone,
            DecommissionDate = entity.DecommissionDate,
            Status = Enum.TryParse<ProductStatus>(entity.Status, out var status) ? status : ProductStatus.Active,
            Metadata = !string.IsNullOrEmpty(entity.MetadataJson)
                        ? JsonSerializer.Deserialize<Dictionary<string, string>>(entity.MetadataJson) ?? new Dictionary<string, string>()
                        : new Dictionary<string, string>(),
            Audit = new AuditInfo
            {
                IsActive = entity.IsActive,
                IsDeleted = entity.IsDeleted,
                CreatedBy = entity.CreatedBy,
                CreatedOn = entity.CreatedOn,
                UpdatedBy = entity.UpdatedBy,
                UpdatedOn = entity.UpdatedOn,
                DeletedBy = entity.DeletedBy,
                DeletedOn = entity.DeletedOn,
                RowVersion = entity.RowVersion
            },
            Workflow = new WorkflowInfo
            {
                Status = (Core.Models.Common.EntityStatus)entity.EntityStatus,
                SubmittedBy = entity.SubmittedBy,
                SubmittedOn = entity.SubmittedOn,
                ReviewedBy = entity.ReviewedBy,
                ReviewedOn = entity.ReviewedOn,
                ReviewComments = entity.ReviewComments
            }
        };
    }

    /// <summary>
    /// Updates existing ProductEntity with values from EnterpriseProduct core model
    /// </summary>
    public static void UpdateFromModel(this ProductEntity entity, EnterpriseProduct model)
    {
        if (entity == null || model == null) return;

        entity.Name = model.Name;
        entity.Description = model.Description;
        entity.ReleaseDate = model.ReleaseDate;
        entity.SupportEmail = model.SupportEmail;
        entity.SupportPhone = model.SupportPhone;
        entity.DecommissionDate = model.DecommissionDate;
        entity.Status = model.Status.ToString();
        entity.MetadataJson = model.Metadata != null ? JsonSerializer.Serialize(model.Metadata) : "{}";
        entity.IsActive = model.Audit.IsActive;
        entity.IsDeleted = model.Audit.IsDeleted;
        entity.UpdatedBy = model.Audit.UpdatedBy;
        entity.UpdatedOn = model.Audit.UpdatedOn;
        entity.DeletedBy = model.Audit.DeletedBy;
        entity.DeletedOn = model.Audit.DeletedOn;
        entity.EntityStatus = (int)model.Workflow.Status;
        entity.SubmittedBy = model.Workflow.SubmittedBy;
        entity.SubmittedOn = model.Workflow.SubmittedOn;
        entity.ReviewedBy = model.Workflow.ReviewedBy;
        entity.ReviewedOn = model.Workflow.ReviewedOn;
        entity.ReviewComments = model.Workflow.ReviewComments;
        entity.RowVersion = model.Audit.RowVersion;
    }
}
