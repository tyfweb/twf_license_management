using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using System.Threading.Tasks;

namespace TechWayFit.Licensing.Management.Web.Components
{
    /// <summary>
    /// View component for loading the current theme CSS dynamically
    /// </summary>
    public class ThemeLoaderViewComponent : ViewComponent
    {
        private readonly ISettingService _settingService;
        
        public ThemeLoaderViewComponent(ISettingService settingService)
        {
            _settingService = settingService;
        }

        /// <summary>
        /// Invokes the theme loader to render the appropriate theme CSS link
        /// </summary>
        /// <returns>View result with theme CSS link</returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                // Get current theme from settings
                var currentTheme = await _settingService.GetSettingValueAsync<string>("UI", "CurrentTheme", "default");
                
                // Get theme auto-detect setting
                var autoDetect = await _settingService.GetSettingValueAsync<bool>("UI", "ThemeAutoDetect", false);
                
                // Get transition duration
                var transitionDuration = await _settingService.GetSettingValueAsync<string>("UI", "ThemeTransitionDuration", "200");

                var model = new ThemeLoaderModel
                {
                    CurrentTheme = currentTheme,
                    AutoDetect = autoDetect,
                    TransitionDuration = transitionDuration
                };

                return View(model);
            }
            catch (Exception ex)
            {
                // Log error and fall back to default theme
                // In a real application, you would use proper logging
                System.Diagnostics.Debug.WriteLine($"Error loading theme: {ex.Message}");
                
                var defaultModel = new ThemeLoaderModel
                {
                    CurrentTheme = "default",
                    AutoDetect = false,
                    TransitionDuration = "200"
                };
                
                return View(defaultModel);
            }
        }
    }

    /// <summary>
    /// Model for the theme loader view component
    /// </summary>
    public class ThemeLoaderModel
    {
        public string CurrentTheme { get; set; } = "default";
        public bool AutoDetect { get; set; } = false;
        public string TransitionDuration { get; set; } = "200";
    }
}
