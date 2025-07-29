using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Settings;
using Microsoft.Extensions.Logging;

namespace TechWayFit.Licensing.Management.Web.Helpers
{
    /// <summary>
    /// Helper class for accessing settings in controllers and other C# code
    /// </summary>
    public class SettingsHelper
    {
        private readonly ISettingService _settingService;
        private readonly ILogger<SettingsHelper> _logger;

        public SettingsHelper(ISettingService settingService, ILogger<SettingsHelper> logger)
        {
            _settingService = settingService;
            _logger = logger;
        }

        /// <summary>
        /// Get a setting value by category and key
        /// </summary>
        /// <param name="category">Setting category</param>
        /// <param name="key">Setting key</param>
        /// <param name="defaultValue">Default value if setting not found</param>
        /// <returns>Setting value or default</returns>
        public async Task<string?> GetSettingAsync(string category, string key, string? defaultValue = null)
        {
            try
            {
                var setting = await _settingService.GetSettingAsync(category, key);
                return setting?.Value ?? defaultValue;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get setting {Category}.{Key}", category, key);
                return defaultValue;
            }
        }

        /// <summary>
        /// Get a setting value as a specific type
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <param name="category">Setting category</param>
        /// <param name="key">Setting key</param>
        /// <param name="defaultValue">Default value if setting not found or conversion fails</param>
        /// <returns>Converted setting value</returns>
        public async Task<T> GetSettingAsAsync<T>(string category, string key, T defaultValue = default!)
        {
            try
            {
                var setting = await _settingService.GetSettingAsync(category, key);
                if (setting?.Value == null)
                {
                    return defaultValue;
                }

                return ConvertSettingValue<T>(setting.Value, defaultValue);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get setting {Category}.{Key} as {Type}", category, key, typeof(T).Name);
                return defaultValue;
            }
        }

        /// <summary>
        /// Check if a boolean setting is enabled
        /// </summary>
        /// <param name="category">Setting category</param>
        /// <param name="key">Setting key</param>
        /// <param name="defaultValue">Default value if setting not found</param>
        /// <returns>True if setting is enabled</returns>
        public async Task<bool> IsSettingEnabledAsync(string category, string key, bool defaultValue = false)
        {
            return await GetSettingAsAsync(category, key, defaultValue);
        }

        /// <summary>
        /// Get multiple settings by category
        /// </summary>
        /// <param name="category">Setting category</param>
        /// <returns>Dictionary of key-value pairs</returns>
        public async Task<Dictionary<string, string?>> GetCategorySettingsAsync(string category)
        {
            try
            {
                var settings = await _settingService.GetSettingsByCategoryAsync(category);
                return settings.ToDictionary(s => s.Key, s => s.Value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get settings for category {Category}", category);
                return new Dictionary<string, string?>();
            }
        }

        /// <summary>
        /// Get application-specific settings with helper methods
        /// </summary>
        public ApplicationSettings App => new ApplicationSettings(this);

        /// <summary>
        /// Get system-specific settings with helper methods
        /// </summary>
        public SystemSettings System => new SystemSettings(this);

        /// <summary>
        /// Get email-specific settings with helper methods
        /// </summary>
        public EmailSettings Email => new EmailSettings(this);

        /// <summary>
        /// Get security-specific settings with helper methods
        /// </summary>
        public SecuritySettings Security => new SecuritySettings(this);

        /// <summary>
        /// Get license-specific settings with helper methods
        /// </summary>
        public LicenseSettings License => new LicenseSettings(this);

        /// <summary>
        /// Get UI-specific settings with helper methods
        /// </summary>
        public UISettings UI => new UISettings(this);

        /// <summary>
        /// Convert setting value to specified type
        /// </summary>
        private static T ConvertSettingValue<T>(string value, T defaultValue)
        {
            try
            {
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)value;
                }
                else if (typeof(T) == typeof(bool))
                {
                    return (T)(object)(value.ToLowerInvariant() == "true" || value == "1");
                }
                else if (typeof(T) == typeof(int))
                {
                    return int.TryParse(value, out var intValue) ? (T)(object)intValue : defaultValue;
                }
                else if (typeof(T) == typeof(decimal))
                {
                    return decimal.TryParse(value, out var decimalValue) ? (T)(object)decimalValue : defaultValue;
                }
                else if (typeof(T) == typeof(double))
                {
                    return double.TryParse(value, out var doubleValue) ? (T)(object)doubleValue : defaultValue;
                }
                else if (typeof(T) == typeof(DateTime))
                {
                    return DateTime.TryParse(value, out var dateValue) ? (T)(object)dateValue : defaultValue;
                }
                else
                {
                    // Try to convert using Convert.ChangeType
                    return (T)Convert.ChangeType(value, typeof(T));
                }
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Application-specific settings helper
    /// </summary>
    public class ApplicationSettings
    {
        private readonly SettingsHelper _helper;

        internal ApplicationSettings(SettingsHelper helper)
        {
            _helper = helper;
        }

        public async Task<string?> GetNameAsync() => await _helper.GetSettingAsync("System", "ApplicationName", "TechWayFit License Management");
        public async Task<string?> GetVersionAsync() => await _helper.GetSettingAsync("System", "ApplicationVersion", "1.0.0");
        public async Task<string?> GetCompanyNameAsync() => await _helper.GetSettingAsync("System", "CompanyName", "TechWayFit Solutions");
        public async Task<string?> GetLogoUrlAsync() => await _helper.GetSettingAsync("System", "CompanyLogo", "/images/logo.png");
        public async Task<string?> GetFaviconUrlAsync() => await _helper.GetSettingAsync("System", "FaviconUrl", "/favicon.ico");
    }

    /// <summary>
    /// System-specific settings helper
    /// </summary>
    public class SystemSettings
    {
        private readonly SettingsHelper _helper;

        internal SystemSettings(SettingsHelper helper)
        {
            _helper = helper;
        }

        public async Task<int> GetSessionTimeoutAsync() => await _helper.GetSettingAsAsync("Security", "SessionTimeoutMinutes", 30);
        public async Task<bool> IsMaintenanceModeAsync() => await _helper.GetSettingAsAsync("System", "MaintenanceMode", false);
    }

    /// <summary>
    /// Email-specific settings helper
    /// </summary>
    public class EmailSettings
    {
        private readonly SettingsHelper _helper;

        internal EmailSettings(SettingsHelper helper)
        {
            _helper = helper;
        }

        public async Task<string?> GetSmtpServerAsync() => await _helper.GetSettingAsync("Email", "SmtpServer", "smtp.gmail.com");
        public async Task<int> GetSmtpPortAsync() => await _helper.GetSettingAsAsync("Email", "SmtpPort", 587);
        public async Task<string?> GetUsernameAsync() => await _helper.GetSettingAsync("Email", "SmtpUsername");
        public async Task<string?> GetPasswordAsync() => await _helper.GetSettingAsync("Email", "SmtpPassword");
        public async Task<string?> GetFromAddressAsync() => await _helper.GetSettingAsync("Email", "FromAddress");
        public async Task<string?> GetFromNameAsync() => await _helper.GetSettingAsync("Email", "FromName");
        public async Task<bool> IsSslEnabledAsync() => await _helper.GetSettingAsAsync("Email", "EnableSsl", true);
        public async Task<bool> IsEnabledAsync() => await _helper.GetSettingAsAsync("Notification", "EmailNotificationsEnabled", true);
    }

    /// <summary>
    /// Security-specific settings helper
    /// </summary>
    public class SecuritySettings
    {
        private readonly SettingsHelper _helper;

        internal SecuritySettings(SettingsHelper helper)
        {
            _helper = helper;
        }

        public async Task<int> GetPasswordMinLengthAsync() => await _helper.GetSettingAsAsync("Security", "PasswordMinLength", 8);
        public async Task<bool> RequireUppercaseAsync() => await _helper.GetSettingAsAsync("Security", "RequireUppercase", true);
        public async Task<bool> RequireLowercaseAsync() => await _helper.GetSettingAsAsync("Security", "RequireLowercase", true);
        public async Task<bool> RequireNumbersAsync() => await _helper.GetSettingAsAsync("Security", "RequireNumbers", true);
        public async Task<bool> RequireSymbolsAsync() => await _helper.GetSettingAsAsync("Security", "RequireSymbols", true);
    }

    /// <summary>
    /// License-specific settings helper
    /// </summary>
    public class LicenseSettings
    {
        private readonly SettingsHelper _helper;

        internal LicenseSettings(SettingsHelper helper)
        {
            _helper = helper;
        }

        public async Task<int> GetDefaultDurationDaysAsync() => await _helper.GetSettingAsAsync("License", "DefaultDurationDays", 365);
        public async Task<int> GetGracePeriodDaysAsync() => await _helper.GetSettingAsAsync("License", "GracePeriodDays", 30);
        public async Task<bool> IsAutoRenewEnabledAsync() => await _helper.GetSettingAsAsync("License", "AutoRenewEnabled", false);
        public async Task<int> GetMaxActivationsAsync() => await _helper.GetSettingAsAsync("License", "MaxActivationsPerLicense", 5);
    }

    /// <summary>
    /// UI-specific settings helper
    /// </summary>
    public class UISettings
    {
        private readonly SettingsHelper _helper;

        internal UISettings(SettingsHelper helper)
        {
            _helper = helper;
        }

        public async Task<string?> GetDefaultThemeAsync() => await _helper.GetSettingAsync("UI", "DefaultTheme", "light");
        public async Task<int> GetItemsPerPageAsync() => await _helper.GetSettingAsAsync("UI", "ItemsPerPage", 25);
        public async Task<bool> IsDarkModeEnabledAsync() => await _helper.GetSettingAsAsync("UI", "EnableDarkMode", true);
    }
}
