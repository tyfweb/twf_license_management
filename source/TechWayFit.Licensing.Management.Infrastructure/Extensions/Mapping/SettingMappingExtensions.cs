using TechWayFit.Licensing.Management.Core.Models.Settings;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Settings;

namespace TechWayFit.Licensing.Management.Infrastructure.Extensions.Mapping;

/// <summary>
/// Extension methods for mapping between Setting core model and SettingEntity
/// </summary>
public static class SettingMappingExtensions
{
    /// <summary>
    /// Converts Setting core model to SettingEntity
    /// </summary>
    public static SettingEntity ToEntity(this Setting model)
    {
        if (model == null) return null!;

        return new SettingEntity
        {
            Id = model.SettingId,
            Category = model.Category,
            Key = model.Key,
            Value = model.Value,
            DefaultValue = model.DefaultValue,
            DataType = model.DataType,
            DisplayName = model.DisplayName,
            Description = model.Description,
            DisplayOrder = model.SortOrder,
            IsEditable = !model.IsReadOnly,
            IsRequired = model.IsRequired,
            ValidationRules = model.ValidationPattern,
            IsActive = model.Audit.IsActive,
            IsDeleted = model.Audit.IsDeleted,
            CreatedBy = model.Audit.CreatedBy,
            CreatedOn = model.Audit.CreatedOn,
            UpdatedBy = model.Audit.UpdatedBy,
            UpdatedOn = model.Audit.UpdatedOn,
            DeletedBy = model.Audit.DeletedBy,
            DeletedOn = model.Audit.DeletedOn,
            RowVersion = model.Audit.RowVersion
        };
    }

    /// <summary>
    /// Converts SettingEntity to Setting core model
    /// </summary>
    public static Setting ToModel(this SettingEntity entity)
    {
        if (entity == null) return null!;

        return new Setting
        {
            SettingId = entity.Id,
            Category = entity.Category,
            Key = entity.Key,
            Value = entity.Value,
            DefaultValue = entity.DefaultValue,
            DataType = entity.DataType,
            DisplayName = entity.DisplayName,
            Description = entity.Description,
            SortOrder = entity.DisplayOrder,
            IsReadOnly = !entity.IsEditable,
            IsRequired = entity.IsRequired,
            ValidationPattern = entity.ValidationRules,
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
            }
        };
    }

    /// <summary>
    /// Updates existing SettingEntity with values from Setting core model
    /// </summary>
    public static void UpdateFromModel(this SettingEntity entity, Setting model)
    {
        if (entity == null || model == null) return;

        entity.Category = model.Category;
        entity.Key = model.Key;
        entity.Value = model.Value;
        entity.DefaultValue = model.DefaultValue;
        entity.DataType = model.DataType;
        entity.DisplayName = model.DisplayName;
        entity.Description = model.Description;
        entity.DisplayOrder = model.SortOrder;
        entity.IsEditable = !model.IsReadOnly;
        entity.IsRequired = model.IsRequired;
        entity.ValidationRules = model.ValidationPattern;
        entity.IsActive = model.Audit.IsActive;
        entity.IsDeleted = model.Audit.IsDeleted;
        entity.CreatedBy = model.Audit.CreatedBy;
        entity.CreatedOn = model.Audit.CreatedOn;
        entity.UpdatedBy = model.Audit.UpdatedBy;
        entity.UpdatedOn = model.Audit.UpdatedOn;
        entity.DeletedBy = model.Audit.DeletedBy;
        entity.DeletedOn = model.Audit.DeletedOn;
        entity.RowVersion = model.Audit.RowVersion;
    }
}
