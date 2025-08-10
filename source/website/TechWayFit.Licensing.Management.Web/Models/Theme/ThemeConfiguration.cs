using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Web.Models.Theme
{
    /// <summary>
    /// Represents a theme configuration with all its visual properties
    /// </summary>
    public class ThemeConfiguration
    {
        /// <summary>
        /// Unique identifier for the theme (used in CSS and storage)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string ThemeName { get; set; } = string.Empty;

        /// <summary>
        /// Display name shown to users in the theme selector
        /// </summary>
        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Brief description of the theme
        /// </summary>
        [StringLength(200)]
        public string? Description { get; set; }

        /// <summary>
        /// CSS variables and their values for this theme
        /// </summary>
        public Dictionary<string, string> CssVariables { get; set; } = new();

        /// <summary>
        /// Path to preview image for theme gallery
        /// </summary>
        public string? PreviewImage { get; set; }

        /// <summary>
        /// Whether this is a dark mode theme
        /// </summary>
        public bool IsDarkMode { get; set; }

        /// <summary>
        /// Primary accent color (hex format)
        /// </summary>
        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", 
            ErrorMessage = "Accent color must be a valid hex color")]
        public string AccentColor { get; set; } = "#3b82f6";

        /// <summary>
        /// Secondary accent color (hex format)
        /// </summary>
        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", 
            ErrorMessage = "Secondary color must be a valid hex color")]
        public string? SecondaryColor { get; set; }

        /// <summary>
        /// Background pattern CSS (optional)
        /// </summary>
        public string? BackgroundPattern { get; set; }

        /// <summary>
        /// Whether this theme is built-in or custom
        /// </summary>
        public bool IsBuiltIn { get; set; } = true;

        /// <summary>
        /// Theme category for organization
        /// </summary>
        public ThemeCategory Category { get; set; } = ThemeCategory.Professional;

        /// <summary>
        /// When this theme was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When this theme was last modified
        /// </summary>
        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Theme author (for custom themes)
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// Theme version
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Whether theme is available to all users
        /// </summary>
        public bool IsPublic { get; set; } = true;
    }

    /// <summary>
    /// Theme categories for organization
    /// </summary>
    public enum ThemeCategory
    {
        Professional,
        Creative,
        Minimal,
        Dark,
        Colorful,
        Custom,
        Seasonal
    }
}
