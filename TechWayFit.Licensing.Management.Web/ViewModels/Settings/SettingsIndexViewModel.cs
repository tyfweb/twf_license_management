namespace TechWayFit.Licensing.Management.Web.ViewModels.Settings
{
    /// <summary>
    /// View model for the settings index page
    /// </summary>
    public class SettingsIndexViewModel
    {
        /// <summary>
        /// All settings grouped by category
        /// </summary>
        public Dictionary<string, List<SettingViewModel>> SettingsGrouped { get; set; } = new();

        /// <summary>
        /// Read-only configuration settings from appsettings.json
        /// </summary>
        public Dictionary<string, object> ConfigurationSettings { get; set; } = new();

        /// <summary>
        /// List of available categories
        /// </summary>
        public List<string> Categories { get; set; } = new();

        /// <summary>
        /// Currently selected category for filtering
        /// </summary>
        public string? SelectedCategory { get; set; }

        /// <summary>
        /// Search term for filtering settings
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Total number of settings
        /// </summary>
        public int TotalSettings => SettingsGrouped.Values.SelectMany(s => s).Count();

        /// <summary>
        /// Number of modified settings (different from default)
        /// </summary>
        public int ModifiedSettings => SettingsGrouped.Values.SelectMany(s => s).Count(s => s.IsModified);

        /// <summary>
        /// Number of read-only settings
        /// </summary>
        public int ReadOnlySettings => SettingsGrouped.Values.SelectMany(s => s).Count(s => s.IsReadOnly);

        /// <summary>
        /// Get settings for a specific category
        /// </summary>
        /// <param name="category">Category name</param>
        /// <returns>Settings in the category</returns>
        public List<SettingViewModel> GetSettingsForCategory(string category)
        {
            return SettingsGrouped.TryGetValue(category, out var settings) ? settings : new List<SettingViewModel>();
        }

        /// <summary>
        /// Check if a category has any settings
        /// </summary>
        /// <param name="category">Category name</param>
        /// <returns>True if category has settings</returns>
        public bool HasSettingsInCategory(string category)
        {
            return SettingsGrouped.ContainsKey(category) && SettingsGrouped[category].Any();
        }

        /// <summary>
        /// Get the count of settings in a category
        /// </summary>
        /// <param name="category">Category name</param>
        /// <returns>Number of settings in the category</returns>
        public int GetCategorySettingsCount(string category)
        {
            return SettingsGrouped.TryGetValue(category, out var settings) ? settings.Count : 0;
        }

        /// <summary>
        /// Get the count of modified settings in a category
        /// </summary>
        /// <param name="category">Category name</param>
        /// <returns>Number of modified settings in the category</returns>
        public int GetCategoryModifiedCount(string category)
        {
            return SettingsGrouped.TryGetValue(category, out var settings) ? settings.Count(s => s.IsModified) : 0;
        }
    }
}
