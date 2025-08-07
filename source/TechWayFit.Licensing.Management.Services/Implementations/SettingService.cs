using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Settings;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.Settings;

namespace TechWayFit.Licensing.Management.Services.Implementations
{
    /// <summary>
    /// Service implementation for managing application settings with caching
    /// </summary>
    public class SettingService : ISettingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SettingService> _logger;
        private readonly IMemoryCache _cache;
        
        // Cache configuration
        private const string CACHE_KEY_ALL_SETTINGS = "settings_all";
        private const string CACHE_KEY_PREFIX = "setting_";
        private readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(30);

        public SettingService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<SettingService> logger,
            IMemoryCache cache)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _configuration = configuration;
            _logger = logger;
            _cache = cache;
        }

        public async Task<Dictionary<string, IEnumerable<Setting>>> GetAllSettingsGroupedAsync()
        {
            try
            {
                // Try to get from cache first
                if (_cache.TryGetValue(CACHE_KEY_ALL_SETTINGS, out Dictionary<string, IEnumerable<Setting>>? cachedSettings))
                {
                    _logger.LogDebug("Retrieved all settings from cache");
                    return cachedSettings!;
                }

                // Not in cache, fetch from database
                var entities = await _unitOfWork.Settings.GetAllGroupedByCategoryAsync();
                var settings = entities.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Select(e => e.ToModel())
                );

                // Cache the result
                _cache.Set(CACHE_KEY_ALL_SETTINGS, settings, CACHE_EXPIRATION);
                _logger.LogDebug("Cached all settings for {CacheExpiration} minutes", CACHE_EXPIRATION.TotalMinutes);

                return settings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving grouped settings");
                throw;
            }
        }

        public async Task<IEnumerable<Setting>> GetSettingsByCategoryAsync(string category)
        {
            try
            {
                var entities = await _unitOfWork.Settings.GetByCategoryAsync(category);
                return entities.Select(e => e.ToModel()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving settings for category {Category}", category);
                throw;
            }
        }

        public async Task<Setting?> GetSettingAsync(string category, string key)
        {
            try
            {
                var entity = await _unitOfWork.Settings.GetByKeyAsync(category, key);
                return entity?.ToModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving setting {Category}.{Key}", category, key);
                throw;
            }
        }

        public async Task<T> GetSettingValueAsync<T>(string category, string key, T defaultValue)
        {
            try
            {
                var value = await _unitOfWork.Settings.GetValueAsync<T>(category, key, defaultValue);
                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving setting value {Category}.{Key}", category, key);
                return defaultValue;
            }
        }

        public async Task<Setting> UpdateSettingAsync(Guid settingId, object? value, string updatedBy)
        {
            try
            {
                var entity = await _unitOfWork.Settings.GetByIdAsync(settingId);
                if (entity == null)
                    throw new ArgumentException($"Setting with ID {settingId} not found");

                // Use the SetValueAsync method which handles the update
                var updatedEntity = await _unitOfWork.Settings.SetValueAsync(entity.Category, entity.Key, value, updatedBy);
                await _unitOfWork.SaveChangesAsync();
                
                // Invalidate cache
                InvalidateCache();
                
                return updatedEntity.ToModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating setting {SettingId}", settingId);
                throw;
            }
        }

        public async Task<IEnumerable<Setting>> UpdateMultipleSettingsAsync(Dictionary<string, object?> settings, string updatedBy)
        {
            try
            {
                var entities = await _unitOfWork.Settings.UpdateMultipleAsync(settings, updatedBy);
                await _unitOfWork.SaveChangesAsync();
                
                // Invalidate cache
                InvalidateCache();
                
                return entities.Select(e => e.ToModel()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating settings");
                throw;
            }
        }

        public async Task<Setting?> ResetSettingAsync(Guid settingId, string updatedBy)
        {
            try
            {
                var entity = await _unitOfWork.Settings.ResetToDefaultAsync(settingId, updatedBy);
                
                // Invalidate cache if reset was successful
                if (entity != null)
                {
                    InvalidateCache();
                }
                
                return entity?.ToModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting setting {SettingId}", settingId);
                throw;
            }
        }

        public async Task<IEnumerable<Setting>> ResetCategoryAsync(string category, string updatedBy)
        {
            try
            {
                var entities = await _unitOfWork.Settings.ResetCategoryToDefaultAsync(category, updatedBy);
                
                // Invalidate cache
                InvalidateCache();
                
                return entities.Select(e => e.ToModel()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting category {Category}", category);
                throw;
            }
        }

        public async Task<IEnumerable<Setting>> SearchSettingsAsync(string searchTerm)
        {
            try
            {
                var entities = await _unitOfWork.Settings.SearchAsync(searchTerm);
                return entities.Select(e => e.ToModel()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching settings with term '{SearchTerm}'", searchTerm);
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            try
            {
                return await _unitOfWork.Settings.GetCategoriesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available categories");
                throw;
            }
        }

        public async Task<bool> SettingExistsAsync(string category, string key)
        {
            try
            {
                return await _unitOfWork.Settings.ExistsAsync(category, key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if setting exists {Category}.{Key}", category, key);
                return false;
            }
        }

        public async Task<Dictionary<string, string>> ValidateAllSettingsAsync()
        {
            try
            {
                return await _unitOfWork.Settings.ValidateAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating all settings");
                throw;
            }
        }

        public async Task<Dictionary<string, object>> GetConfigurationSettingsAsync()
        {
            try
            {
                await Task.CompletedTask; // Make method properly async
                var configurationSettings = new Dictionary<string, object>();

                // Get common configuration sections that should be displayed (read-only)
                var sections = new[]
                {
                    "ConnectionStrings",
                    "Logging",
                    "AllowedHosts",
                    "License",
                    "Email",
                    "Authentication",
                    "Security"
                };

                foreach (var section in sections)
                {
                    var configSection = _configuration.GetSection(section);
                    if (configSection.Exists())
                    {
                        var sectionData = GetConfigurationSectionData(configSection);
                        if (sectionData.Any())
                        {
                            configurationSettings[section] = sectionData;
                        }
                    }
                }

                return configurationSettings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving configuration settings");
                throw;
            }
        }

        public async Task<IEnumerable<Setting>> GetRestartRequiredSettingsAsync()
        {
            try
            {
                var entities = await _unitOfWork.Settings.GetRequiringRestartAsync();
                return entities.Select(e => e.ToModel()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving restart required settings");
                throw;
            }
        }

        public async Task<int> InitializeDefaultSettingsAsync(string createdBy = "System")
        {
            try
            {
                // This would typically read from a configuration file or embedded resource
                // For now, return 0 as we're assuming settings are initialized via database scripts
                await Task.CompletedTask;
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing default settings");
                throw;
            }
        }

        public async Task<string> BackupSettingsAsync()
        {
            try
            {
                var entities = await _unitOfWork.Settings.GetAllAsync(CancellationToken.None);
                var settings = entities.Select(e => e.ToModel()).ToList();
                return JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error backing up settings");
                throw;
            }
        }

        public async Task<int> RestoreSettingsAsync(string backupJson, string updatedBy)
        {
            try
            {
                var settings = JsonSerializer.Deserialize<List<Setting>>(backupJson);
                if (settings == null)
                    return 0;

                var restoredCount = 0;

                foreach (var setting in settings)
                {
                    setting.UpdatedAt = DateTime.UtcNow;
                    setting.UpdatedBy = updatedBy;

                    var entity = SettingEntity.FromModel(setting);
                    var existing = await _unitOfWork.Settings.GetByIdAsync(setting.SettingId);

                    if (existing != null)
                    {
                        await _unitOfWork.Settings.UpdateAsync(entity);
                    }
                    else
                    {
                        await _unitOfWork.Settings.AddAsync(entity);
                    }

                    restoredCount++;
                }

                return restoredCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring settings from backup");
                throw;
            }
        }

        public async Task<IEnumerable<Setting>> GetSettingsByTagsAsync(params string[] tags)
        {
            try
            {
                var entities = await _unitOfWork.Settings.GetByTagsAsync(tags);
                return entities.Select(e => e.ToModel()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving settings by tags");
                throw;
            }
        }

        public async Task<IEnumerable<Setting>> GetSettingsByEnvironmentAsync(string environment)
        {
            try
            {
                var entities = await _unitOfWork.Settings.GetByEnvironmentAsync(environment);
                return entities.Select(e => e.ToModel()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving settings for environment {Environment}", environment);
                throw;
            }
        }

        private Dictionary<string, object?> GetConfigurationSectionData(IConfigurationSection section)
        {
            var data = new Dictionary<string, object?>();

            if (section.Value != null)
            {
                return new Dictionary<string, object?> { [section.Key] = section.Value };
            }

            foreach (var child in section.GetChildren())
            {
                if (child.Value != null)
                {
                    data[child.Key] = child.Value;
                }
                else
                {
                    var childData = GetConfigurationSectionData(child);
                    if (childData.Any())
                    {
                        data[child.Key] = childData;
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Invalidates all cached settings
        /// </summary>
        private void InvalidateCache()
        {
            _cache.Remove(CACHE_KEY_ALL_SETTINGS);
            _logger.LogDebug("Settings cache invalidated");
        }
    }
}
