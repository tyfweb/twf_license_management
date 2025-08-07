using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;

namespace TechWayFit.Licensing.Management.Infrastructure.Extensions.Mapping;

/// <summary>
/// Extension methods for mapping between ProductFeature core model and ProductFeatureEntity
/// </summary>
public static class ProductFeatureMappingExtensions
{
    /// <summary>
    /// Converts ProductFeature core model to ProductFeatureEntity
    /// </summary>
    public static ProductFeatureEntity ToEntity(this ProductFeature model)
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
    public static ProductFeature ToModel(this ProductFeatureEntity entity)
    {
        if (entity == null) return null!;

        return new ProductFeature
        {
            FeatureId = entity.Id,
            ProductId = entity.ProductId,
            TierId = entity.TierId,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            IsEnabled = entity.IsEnabled,
            DisplayOrder = entity.DisplayOrder,
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
                Status = (EntityStatus)entity.EntityStatus,
                SubmittedBy = entity.SubmittedBy,
                SubmittedOn = entity.SubmittedOn,
                ReviewedBy = entity.ReviewedBy,
                ReviewedOn = entity.ReviewedOn,
                ReviewComments = entity.ReviewComments
            }
        };
    }

    /// <summary>
    /// Updates existing ProductFeatureEntity with values from ProductFeature core model
    /// </summary>
    public static void UpdateFromModel(this ProductFeatureEntity entity, ProductFeature model)
    {
        if (entity == null || model == null) return;

        entity.ProductId = model.ProductId;
        entity.TierId = model.TierId;
        entity.Code = model.Code;
        entity.Name = model.Name;
        entity.Description = model.Description;
        entity.IsEnabled = model.IsEnabled;
        entity.DisplayOrder = model.DisplayOrder;
        entity.IsActive = model.Audit.IsActive;
        entity.IsDeleted = model.Audit.IsDeleted;
        entity.CreatedBy = model.Audit.CreatedBy;
        entity.CreatedOn = model.Audit.CreatedOn;
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
