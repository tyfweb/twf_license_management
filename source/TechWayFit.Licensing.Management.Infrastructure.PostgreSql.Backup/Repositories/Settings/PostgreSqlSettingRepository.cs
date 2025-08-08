using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Settings;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories;

using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.Settings;
using TechWayFit.Licensing.Management.Core.Models.Settings;
using TechWayFit.Licensing.Management.Core.Contracts;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.Settings;

/// <summary>
/// Repository implementation for managing system settings
/// </summary>
public class PostgreSqlSettingRepository : BaseRepository<Setting,SettingEntity>, ISettingRepository
{
    public PostgreSqlSettingRepository(PostgreSqlPostgreSqlLicensingDbContext context,IUserContext userContext) : base(context,userContext)
    {
    }

    /// <summary>
    /// Get a setting by category and key
    /// </summary>
    public async Task<Setting?> GetByKeyAsync(string category, string key)
    {
        var result = await _dbSet
                .FirstOrDefaultAsync(s => s.Category == category && s.Key == key && s.IsActive);
        return result?.Map();
    }

    /// <summary>
    /// Get all settings in a specific category
    /// </summary>
    public async Task<IEnumerable<Setting>> GetByCategoryAsync(string category)
    {
        var result = await _dbSet
            .Where(s => s.Category == category && s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.DisplayName)
            .ToListAsync();
        return result.Select(s => s.Map());
    }

    /// <summary>
    /// Get all settings grouped by category
    /// </summary>
    public async Task<Dictionary<string, IEnumerable<Setting>>> GetAllGroupedByCategoryAsync()
    {
        var settings = await _dbSet
            .Where(s => s.IsActive)
            .OrderBy(s => s.Category)
            .ThenBy(s => s.DisplayOrder)
            .ThenBy(s => s.DisplayName)
            .ToListAsync();

        var result = settings.GroupBy(s => s.Category)
                      .ToDictionary(g => g.Key, g => g.AsEnumerable());
        return result.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Select(s => s.Map()));
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
    public async Task<Setting> SetValueAsync(string category, string key, object? value, string updatedBy)
    {
        var setting = await GetByKeyAsync(category, key);
        var stringValue = ConvertValueToString(value);

        if (setting == null)
        {
            await AddAsync(new Setting
            {
                Category = category,
                Key = key,
                Value = stringValue,
                DataType = DetermineDataType(value),
                DisplayName = $"{category}.{key}"
            }); 
        }
        else
        {
            // Update existing setting
            await UpdateAsync(setting.SettingId,setting);
        }

        await _context.SaveChangesAsync();
        return setting?? new Setting
        {
            Category = category,
            Key = key,
            Value = stringValue,
            DataType = DetermineDataType(value),
            DisplayName = $"{category}.{key}", 
        };
    }

    /// <summary>
    public async Task<IEnumerable<Setting>> UpdateMultipleAsync(Dictionary<string, object?> settings, string updatedBy)
    {
        var updatedSettings = new List<Setting>();

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
    public async Task<Setting?> ResetToDefaultAsync(Guid settingId, string updatedBy)
    {
        var setting = await _dbSet.FirstOrDefaultAsync(s => s.Id == settingId && s.IsActive);
        if (setting == null) return null;

        setting.Value = setting.DefaultValue;
        setting.UpdatedBy = updatedBy;
        setting.UpdatedOn = DateTime.UtcNow;

        _dbSet.Update(setting);
        await _context.SaveChangesAsync();
        return setting.Map();
    }

    /// <summary>
    /// Reset all settings in a category to their default values
    /// </summary>
    public async Task<IEnumerable<Setting>> ResetCategoryToDefaultAsync(string category, string updatedBy)
    {
        var settings = await _dbSet
            .Where(s => s.Category == category && s.IsActive)
            .ToListAsync();

        foreach (var setting in settings)
        {
            setting.Value = setting.DefaultValue;
            setting.UpdatedBy = updatedBy;
            setting.UpdatedOn = DateTime.UtcNow;
        }

        _dbSet.UpdateRange(settings);
        await _context.SaveChangesAsync();
        return settings.Select(s => s.Map());
    }

    /// <summary>
    /// Search settings by display name or description
    /// </summary>
    public async Task<IEnumerable<Setting>> SearchAsync(string searchTerm)
    {
        var result = await _dbSet
            .Where(s => s.IsActive &&
                       (s.DisplayName.Contains(searchTerm) ||
                        s.Description != null && s.Description.Contains(searchTerm)))
            .OrderBy(s => s.Category)
            .ThenBy(s => s.DisplayOrder)
            .ToListAsync();
        return result.Select(s => s.Map());
    }

    /// <summary>
    /// Get all available categories
    /// </summary>
    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        var result = await _dbSet
            .Where(s => s.IsActive)
            .Select(s => s.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
        return result;
    }

    /// <summary>
    /// Check if a setting exists
    /// </summary>
    public async Task<bool> ExistsAsync(string category, string key)
    {
        var result = await _dbSet
            .AnyAsync(s => s.Category == category && s.Key == key && s.IsActive);
        return result;
    }

    /// <summary>
    /// Validate all settings and var result =any validation errors
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
                    errors[setting.Id.ToString()] = $"Setting '{setting.DisplayName}' is required but has no value.";
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
                errors[setting.Id.ToString()] = $"Validation error for '{setting.DisplayName}': {ex.Message}";
            }
        }

        return errors;
    }

    /// <summary>
    /// Get settings by environment
    /// </summary>
    public async Task<IEnumerable<Setting>> GetByEnvironmentAsync(string environment)
    {
        // Environment property no longer exists in SettingEntity
        // var result =all active settings as environment filtering is not supported
        var result = await _dbSet
            .Where(s => s.IsActive)
            .OrderBy(s => s.Category)
            .ThenBy(s => s.DisplayOrder)
            .ToListAsync();
        return result.Select(s => s.Map());
    }

    /// <summary>
    /// Get settings that require restart
    /// </summary>
    public async Task<IEnumerable<Setting>> GetRequiringRestartAsync()
    {
        // RequiresRestart property no longer exists in SettingEntity
        // Return empty list as restart requirements are not tracked
        var result = await Task.FromResult(new List<SettingEntity>());
        return result.Select(s => s.Map());
    }

    /// <summary>
    /// Get deprecated settings
    /// </summary>
    public async Task<IEnumerable<Setting>> GetDeprecatedAsync()
    {
        // IsDeprecated property no longer exists in SettingEntity
        // Return empty list as deprecation status is not tracked
        var result = await Task.FromResult(new List<SettingEntity>());
        return result.Select(s => s.Map());
    }

    /// <summary>
    /// Get settings by tags
    /// </summary>
    public async Task<IEnumerable<Setting>> GetByTagsAsync(params string[] tags)
    {
        // Tags property no longer exists in SettingEntity
        // Return empty list as tag-based filtering is not supported
        var result = await Task.FromResult(new List<SettingEntity>());
        return result.Select(s => s.Map());
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
