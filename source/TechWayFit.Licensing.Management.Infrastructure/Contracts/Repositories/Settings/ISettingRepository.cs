using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Settings;

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Settings;

/// <summary>
/// Repository interface for managing system settings
/// </summary>
public interface ISettingRepository : IBaseRepository<SettingEntity>
{
    /// <summary>
    /// Get a setting by category and key
    /// </summary>
    /// <param name="category">Setting category</param>
    /// <param name="key">Setting key</param>
    /// <returns>The setting if found, null otherwise</returns>
    Task<SettingEntity?> GetByKeyAsync(string category, string key);

    /// <summary>
    /// Get all settings in a specific category
    /// </summary>
    /// <param name="category">Category to filter by</param>
    /// <returns>List of settings in the category</returns>
    Task<IEnumerable<SettingEntity>> GetByCategoryAsync(string category);

    /// <summary>
    /// Get all settings grouped by category
    /// </summary>
    /// <returns>Dictionary with category as key and settings as value</returns>
    Task<Dictionary<string, IEnumerable<SettingEntity>>> GetAllGroupedByCategoryAsync();

    /// <summary>
    /// Get the value of a setting as a specific type
    /// </summary>
    /// <typeparam name="T">Type to convert the value to</typeparam>
    /// <param name="category">Setting category</param>
    /// <param name="key">Setting key</param>
    /// <param name="defaultValue">Default value if setting not found or conversion fails</param>
    /// <returns>The typed value or default</returns>
    Task<T> GetValueAsync<T>(string category, string key, T defaultValue);

    /// <summary>
    /// Set the value of a setting, creating it if it doesn't exist
    /// </summary>
    /// <param name="category">Setting category</param>
    /// <param name="key">Setting key</param>
    /// <param name="value">Value to set</param>
    /// <param name="updatedBy">User making the update</param>
    /// <returns>The updated or created setting</returns>
    Task<SettingEntity> SetValueAsync(string category, string key, object? value, string updatedBy);

    /// <summary>
    /// Update multiple settings in a single transaction
    /// </summary>
    /// <param name="settings">Dictionary of settings with FullKey as key and value as value</param>
    /// <param name="updatedBy">User making the updates</param>
    /// <returns>List of updated settings</returns>
    Task<IEnumerable<SettingEntity>> UpdateMultipleAsync(Dictionary<string, object?> settings, string updatedBy);

    /// <summary>
    /// Reset a setting to its default value
    /// </summary>
    /// <param name="settingId">ID of the setting to reset</param>
    /// <param name="updatedBy">User performing the reset</param>
    /// <returns>The reset setting</returns>
    Task<SettingEntity?> ResetToDefaultAsync(Guid settingId, string updatedBy);

    /// <summary>
    /// Reset all settings in a category to their default values
    /// </summary>
    /// <param name="category">Category to reset</param>
    /// <param name="updatedBy">User performing the reset</param>
    /// <returns>List of reset settings</returns>
    Task<IEnumerable<SettingEntity>> ResetCategoryToDefaultAsync(string category, string updatedBy);

    /// <summary>
    /// Search settings by display name or description
    /// </summary>
    /// <param name="searchTerm">Term to search for</param>
    /// <returns>Matching settings</returns>
    Task<IEnumerable<SettingEntity>> SearchAsync(string searchTerm);

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
    Task<bool> ExistsAsync(string category, string key);

    /// <summary>
    /// Validate all settings and return any validation errors
    /// </summary>
    /// <returns>Dictionary with setting ID as key and error message as value</returns>
    Task<Dictionary<string, string>> ValidateAllAsync();

    /// <summary>
    /// Get settings by environment
    /// </summary>
    /// <param name="environment">Environment to filter by</param>
    /// <returns>Settings for the specified environment</returns>
    Task<IEnumerable<SettingEntity>> GetByEnvironmentAsync(string environment);

    /// <summary>
    /// Get settings that require restart
    /// </summary>
    /// <returns>Settings that require application restart</returns>
    Task<IEnumerable<SettingEntity>> GetRequiringRestartAsync();

    /// <summary>
    /// Get deprecated settings
    /// </summary>
    /// <returns>Deprecated settings</returns>
    Task<IEnumerable<SettingEntity>> GetDeprecatedAsync();

    /// <summary>
    /// Get settings by tags
    /// </summary>
    /// <param name="tags">Tags to search for</param>
    /// <returns>Settings matching any of the specified tags</returns>
    Task<IEnumerable<SettingEntity>> GetByTagsAsync(params string[] tags);
}
