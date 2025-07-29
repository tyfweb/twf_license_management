using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TechWayFit.Licensing.Infrastructure.Contracts.Repositories.Settings;
using TechWayFit.Licensing.Infrastructure.Data.Context;
using TechWayFit.Licensing.Infrastructure.Data.Repositories;
using TechWayFit.Licensing.Infrastructure.Models.Entities.Settings;

namespace TechWayFit.Licensing.Infrastructure.Data.Repositories.Settings;

/// <summary>
/// Repository implementation for managing system settings
/// </summary>
public class SettingRepository : BaseRepository<SettingEntity>, ISettingRepository
{
    public SettingRepository(LicensingDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get a setting by category and key
    /// </summary>
    public async Task<SettingEntity?> GetByKeyAsync(string category, string key)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.Category == category && s.Key == key && s.IsActive);
    }

    /// <summary>
    /// Get all settings in a specific category
    /// </summary>
    public async Task<IEnumerable<SettingEntity>> GetByCategoryAsync(string category)
    {
        return await _dbSet
            .Where(s => s.Category == category && s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.DisplayName)
            .ToListAsync();
    }

    /// <summary>
    /// Get all settings grouped by category
    /// </summary>
    public async Task<Dictionary<string, IEnumerable<SettingEntity>>> GetAllGroupedByCategoryAsync()
    {
        var settings = await _dbSet
            .Where(s => s.IsActive)
            .OrderBy(s => s.Category)
            .ThenBy(s => s.DisplayOrder)
            .ThenBy(s => s.DisplayName)
            .ToListAsync();

        return settings.GroupBy(s => s.Category)
                      .ToDictionary(g => g.Key, g => g.AsEnumerable());
    }

    /// <summary>
    /// Get the value of a setting as a specific type
    /// </summary>
    public async Task<T> GetValueAsync<T>(string category, string key, T defaultValue)
    {
        try
        {
            var setting = await GetByKeyAsync(category, key);
            if (setting?.Value == null)
                return defaultValue;

            // Handle different data types
            return setting.DataType.ToLower() switch
            {
                "string" => (T)(object)setting.Value,
                "int" => (T)(object)int.Parse(setting.Value),
                "bool" => (T)(object)bool.Parse(setting.Value),
                "decimal" => (T)(object)decimal.Parse(setting.Value),
                "double" => (T)(object)double.Parse(setting.Value),
                "datetime" => (T)(object)DateTime.Parse(setting.Value),
                "json" => JsonSerializer.Deserialize<T>(setting.Value) ?? defaultValue,
                _ => (T)Convert.ChangeType(setting.Value, typeof(T))
            };
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Set the value of a setting, creating it if it doesn't exist
    /// </summary>
    public async Task<SettingEntity> SetValueAsync(string category, string key, object? value, string updatedBy)
    {
        var setting = await GetByKeyAsync(category, key);
        var stringValue = ConvertValueToString(value);

        if (setting == null)
        {
            // Create new setting
            setting = new SettingEntity
            {
                Category = category,
                Key = key,
                Value = stringValue,
                DataType = DetermineDataType(value),
                DisplayName = $"{category}.{key}",
                CreatedBy = updatedBy,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                ValueSource = "Database"
            };
            await _dbSet.AddAsync(setting);
        }
        else
        {
            // Update existing setting
            setting.Value = stringValue;
            setting.UpdatedBy = updatedBy;
            setting.UpdatedOn = DateTime.UtcNow;
            setting.ValueSource = "Database";
            _dbSet.Update(setting);
        }

        await _context.SaveChangesAsync();
        return setting;
    }

    /// <summary>
    /// Update multiple settings in a single transaction
    /// </summary>
    public async Task<IEnumerable<SettingEntity>> UpdateMultipleAsync(Dictionary<string, object?> settings, string updatedBy)
    {
        var updatedSettings = new List<SettingEntity>();

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var kvp in settings)
            {
                var parts = kvp.Key.Split('.');
                if (parts.Length != 2) continue;

                var category = parts[0];
                var key = parts[1];
                var setting = await SetValueAsync(category, key, kvp.Value, updatedBy);
                updatedSettings.Add(setting);
            }

            await transaction.CommitAsync();
            return updatedSettings;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Reset a setting to its default value
    /// </summary>
    public async Task<SettingEntity?> ResetToDefaultAsync(string settingId, string updatedBy)
    {
        var setting = await _dbSet.FirstOrDefaultAsync(s => s.SettingId == settingId && s.IsActive);
        if (setting == null) return null;

        setting.Value = setting.DefaultValue;
        setting.UpdatedBy = updatedBy;
        setting.UpdatedOn = DateTime.UtcNow;
        setting.ValueSource = "Default";

        _dbSet.Update(setting);
        await _context.SaveChangesAsync();
        return setting;
    }

    /// <summary>
    /// Reset all settings in a category to their default values
    /// </summary>
    public async Task<IEnumerable<SettingEntity>> ResetCategoryToDefaultAsync(string category, string updatedBy)
    {
        var settings = await _dbSet
            .Where(s => s.Category == category && s.IsActive)
            .ToListAsync();

        foreach (var setting in settings)
        {
            setting.Value = setting.DefaultValue;
            setting.UpdatedBy = updatedBy;
            setting.UpdatedOn = DateTime.UtcNow;
            setting.ValueSource = "Default";
        }

        _dbSet.UpdateRange(settings);
        await _context.SaveChangesAsync();
        return settings;
    }

    /// <summary>
    /// Search settings by display name or description
    /// </summary>
    public async Task<IEnumerable<SettingEntity>> SearchAsync(string searchTerm)
    {
        return await _dbSet
            .Where(s => s.IsActive && 
                       (s.DisplayName.Contains(searchTerm) || 
                        s.Description != null && s.Description.Contains(searchTerm) ||
                        s.Tags != null && s.Tags.Contains(searchTerm)))
            .OrderBy(s => s.Category)
            .ThenBy(s => s.DisplayOrder)
            .ToListAsync();
    }

    /// <summary>
    /// Get all available categories
    /// </summary>
    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        return await _dbSet
            .Where(s => s.IsActive)
            .Select(s => s.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    /// <summary>
    /// Check if a setting exists
    /// </summary>
    public async Task<bool> ExistsAsync(string category, string key)
    {
        return await _dbSet
            .AnyAsync(s => s.Category == category && s.Key == key && s.IsActive);
    }

    /// <summary>
    /// Validate all settings and return any validation errors
    /// </summary>
    public async Task<Dictionary<string, string>> ValidateAllAsync()
    {
        var errors = new Dictionary<string, string>();
        var settings = await _dbSet.Where(s => s.IsActive).ToListAsync();

        foreach (var setting in settings)
        {
            try
            {
                // Check required settings
                if (setting.IsRequired && string.IsNullOrEmpty(setting.Value))
                {
                    errors[setting.SettingId] = $"Setting '{setting.DisplayName}' is required but has no value.";
                    continue;
                }

                // Validate data type
                if (!string.IsNullOrEmpty(setting.Value))
                {
                    ValidateDataType(setting.Value, setting.DataType);
                }

                // TODO: Add more validation rules based on ValidationRules JSON
            }
            catch (Exception ex)
            {
                errors[setting.SettingId] = $"Validation error for '{setting.DisplayName}': {ex.Message}";
            }
        }

        return errors;
    }

    /// <summary>
    /// Get settings by environment
    /// </summary>
    public async Task<IEnumerable<SettingEntity>> GetByEnvironmentAsync(string environment)
    {
        return await _dbSet
            .Where(s => s.IsActive && (s.Environment == environment || s.Environment == "All"))
            .OrderBy(s => s.Category)
            .ThenBy(s => s.DisplayOrder)
            .ToListAsync();
    }

    /// <summary>
    /// Get settings that require restart
    /// </summary>
    public async Task<IEnumerable<SettingEntity>> GetRequiringRestartAsync()
    {
        return await _dbSet
            .Where(s => s.IsActive && s.RequiresRestart)
            .OrderBy(s => s.Category)
            .ThenBy(s => s.DisplayOrder)
            .ToListAsync();
    }

    /// <summary>
    /// Get deprecated settings
    /// </summary>
    public async Task<IEnumerable<SettingEntity>> GetDeprecatedAsync()
    {
        return await _dbSet
            .Where(s => s.IsActive && s.IsDeprecated)
            .OrderBy(s => s.Category)
            .ThenBy(s => s.DisplayOrder)
            .ToListAsync();
    }

    /// <summary>
    /// Get settings by tags
    /// </summary>
    public async Task<IEnumerable<SettingEntity>> GetByTagsAsync(params string[] tags)
    {
        if (tags == null || tags.Length == 0)
            return new List<SettingEntity>();

        return await _dbSet
            .Where(s => s.IsActive && s.Tags != null && tags.Any(tag => s.Tags.Contains(tag)))
            .OrderBy(s => s.Category)
            .ThenBy(s => s.DisplayOrder)
            .ToListAsync();
    }

    #region Private Helper Methods

    private string? ConvertValueToString(object? value)
    {
        if (value == null) return null;
        
        return value switch
        {
            string str => str,
            bool b => b.ToString().ToLower(),
            DateTime dt => dt.ToString("O"), // ISO 8601 format
            _ when value.GetType().IsClass && !(value is string) => JsonSerializer.Serialize(value),
            _ => value.ToString()
        };
    }

    private string DetermineDataType(object? value)
    {
        if (value == null) return "string";
        
        return value switch
        {
            string => "string",
            bool => "bool",
            int => "int",
            decimal => "decimal",
            double => "double",
            DateTime => "datetime",
            _ when value.GetType().IsClass => "json",
            _ => "string"
        };
    }

    private void ValidateDataType(string value, string dataType)
    {
        switch (dataType.ToLower())
        {
            case "int":
                int.Parse(value);
                break;
            case "bool":
                bool.Parse(value);
                break;
            case "decimal":
                decimal.Parse(value);
                break;
            case "double":
                double.Parse(value);
                break;
            case "datetime":
                DateTime.Parse(value);
                break;
            case "json":
                JsonSerializer.Deserialize<object>(value);
                break;
            // string doesn't need validation
        }
    }

    #endregion
}
