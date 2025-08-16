using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Settings
{
    /// <summary>
    /// View model for displaying a single setting
    /// </summary>
    public class SettingViewModel
    {
        /// <summary>
        /// Unique identifier for the setting
        /// </summary>
        public string SettingId { get; set; } = string.Empty;

        /// <summary>
        /// Category group for organizing settings
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Unique key within the category
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Current value of the setting
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Default value for the setting
        /// </summary>
        public string? DefaultValue { get; set; }

        /// <summary>
        /// Human-readable display name
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description of the setting
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Data type for validation and conversion
        /// </summary>
        public string DataType { get; set; } = "String";

        /// <summary>
        /// Whether this setting is required
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Whether this setting is read-only
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Regular expression pattern for validation
        /// </summary>
        public string? ValidationPattern { get; set; }

        /// <summary>
        /// Sort order within the category
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Possible values for list-type settings (JSON array)
        /// </summary>
        public string? PossibleValues { get; set; }

        /// <summary>
        /// When the setting was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the setting was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// User who created the setting
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// User who last updated the setting
        /// </summary>
        public string UpdatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Get the full setting key in Category.Key format
        /// </summary>
        public string FullKey => $"{Category}.{Key}";

        /// <summary>
        /// Check if the current value is different from the default
        /// </summary>
        public bool IsModified => !string.Equals(Value, DefaultValue, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Get the input type for HTML form rendering
        /// </summary>
        public string InputType
        {
            get
            {
                return DataType.ToLowerInvariant() switch
                {
                    "boolean" or "bool" => "checkbox",
                    "integer" or "int" => "number",
                    "decimal" or "double" or "float" => "number",
                    "datetime" or "date" => "datetime-local",
                    "email" => "email",
                    "url" => "url",
                    "password" => "password",
                    "json" => "textarea",
                    _ => "text"
                };
            }
        }

        /// <summary>
        /// Get CSS class for styling based on setting state
        /// </summary>
        public string CssClass
        {
            get
            {
                var classes = new List<string>();

                if (IsReadOnly)
                    classes.Add("readonly");

                if (IsRequired)
                    classes.Add("required");

                if (IsModified)
                    classes.Add("modified");

                return string.Join(" ", classes);
            }
        }
    }
}
