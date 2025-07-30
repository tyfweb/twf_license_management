using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Infrastructure.Models.Entities.Settings;

/// <summary>
/// Entity model for system settings storage
/// </summary>
public class SettingEntity : BaseAuditEntity
{
    /// <summary>
    /// Unique identifier for the setting
    /// </summary>
    [Key]
    public string SettingId { get; set; } = Guid.NewGuid().ToString();

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

    /// <summary>
    /// Source of the current value (Default, Configuration, Database, User)
    /// </summary>
    [MaxLength(50)]
    public string ValueSource { get; set; } = "Database";

    /// <summary>
    /// Tags for categorization and searching
    /// </summary>
    [MaxLength(500)]
    public string? Tags { get; set; }

    /// <summary>
    /// Whether this setting requires application restart to take effect
    /// </summary>
    public bool RequiresRestart { get; set; } = false;

    /// <summary>
    /// Environment where this setting applies (Development, Staging, Production, All)
    /// </summary>
    [MaxLength(50)]
    public string Environment { get; set; } = "All";

    /// <summary>
    /// Version when this setting was introduced
    /// </summary>
    [MaxLength(20)]
    public string? IntroducedInVersion { get; set; }

    /// <summary>
    /// Whether this setting is deprecated
    /// </summary>
    public bool IsDeprecated { get; set; } = false;

    /// <summary>
    /// Deprecation message
    /// </summary>
    [MaxLength(500)]
    public string? DeprecationMessage { get; set; }

    /// <summary>
    /// Convert from Core model to Entity
    /// </summary>
    /// <param name="model">Core Setting model</param>
    /// <returns>SettingEntity</returns>
    public static SettingEntity FromModel(TechWayFit.Licensing.Management.Core.Models.Settings.Setting model)
    {
        return new SettingEntity
        {
            SettingId = model.SettingId,
            Category = model.Category,
            Key = model.Key,
            Value = model.Value,
            DefaultValue = model.DefaultValue,
            DataType = model.DataType,
            DisplayName = model.DisplayName,
            Description = model.Description,
            GroupName = string.Empty, // Map this appropriately if needed
            DisplayOrder = model.SortOrder,
            IsEditable = !model.IsReadOnly,
            IsRequired = model.IsRequired,
            IsSensitive = false, // Set based on your business logic
            ValidationRules = model.ValidationPattern,
            PossibleValues = null, // Set if you have this data
            ValueSource = "Database",
            Tags = null, // Set if you have tags
            RequiresRestart = false, // Set based on your business logic
            Environment = "All",
            IntroducedInVersion = null,
            IsDeprecated = false,
            DeprecationMessage = null,
            IsActive = true,
            CreatedBy = model.CreatedBy,
            CreatedOn = model.CreatedAt,
            UpdatedBy = model.UpdatedBy,
            UpdatedOn = model.UpdatedAt
        };
    }

    /// <summary>
    /// Convert Entity to Core model
    /// </summary>
    /// <returns>Core Setting model</returns>
    public TechWayFit.Licensing.Management.Core.Models.Settings.Setting ToModel()
    {
        return new TechWayFit.Licensing.Management.Core.Models.Settings.Setting
        {
            SettingId = SettingId,
            Category = Category,
            Key = Key,
            Value = Value,
            DefaultValue = DefaultValue,
            DataType = DataType,
            DisplayName = DisplayName,
            Description = Description,
            IsRequired = IsRequired,
            IsReadOnly = !IsEditable,
            ValidationPattern = ValidationRules,
            SortOrder = DisplayOrder,
            CreatedAt = CreatedOn,
            UpdatedAt = UpdatedOn ?? CreatedOn,
            CreatedBy = CreatedBy,
            UpdatedBy = UpdatedBy ?? CreatedBy
        };
    }
}
