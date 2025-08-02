using TechWayFit.Licensing.Management.Core.Models.Settings;

namespace TechWayFit.Licensing.Management.Core.Contracts.Services;

/// <summary>
/// Service interface for managing system settings
/// </summary>
public interface ISettingService
{
    /// <summary>
    /// Get all settings grouped by category
    /// </summary>
    /// <returns>Dictionary with category as key and settings as value</returns>
    Task<Dictionary<string, IEnumerable<Setting>>> GetAllSettingsGroupedAsync();

    /// <summary>
    /// Get all settings for a specific category
    /// </summary>
    /// <param name="category">Category to filter by</param>
    /// <returns>Settings in the specified category</returns>
    Task<IEnumerable<Setting>> GetSettingsByCategoryAsync(string category);

    /// <summary>
    /// Get a specific setting by category and key
    /// </summary>
    /// <param name="category">Setting category</param>
    /// <param name="key">Setting key</param>
    /// <returns>The setting if found, null otherwise</returns>
    Task<Setting?> GetSettingAsync(string category, string key);

    /// <summary>
    /// Get a setting value as a specific type
    /// </summary>
    /// <typeparam name="T">Type to convert the value to</typeparam>
    /// <param name="category">Setting category</param>
    /// <param name="key">Setting key</param>
    /// <param name="defaultValue">Default value if setting not found or conversion fails</param>
    /// <returns>The typed value or default</returns>
    Task<T> GetSettingValueAsync<T>(string category, string key, T defaultValue);

    /// <summary>
    /// Update a setting value
    /// </summary>
    /// <param name="settingId">ID of the setting to update</param>
    /// <param name="value">New value</param>
    /// <param name="updatedBy">User making the update</param>
    /// <returns>Updated setting</returns>
    Task<Setting> UpdateSettingAsync(string settingId, object? value, string updatedBy);

    /// <summary>
    /// Update multiple settings in a single transaction
    /// </summary>
    /// <param name="settings">Dictionary of setting ID to new value</param>
    /// <param name="updatedBy">User making the updates</param>
    /// <returns>List of updated settings</returns>
    Task<IEnumerable<Setting>> UpdateMultipleSettingsAsync(Dictionary<string, object?> settings, string updatedBy);

    /// <summary>
    /// Reset a setting to its default value
    /// </summary>
    /// <param name="settingId">ID of the setting to reset</param>
    /// <param name="updatedBy">User performing the reset</param>
    /// <returns>Reset setting</returns>
    Task<Setting?> ResetSettingAsync(string settingId, string updatedBy);

    /// <summary>
    /// Reset all settings in a category to their default values
    /// </summary>
    /// <param name="category">Category to reset</param>
    /// <param name="updatedBy">User performing the reset</param>
    /// <returns>List of reset settings</returns>
    Task<IEnumerable<Setting>> ResetCategoryAsync(string category, string updatedBy);

    /// <summary>
    /// Search settings by display name or description
    /// </summary>
    /// <param name="searchTerm">Term to search for</param>
    /// <returns>Matching settings</returns>
    Task<IEnumerable<Setting>> SearchSettingsAsync(string searchTerm);

    /// <summary>
    /// Get all available categories
    /// </summary>
    /// <returns>List of distinct categories</returns>
    Task<IEnumerable<string>> GetCategoriesAsync();

    /// <summary>
    /// Check if a setting exists
    /// </summary>
    /// <param name="category">Setting category</param>
    /// <param name="key">Setting key</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> SettingExistsAsync(string category, string key);

    /// <summary>
    /// Validate all settings and return any validation errors
    /// </summary>
    /// <returns>Dictionary with setting ID as key and error message as value</returns>
    Task<Dictionary<string, string>> ValidateAllSettingsAsync();

    /// <summary>
    /// Get settings from appsettings.json that are read-only
    /// </summary>
    /// <returns>Dictionary of configuration settings</returns>
    Task<Dictionary<string, object>> GetConfigurationSettingsAsync();

    /// <summary>
    /// Get settings that require application restart
    /// </summary>
    /// <returns>Settings that require restart</returns>
    Task<IEnumerable<Setting>> GetRestartRequiredSettingsAsync();

    /// <summary>
    /// Initialize default settings if they don't exist
    /// </summary>
    /// <param name="createdBy">User creating the default settings</param>
    /// <returns>Number of settings created</returns>
    Task<int> InitializeDefaultSettingsAsync(string createdBy = "System");

    /// <summary>
    /// Backup current settings to a JSON string
    /// </summary>
    /// <returns>JSON string containing all settings</returns>
    Task<string> BackupSettingsAsync();

    /// <summary>
    /// Restore settings from a JSON backup
    /// </summary>
    /// <param name="backupJson">JSON string containing settings backup</param>
    /// <param name="updatedBy">User performing the restore</param>
    /// <returns>Number of settings restored</returns>
    Task<int> RestoreSettingsAsync(string backupJson, string updatedBy);

    /// <summary>
    /// Get settings by tags
    /// </summary>
    /// <param name="tags">Tags to search for</param>
    /// <returns>Settings matching any of the specified tags</returns>
    Task<IEnumerable<Setting>> GetSettingsByTagsAsync(params string[] tags);

    /// <summary>
    /// Get settings for a specific environment
    /// </summary>
    /// <param name="environment">Environment to filter by</param>
    /// <returns>Settings for the specified environment</returns>
    Task<IEnumerable<Setting>> GetSettingsByEnvironmentAsync(string environment);
}
