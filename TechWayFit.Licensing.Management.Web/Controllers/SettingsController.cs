using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Web.ViewModels.Settings;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace TechWayFit.Licensing.Management.Web.Controllers
{
    /// <summary>
    /// Controller for managing system settings
    /// </summary>
    [Authorize(Roles = "Administrator")]

    public class SettingsController : Controller
    {
        private readonly ISettingService _settingService;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(
            ISettingService settingService,
            ILogger<SettingsController> logger)
        {
            _settingService = settingService;
            _logger = logger;
        }

        /// <summary>
        /// Display all settings grouped by category
        /// </summary>
        /// <returns>Settings index view</returns>
        public async Task<IActionResult> Index()
        {
            try
            {
                var settingsGrouped = await _settingService.GetAllSettingsGroupedAsync();
                var configurationSettings = await _settingService.GetConfigurationSettingsAsync();

                var viewModel = new SettingsIndexViewModel
                {
                    SettingsGrouped = settingsGrouped.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Select(s => new SettingViewModel
                        {
                            SettingId = s.SettingId,
                            Category = s.Category,
                            Key = s.Key,
                            Value = s.Value,
                            DefaultValue = s.DefaultValue,
                            DisplayName = s.DisplayName,
                            Description = s.Description,
                            DataType = s.DataType,
                            IsRequired = s.IsRequired,
                            IsReadOnly = s.IsReadOnly,
                            ValidationPattern = s.ValidationPattern,
                            SortOrder = s.SortOrder,
                            CreatedAt = s.CreatedAt,
                            UpdatedAt = s.UpdatedAt,
                            CreatedBy = s.CreatedBy,
                            UpdatedBy = s.UpdatedBy
                        }).OrderBy(s => s.SortOrder).ToList()
                    ),
                    ConfigurationSettings = configurationSettings,
                    Categories = settingsGrouped.Keys.OrderBy(k => k).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading settings");
                TempData["ErrorMessage"] = "Error loading settings. Please try again.";
                return View(new SettingsIndexViewModel());
            }
        }

        /// <summary>
        /// Get settings for a specific category (AJAX)
        /// </summary>
        /// <param name="category">Category to filter by</param>
        /// <returns>JSON result with settings</returns>
        [HttpGet]
        public async Task<IActionResult> GetByCategory(string category)
        {
            try
            {
                var settings = await _settingService.GetSettingsByCategoryAsync(category);
                var viewModels = settings.Select(s => new SettingViewModel
                {
                    SettingId = s.SettingId,
                    Category = s.Category,
                    Key = s.Key,
                    Value = s.Value,
                    DefaultValue = s.DefaultValue,
                    DisplayName = s.DisplayName,
                    Description = s.Description,
                    DataType = s.DataType,
                    IsRequired = s.IsRequired,
                    IsReadOnly = s.IsReadOnly,
                    ValidationPattern = s.ValidationPattern,
                    SortOrder = s.SortOrder,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    CreatedBy = s.CreatedBy,
                    UpdatedBy = s.UpdatedBy
                }).OrderBy(s => s.SortOrder).ToList();

                return Json(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving settings for category {Category}", category);
                return Json(new { error = "Error retrieving settings" });
            }
        }

        /// <summary>
        /// Update a single setting value
        /// </summary>
        /// <param name="model">Update model</param>
        /// <returns>JSON result</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSetting([FromBody] UpdateSettingViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid input data" });
                }

                var currentUser = GetCurrentUser();
                var updatedSetting = await _settingService.UpdateSettingAsync(model.SettingId, model.Value, currentUser);

                return Json(new
                {
                    success = true,
                    message = "Setting updated successfully",
                    setting = new SettingViewModel
                    {
                        SettingId = updatedSetting.SettingId,
                        Category = updatedSetting.Category,
                        Key = updatedSetting.Key,
                        Value = updatedSetting.Value,
                        DefaultValue = updatedSetting.DefaultValue,
                        DisplayName = updatedSetting.DisplayName,
                        Description = updatedSetting.Description,
                        DataType = updatedSetting.DataType,
                        IsRequired = updatedSetting.IsRequired,
                        IsReadOnly = updatedSetting.IsReadOnly,
                        ValidationPattern = updatedSetting.ValidationPattern,
                        SortOrder = updatedSetting.SortOrder,
                        CreatedAt = updatedSetting.CreatedAt,
                        UpdatedAt = updatedSetting.UpdatedAt,
                        CreatedBy = updatedSetting.CreatedBy,
                        UpdatedBy = updatedSetting.UpdatedBy
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating setting {SettingId}", model.SettingId);
                return Json(new { success = false, message = "Error updating setting. Please try again." });
            }
        }

        /// <summary>
        /// Update multiple settings in a batch
        /// </summary>
        /// <param name="models">List of update models</param>
        /// <returns>JSON result</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMultipleSettings([FromBody] List<UpdateSettingViewModel> models)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid input data" });
                }

                var currentUser = GetCurrentUser();
                var settingsToUpdate = models.ToDictionary(m => m.SettingId, m => m.Value);
                var updatedSettings = await _settingService.UpdateMultipleSettingsAsync(settingsToUpdate, currentUser);

                return Json(new
                {
                    success = true,
                    message = $"Successfully updated {updatedSettings.Count()} settings",
                    settings = updatedSettings.Select(s => new SettingViewModel
                    {
                        SettingId = s.SettingId,
                        Category = s.Category,
                        Key = s.Key,
                        Value = s.Value,
                        DefaultValue = s.DefaultValue,
                        DisplayName = s.DisplayName,
                        Description = s.Description,
                        DataType = s.DataType,
                        IsRequired = s.IsRequired,
                        IsReadOnly = s.IsReadOnly,
                        ValidationPattern = s.ValidationPattern,
                        SortOrder = s.SortOrder,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt,
                        CreatedBy = s.CreatedBy,
                        UpdatedBy = s.UpdatedBy
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating multiple settings");
                return Json(new { success = false, message = "Error updating settings. Please try again." });
            }
        }

        /// <summary>
        /// Reset a setting to its default value
        /// </summary>
        /// <param name="settingId">Setting ID to reset</param>
        /// <returns>JSON result</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetSetting(string settingId)
        {
            try
            {
                var currentUser = GetCurrentUser();
                var resetSetting = await _settingService.ResetSettingAsync(settingId, currentUser);

                if (resetSetting == null)
                {
                    return Json(new { success = false, message = "Setting not found" });
                }

                return Json(new
                {
                    success = true,
                    message = "Setting reset to default value",
                    setting = new SettingViewModel
                    {
                        SettingId = resetSetting.SettingId,
                        Category = resetSetting.Category,
                        Key = resetSetting.Key,
                        Value = resetSetting.Value,
                        DefaultValue = resetSetting.DefaultValue,
                        DisplayName = resetSetting.DisplayName,
                        Description = resetSetting.Description,
                        DataType = resetSetting.DataType,
                        IsRequired = resetSetting.IsRequired,
                        IsReadOnly = resetSetting.IsReadOnly,
                        ValidationPattern = resetSetting.ValidationPattern,
                        SortOrder = resetSetting.SortOrder,
                        CreatedAt = resetSetting.CreatedAt,
                        UpdatedAt = resetSetting.UpdatedAt,
                        CreatedBy = resetSetting.CreatedBy,
                        UpdatedBy = resetSetting.UpdatedBy
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting setting {SettingId}", settingId);
                return Json(new { success = false, message = "Error resetting setting. Please try again." });
            }
        }

        /// <summary>
        /// Reset all settings in a category to default values
        /// </summary>
        /// <param name="category">Category to reset</param>
        /// <returns>JSON result</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetCategory(string category)
        {
            try
            {
                var currentUser = GetCurrentUser();
                var resetSettings = await _settingService.ResetCategoryAsync(category, currentUser);

                return Json(new
                {
                    success = true,
                    message = $"Reset {resetSettings.Count()} settings in category '{category}' to default values",
                    settings = resetSettings.Select(s => new SettingViewModel
                    {
                        SettingId = s.SettingId,
                        Category = s.Category,
                        Key = s.Key,
                        Value = s.Value,
                        DefaultValue = s.DefaultValue,
                        DisplayName = s.DisplayName,
                        Description = s.Description,
                        DataType = s.DataType,
                        IsRequired = s.IsRequired,
                        IsReadOnly = s.IsReadOnly,
                        ValidationPattern = s.ValidationPattern,
                        SortOrder = s.SortOrder,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt,
                        CreatedBy = s.CreatedBy,
                        UpdatedBy = s.UpdatedBy
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting category {Category}", category);
                return Json(new { success = false, message = "Error resetting category. Please try again." });
            }
        }

        /// <summary>
        /// Search settings
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <returns>JSON result with matching settings</returns>
        [HttpGet]
        public async Task<IActionResult> Search(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return Json(new List<SettingViewModel>());
                }

                var settings = await _settingService.SearchSettingsAsync(searchTerm);
                var viewModels = settings.Select(s => new SettingViewModel
                {
                    SettingId = s.SettingId,
                    Category = s.Category,
                    Key = s.Key,
                    Value = s.Value,
                    DefaultValue = s.DefaultValue,
                    DisplayName = s.DisplayName,
                    Description = s.Description,
                    DataType = s.DataType,
                    IsRequired = s.IsRequired,
                    IsReadOnly = s.IsReadOnly,
                    ValidationPattern = s.ValidationPattern,
                    SortOrder = s.SortOrder,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    CreatedBy = s.CreatedBy,
                    UpdatedBy = s.UpdatedBy
                }).OrderBy(s => s.Category).ThenBy(s => s.SortOrder).ToList();

                return Json(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching settings with term '{SearchTerm}'", searchTerm);
                return Json(new { error = "Error searching settings" });
            }
        }

        /// <summary>
        /// Backup settings to JSON
        /// </summary>
        /// <returns>File download result</returns>
        [HttpGet]
        public async Task<IActionResult> Backup()
        {
            try
            {
                var backupJson = await _settingService.BackupSettingsAsync();
                var fileName = $"settings_backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
                var fileBytes = System.Text.Encoding.UTF8.GetBytes(backupJson);

                return File(fileBytes, "application/json", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating settings backup");
                TempData["ErrorMessage"] = "Error creating backup. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Validate all settings
        /// </summary>
        /// <returns>JSON result with validation results</returns>
        [HttpGet]
        public async Task<IActionResult> ValidateAll()
        {
            try
            {
                var validationResults = await _settingService.ValidateAllSettingsAsync();
                var hasErrors = validationResults.Any();

                return Json(new
                {
                    success = !hasErrors,
                    message = hasErrors ? $"Found {validationResults.Count} validation errors" : "All settings are valid",
                    errors = validationResults
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating settings");
                return Json(new { success = false, message = "Error validating settings" });
            }
        }

        /// <summary>
        /// Get the current user identifier
        /// </summary>
        /// <returns>User identifier or "System"</returns>
        private string GetCurrentUser()
        {
            return User?.Identity?.Name ?? User?.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        }
    }
}
