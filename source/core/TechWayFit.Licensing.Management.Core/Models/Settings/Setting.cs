using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Core.Models.Settings
{
    /// <summary>
    /// Core model for system settings that can be configured through the web interface
    /// Uses composition over inheritance for better readability and flexibility
    /// </summary>
    public class Setting
    {
        /// <summary>
        /// Unique identifier for the setting
        /// </summary>
        public Guid SettingId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Tenant identifier for multi-tenant isolation
        /// </summary>
        public Guid TenantId { get; set; } = Guid.Empty;

        /// <summary>
        /// Audit information - composition over inheritance
        /// </summary>
        public AuditInfo Audit { get; set; } = new AuditInfo();

        /// <summary>
        /// Category group for organizing settings (e.g., Branding, License, Security)
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Unique key within the category for identifying the setting
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// The current value of the setting (stored as string, converted based on DataType)
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Human-readable display name for the setting
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description of what this setting controls
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Data type for validation and proper conversion (String, Integer, Boolean, Decimal, DateTime, Json)
        /// </summary>
        public string DataType { get; set; } = "String";

        /// <summary>
        /// Whether this setting is required and cannot be empty
        /// </summary>
        public bool IsRequired { get; set; } = false;

        /// <summary>
        /// Whether this setting is read-only and cannot be modified through the UI
        /// </summary>
        public bool IsReadOnly { get; set; } = false;

        /// <summary>
        /// Default value for the setting (used when resetting or for new installations)
        /// </summary>
        public string? DefaultValue { get; set; }

        /// <summary>
        /// Regular expression pattern for validating the setting value
        /// </summary>
        public string? ValidationPattern { get; set; }

        /// <summary>
        /// Sort order for displaying settings within a category
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// Get the typed value of the setting based on its DataType
        /// </summary>
        /// <typeparam name="T">The type to convert the value to</typeparam>
        /// <returns>The converted value or default value for the type if conversion fails</returns>
        public T? GetTypedValue<T>()
        {
            if (string.IsNullOrEmpty(Value))
                return default(T);

            try
            {
                return (T?)Convert.ChangeType(Value, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// Get the value as a specific type with a fallback default
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="defaultValue">Default value if conversion fails</param>
        /// <returns>The converted value or the provided default</returns>
        public T GetTypedValue<T>(T defaultValue)
        {
            if (string.IsNullOrEmpty(Value))
                return defaultValue;

            try
            {
                var convertedValue = Convert.ChangeType(Value, typeof(T));
                return convertedValue != null ? (T)convertedValue : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Set the value from a typed object, converting it to string
        /// </summary>
        /// <param name="value">The value to set</param>
        public void SetTypedValue(object? value)
        {
            if (value == null)
            {
                Value = null;
                return;
            }

            // Handle boolean values to ensure consistent string representation
            if (value is bool boolValue)
            {
                Value = boolValue.ToString().ToLowerInvariant();
                return;
            }

            // Handle DateTime values with consistent formatting
            if (value is DateTime dateTimeValue)
            {
                Value = dateTimeValue.ToString("O"); // ISO 8601 format
                return;
            }

            Value = value.ToString();
        }

        /// <summary>
        /// Validate the current value against the validation pattern and data type
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool IsValidValue()
        {
            // Check required constraint
            if (IsRequired && string.IsNullOrWhiteSpace(Value))
                return false;

            // If value is empty and not required, it's valid
            if (string.IsNullOrWhiteSpace(Value))
                return true;

            // Validate against regex pattern if specified
            if (!string.IsNullOrEmpty(ValidationPattern))
            {
                try
                {
                    var regex = new System.Text.RegularExpressions.Regex(ValidationPattern);
                    if (!regex.IsMatch(Value))
                        return false;
                }
                catch
                {
                    // Invalid regex pattern, skip validation
                }
            }

            // Validate data type
            try
            {
                switch (DataType.ToLowerInvariant())
                {
                    case "integer":
                    case "int":
                        int.Parse(Value);
                        break;
                    case "decimal":
                    case "double":
                    case "float":
                        decimal.Parse(Value);
                        break;
                    case "boolean":
                    case "bool":
                        bool.Parse(Value);
                        break;
                    case "datetime":
                    case "date":
                        DateTime.Parse(Value);
                        break;
                    case "json":
                        System.Text.Json.JsonDocument.Parse(Value);
                        break;
                    case "string":
                    default:
                        // String values are always valid
                        break;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get the full setting key in Category.Key format
        /// </summary>
        public string FullKey => $"{Category}.{Key}";

        /// <summary>
        /// Check if this setting matches the specified category and key
        /// </summary>
        /// <param name="category">Category to match</param>
        /// <param name="key">Key to match</param>
        /// <returns>True if matches, false otherwise</returns>
        public bool Matches(string category, string key)
        {
            return string.Equals(Category, category, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Key, key, StringComparison.OrdinalIgnoreCase);
        }
    }
}
