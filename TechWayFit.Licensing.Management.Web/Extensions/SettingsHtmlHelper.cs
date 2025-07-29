using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Settings;

namespace TechWayFit.Licensing.Management.Web.Extensions
{
    /// <summary>
    /// HTML Helper extension for accessing settings in MVC views
    /// </summary>
    public static class SettingsHtmlHelper
    {
        /// <summary>
        /// Get a setting value by category and key
        /// </summary>
        /// <param name="htmlHelper">The HTML helper</param>
        /// <param name="category">Setting category</param>
        /// <param name="key">Setting key</param>
        /// <param name="defaultValue">Default value if setting not found</param>
        /// <returns>Setting value as HTML string</returns>
        public static IHtmlContent Setting(this IHtmlHelper htmlHelper, string category, string key, string defaultValue = "")
        {
            try
            {
                var settingService = htmlHelper.ViewContext.HttpContext.RequestServices
                    .GetService<ISettingService>();

                if (settingService == null)
                {
                    return new HtmlString(defaultValue);
                }

                // This is a synchronous call from async method - we'll need to handle this carefully
                var setting = GetSettingSync(settingService, category, key);
                return new HtmlString(setting?.Value ?? defaultValue);
            }
            catch
            {
                return new HtmlString(defaultValue);
            }
        }

        /// <summary>
        /// Get a setting value as a specific type
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <param name="htmlHelper">The HTML helper</param>
        /// <param name="category">Setting category</param>
        /// <param name="key">Setting key</param>
        /// <param name="defaultValue">Default value if setting not found or conversion fails</param>
        /// <returns>Converted setting value</returns>
        public static T SettingAs<T>(this IHtmlHelper htmlHelper, string category, string key, T defaultValue = default!)
        {
            try
            {
                var settingService = htmlHelper.ViewContext.HttpContext.RequestServices
                    .GetService<ISettingService>();

                if (settingService == null)
                {
                    return defaultValue;
                }

                var setting = GetSettingSync(settingService, category, key);
                if (setting?.Value == null)
                {
                    return defaultValue;
                }

                return ConvertSettingValue<T>(setting.Value, defaultValue);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Check if a boolean setting is enabled
        /// </summary>
        /// <param name="htmlHelper">The HTML helper</param>
        /// <param name="category">Setting category</param>
        /// <param name="key">Setting key</param>
        /// <param name="defaultValue">Default value if setting not found</param>
        /// <returns>True if setting is enabled</returns>
        public static bool IsSettingEnabled(this IHtmlHelper htmlHelper, string category, string key, bool defaultValue = false)
        {
            return htmlHelper.SettingAs(category, key, defaultValue);
        }

        /// <summary>
        /// Get application name setting
        /// </summary>
        /// <param name="htmlHelper">The HTML helper</param>
        /// <returns>Application name</returns>
        public static IHtmlContent AppName(this IHtmlHelper htmlHelper)
        {
            return htmlHelper.Setting("System", "ApplicationName", "TechWayFit License Management");
        }

        /// <summary>
        /// Get company name setting
        /// </summary>
        /// <param name="htmlHelper">The HTML helper</param>
        /// <returns>Company name</returns>
        public static IHtmlContent CompanyName(this IHtmlHelper htmlHelper)
        {
            return htmlHelper.Setting("System", "CompanyName", "TechWayFit Solutions");
        }

        /// <summary>
        /// Get company logo URL setting
        /// </summary>
        /// <param name="htmlHelper">The HTML helper</param>
        /// <returns>Logo URL</returns>
        public static IHtmlContent LogoUrl(this IHtmlHelper htmlHelper)
        {
            return htmlHelper.Setting("System", "CompanyLogo", "/images/logo.png");
        }

        /// <summary>
        /// Get application version setting
        /// </summary>
        /// <param name="htmlHelper">The HTML helper</param>
        /// <returns>Application version</returns>
        public static IHtmlContent AppVersion(this IHtmlHelper htmlHelper)
        {
            return htmlHelper.Setting("System", "ApplicationVersion", "1.0.0");
        }

        /// <summary>
        /// Helper method to get setting synchronously (not ideal but needed for HTML helpers)
        /// </summary>
        private static Setting? GetSettingSync(ISettingService settingService, string category, string key)
        {
            try
            {
                // We use Task.Run to avoid deadlocks, though this is not ideal
                return Task.Run(async () => await settingService.GetSettingAsync(category, key)).GetAwaiter().GetResult();
            }
            catch
            {
                return null;
            }
        }

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
}
