using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Web.ViewModels.Settings;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TechWayFit.Licensing.Management.Web.Helpers;

namespace TechWayFit.Licensing.Management.Web.Controllers
{
    /// <summary>
    /// Controller for managing system settings
    /// </summary>
    [Authorize(Roles = "Administrator")]

    public class SettingsController : BaseController
    {
        private readonly ISettingService _settingService;
        private readonly ILogger<SettingsController> _logger;
        private readonly IWebHostEnvironment _hostEnvironment;

        public SettingsController(
            ISettingService settingService,
            ILogger<SettingsController> logger,
            IWebHostEnvironment hostEnvironment)
        {
            _settingService = settingService;
            _logger = logger;
            _hostEnvironment = hostEnvironment;
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
                            SettingId = s.SettingId.ConvertToString(),
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
                            CreatedAt = s.Audit.CreatedOn,
                            UpdatedAt = s.Audit.UpdatedOn ?? s.Audit.CreatedOn,
                            CreatedBy = s.Audit.CreatedBy,
                            UpdatedBy = s.Audit.UpdatedBy ?? string.Empty
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
                    SettingId = s.SettingId.ConvertToString(),
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
                    CreatedAt = s.Audit.CreatedOn,
                    UpdatedAt = s.Audit.UpdatedOn ?? s.Audit.CreatedOn,
                    CreatedBy = s.Audit.CreatedBy,
                    UpdatedBy = s.Audit.UpdatedBy ?? string.Empty
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
                var updatedSetting = await _settingService.UpdateSettingAsync(model.SettingId.ToGuid(), model.Value, currentUser);

                return Json(new
                {
                    success = true,
                    message = "Setting updated successfully",
                    setting = new SettingViewModel
                    {
                        SettingId = updatedSetting.SettingId.ConvertToString(),
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
                        CreatedAt = updatedSetting.Audit.CreatedOn,
                        UpdatedAt = updatedSetting.Audit.UpdatedOn ?? updatedSetting.Audit.CreatedOn,
                        CreatedBy = updatedSetting.Audit.CreatedBy,
                        UpdatedBy = updatedSetting.Audit.UpdatedBy ?? string.Empty
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
                var settingsToUpdate = models.ToDictionary(m => m.SettingId.ToString(), m => m.Value);
                var updatedSettings = await _settingService.UpdateMultipleSettingsAsync(settingsToUpdate, currentUser);

                return Json(new
                {
                    success = true,
                    message = $"Successfully updated {updatedSettings.Count()} settings",
                    settings = updatedSettings.Select(s => new SettingViewModel
                    {
                        SettingId = s.SettingId.ConvertToString(),
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
                        CreatedAt = s.Audit.CreatedOn,
                        UpdatedAt = s.Audit.UpdatedOn ?? s.Audit.CreatedOn,
                        CreatedBy = s.Audit.CreatedBy,
                        UpdatedBy = s.Audit.UpdatedBy ?? string.Empty
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
        /// Get the default value for a setting
        /// </summary>
        /// <param name="settingId">Setting ID</param>
        /// <returns>JSON result with default value</returns>
        [HttpGet]
        public async Task<IActionResult> GetDefaultValue(string settingId)
        {
            try
            {
                // Use ResetSettingAsync to get the setting info including default value
                // but don't actually save it by passing a fake user and then discarding the result
                var allSettings = await _settingService.GetAllSettingsGroupedAsync();
                var setting = allSettings
                    .SelectMany(kvp => kvp.Value)
                    .FirstOrDefault(s => s.SettingId.ToString() == settingId);

                if (setting == null)
                {
                    return Json(new { success = false, message = "Setting not found" });
                }

                return Json(new { 
                    success = true, 
                    defaultValue = setting.DefaultValue ?? string.Empty 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving default value for setting {SettingId}", settingId);
                return Json(new { success = false, message = "Error retrieving default value" });
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
                var resetSetting = await _settingService.ResetSettingAsync(Guid.Parse(settingId), currentUser);

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
                        SettingId = resetSetting.SettingId.ConvertToString(),
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
                        CreatedAt = resetSetting.Audit.CreatedOn,
                        UpdatedAt = resetSetting.Audit.UpdatedOn ?? resetSetting.Audit.CreatedOn,
                        CreatedBy = resetSetting.Audit.CreatedBy,
                        UpdatedBy = resetSetting.Audit.UpdatedBy ?? string.Empty
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
                        SettingId = s.SettingId.ConvertToString(),
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
                        CreatedAt = s.Audit.CreatedOn,
                        UpdatedAt = s.Audit.UpdatedOn ?? s.Audit.CreatedOn,
                        CreatedBy = s.Audit.CreatedBy,
                        UpdatedBy = s.Audit.UpdatedBy ?? string.Empty
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
                    SettingId = s.SettingId.ConvertToString(),
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
                    CreatedAt = s.Audit.CreatedOn,
                    UpdatedAt = s.Audit.UpdatedOn ?? s.Audit.CreatedOn,
                    CreatedBy = s.Audit.CreatedBy,
                    UpdatedBy = s.Audit.UpdatedBy ?? string.Empty
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

        #region Theme Management

        /// <summary>
        /// Get current theme configuration
        /// </summary>
        /// <returns>JSON result with current theme</returns>
        [HttpGet]
        public async Task<IActionResult> GetCurrentTheme()
        {
            try
            {
                var currentTheme = await _settingService.GetSettingValueAsync<string>("UI", "CurrentTheme", "default");
                return Json(new { success = true, theme = currentTheme });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current theme");
                return Json(new { success = false, message = "Error getting current theme" });
            }
        }

        /// <summary>
        /// Get available themes
        /// </summary>
        /// <returns>JSON result with available themes</returns>
        [HttpGet]
        public async Task<IActionResult> GetAvailableThemes()
        {
            try
            {
                var availableThemesString = await _settingService.GetSettingValueAsync<string>("UI", "AvailableThemes", "default,dark,blue,green,purple");
                var availableThemes = availableThemesString.Split(',').Select(t => t.Trim()).ToList();
                
                var themeDetails = availableThemes.Select(theme => new
                {
                    Name = theme,
                    DisplayName = GetThemeDisplayName(theme),
                    CssFile = $"/css/themes/{theme}-theme.css",
                    IsDarkMode = theme.Contains("dark", StringComparison.OrdinalIgnoreCase)
                }).ToList();

                return Json(new { success = true, themes = themeDetails });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available themes");
                return Json(new { success = false, message = "Error getting available themes" });
            }
        }

        /// <summary>
        /// Set current theme
        /// </summary>
        /// <param name="themeName">Name of the theme to set</param>
        /// <returns>JSON result</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetCurrentTheme([FromBody] ThemeRequestViewModel request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Theme))
                {
                    return Json(new { success = false, message = "Theme name is required" });
                }

                // Validate theme exists in available themes
                var availableThemesString = await _settingService.GetSettingValueAsync<string>("UI", "AvailableThemes", "default,dark,blue,green,purple");
                var availableThemes = availableThemesString.Split(',').Select(t => t.Trim()).ToList();
                
                if (!availableThemes.Contains(request.Theme, StringComparer.OrdinalIgnoreCase))
                {
                    return Json(new { success = false, message = "Invalid theme selected" });
                }

                var currentUser = GetCurrentUser();
                var setting = await _settingService.GetSettingAsync("UI", "CurrentTheme");
                
                if (setting != null)
                {
                    await _settingService.UpdateSettingAsync(setting.SettingId, request.Theme, currentUser);
                }

                return Json(new 
                { 
                    success = true, 
                    message = "Theme updated successfully", 
                    theme = request.Theme,
                    cssFile = $"/css/themes/{request.Theme}.css"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting current theme to {ThemeName}", request.Theme);
                return Json(new { success = false, message = "Error updating theme. Please try again." });
            }
        }

        /// <summary>
        /// Toggle theme auto-detection
        /// </summary>
        /// <param name="enabled">Whether to enable auto-detection</param>
        /// <returns>JSON result</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetThemeAutoDetect([FromBody] ThemeAutoDetectRequestViewModel request)
        {
            try
            {
                var currentUser = GetCurrentUser();
                var setting = await _settingService.GetSettingAsync("UI", "ThemeAutoDetect");
                
                if (setting != null)
                {
                    await _settingService.UpdateSettingAsync(setting.SettingId, request.AutoDetect.ToString().ToLower(), currentUser);
                }

                return Json(new 
                { 
                    success = true, 
                    message = "Theme auto-detection updated successfully", 
                    enabled = request.AutoDetect
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting theme auto-detection to {Enabled}", request.AutoDetect);
                return Json(new { success = false, message = "Error updating theme auto-detection. Please try again." });
            }
        }

        /// <summary>
        /// Generate CSS for current theme
        /// </summary>
        /// <returns>CSS content</returns>
        [HttpGet]
        public async Task<IActionResult> GetThemeCss()
        {
            try
            {
                var currentTheme = await _settingService.GetSettingValueAsync<string>("UI", "CurrentTheme", "default");
                var cssFilePath = Path.Combine(_hostEnvironment.WebRootPath, "css", "themes", $"{currentTheme}-theme.css");
                
                if (System.IO.File.Exists(cssFilePath))
                {
                    var cssContent = await System.IO.File.ReadAllTextAsync(cssFilePath);
                    return Content(cssContent, "text/css");
                }
                
                // Fallback to default theme
                var defaultCssPath = Path.Combine(_hostEnvironment.WebRootPath, "css", "themes", "default-theme.css");
                if (System.IO.File.Exists(defaultCssPath))
                {
                    var defaultCssContent = await System.IO.File.ReadAllTextAsync(defaultCssPath);
                    return Content(defaultCssContent, "text/css");
                }
                
                return Content("/* No theme CSS found */", "text/css");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting theme CSS");
                return Content("/* Error loading theme CSS */", "text/css");
            }
        }

        #endregion

        /// <summary>
        /// Get display name for theme
        /// </summary>
        /// <param name="themeName">Theme internal name</param>
        /// <returns>Display name</returns>
        private static string GetThemeDisplayName(string themeName)
        {
            return themeName.ToLower() switch
            {
                "default" => "Default Light",
                "dark" => "Dark Mode",
                "blue" => "Ocean Blue",
                "green" => "Forest Green",
                "purple" => "Royal Purple",
                "clean" => "Clean Modern",
                "modern1" => "Modern Aurora",
                "modern2" => "Modern Professional",
                _ => themeName.Substring(0, 1).ToUpper() + themeName.Substring(1).ToLower()
            };
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
