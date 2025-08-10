using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.Settings;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Settings;

/// <summary>
/// Entity model for system settings storage
/// </summary>
[Table("settings")]
public class SettingEntity : BaseEntity, IEntityMapper<Setting, SettingEntity>
{

    /// <summary>
    /// Category of the setting (e.g., "System", "Branding", "Email", "Security")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Unique key within the category
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Current value of the setting (stored as JSON string for complex objects)
    /// </summary>
    [MaxLength(4000)]
    public string? Value { get; set; }

    /// <summary>
    /// Default value for the setting
    /// </summary>
    [MaxLength(4000)]
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Data type of the setting value (string, int, bool, json, etc.)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string DataType { get; set; } = "string";

    /// <summary>
    /// Display name for the UI
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Description/help text for the setting
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Category for grouping in UI
    /// </summary>
    [MaxLength(100)]
    public string? GroupName { get; set; }

    /// <summary>
    /// Display order within the group/category
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Whether this setting can be edited through the UI
    /// </summary>
    public bool IsEditable { get; set; } = true;

    /// <summary>
    /// Whether this setting is required (cannot be empty)
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// Whether this setting contains sensitive data (passwords, API keys)
    /// </summary>
    public bool IsSensitive { get; set; } = false;

    /// <summary>
    /// JSON string containing validation rules (min/max length, regex pattern, etc.)
    /// </summary>
    [MaxLength(2000)]
    public string? ValidationRules { get; set; }

    /// <summary>
    /// JSON string containing possible values for dropdown/select inputs
    /// </summary>
    [MaxLength(2000)]
    public string? PossibleValues { get; set; }

    /// <summary>
    /// Full key combining category and key (for quick lookups)
    /// </summary>
    [MaxLength(201)] // Category(100) + "." + Key(100) + 1
    public string FullKey => $"{Category}.{Key}";

    #region IEntityMapper Implementation
     public SettingEntity Map(Setting model)
    {
        if (model == null) return null!;

        Id = model.SettingId;
        TenantId = model.TenantId;
        Category = model.Category;
        Key = model.Key;
        Value = model.Value;
        DefaultValue = model.DefaultValue;
        DataType = model.DataType;
        DisplayName = model.DisplayName;
        Description = model.Description;
        DisplayOrder = model.SortOrder;
        IsEditable = !model.IsReadOnly;
        IsRequired = model.IsRequired;
        ValidationRules = model.ValidationPattern;
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

    /// <summary>
    /// Converts SettingEntity to Setting core model
    /// </summary>
    public Setting Map()
    {         
        return new Setting
        {
            SettingId = this.Id,
            TenantId = this.TenantId,
            Category = this.Category,
            Key = this.Key,
            Value = this.Value,
            DefaultValue = this.DefaultValue,
            DataType = this.DataType,
            DisplayName = this.DisplayName,
            Description = this.Description,
            SortOrder = this.DisplayOrder,
            IsReadOnly = !this.IsEditable,
            IsRequired = this.IsRequired,
            ValidationPattern = this.ValidationRules,
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
