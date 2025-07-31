using TechWayFit.Licensing.Management.Web.Models.Theme;

namespace TechWayFit.Licensing.Management.Web.Services.Theme
{
    /// <summary>
    /// Service interface for managing themes and user preferences
    /// </summary>
    public interface IThemeService
    {
        /// <summary>
        /// Get all available themes
        /// </summary>
        Task<IEnumerable<ThemeConfiguration>> GetAvailableThemesAsync();

        /// <summary>
        /// Get a specific theme by name
        /// </summary>
        Task<ThemeConfiguration?> GetThemeAsync(string themeName);

        /// <summary>
        /// Get user's theme preference
        /// </summary>
        Task<UserThemePreference?> GetUserThemePreferenceAsync(string userId);

        /// <summary>
        /// Save user's theme preference
        /// </summary>
        Task SaveUserThemePreferenceAsync(UserThemePreference preference);

        /// <summary>
        /// Get the default theme
        /// </summary>
        Task<ThemeConfiguration> GetDefaultThemeAsync();

        /// <summary>
        /// Create a custom theme
        /// </summary>
        Task<ThemeConfiguration> CreateCustomThemeAsync(ThemeConfiguration theme, string userId);

        /// <summary>
        /// Update an existing custom theme
        /// </summary>
        Task<ThemeConfiguration> UpdateCustomThemeAsync(ThemeConfiguration theme, string userId);

        /// <summary>
        /// Delete a custom theme
        /// </summary>
        Task<bool> DeleteCustomThemeAsync(string themeName, string userId);

        /// <summary>
        /// Generate CSS for a theme
        /// </summary>
        Task<string> GenerateThemeCssAsync(string themeName);

        /// <summary>
        /// Generate CSS for user's customized theme
        /// </summary>
        Task<string> GenerateUserThemeCssAsync(string userId);

        /// <summary>
        /// Import theme from JSON
        /// </summary>
        Task<ThemeConfiguration> ImportThemeAsync(string themeJson, string userId);

        /// <summary>
        /// Export theme to JSON
        /// </summary>
        Task<string> ExportThemeAsync(string themeName);

        /// <summary>
        /// Validate theme configuration
        /// </summary>
        Task<(bool IsValid, List<string> Errors)> ValidateThemeAsync(ThemeConfiguration theme);

        /// <summary>
        /// Get theme categories
        /// </summary>
        Task<IEnumerable<ThemeCategory>> GetThemeCategoriesAsync();

        /// <summary>
        /// Get themes by category
        /// </summary>
        Task<IEnumerable<ThemeConfiguration>> GetThemesByCategoryAsync(ThemeCategory category);

        /// <summary>
        /// Search themes by name or description
        /// </summary>
        Task<IEnumerable<ThemeConfiguration>> SearchThemesAsync(string searchTerm);

        /// <summary>
        /// Get popular themes
        /// </summary>
        Task<IEnumerable<ThemeConfiguration>> GetPopularThemesAsync(int count = 10);

        /// <summary>
        /// Reset user theme to default
        /// </summary>
        Task ResetUserThemeAsync(string userId);
    }
}
