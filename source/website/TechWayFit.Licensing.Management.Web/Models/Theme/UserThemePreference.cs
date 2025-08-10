using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Web.Models.Theme
{
    /// <summary>
    /// Represents a user's theme preference and customizations
    /// </summary>
    public class UserThemePreference
    {
        /// <summary>
        /// Unique identifier for the preference record
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// User ID this preference belongs to
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Selected theme name
        /// </summary>
        [Required]
        [StringLength(50)]
        public string ThemeName { get; set; } = "default";

        /// <summary>
        /// Custom CSS variable overrides specific to this user
        /// </summary>
        public Dictionary<string, string> CustomVariables { get; set; } = new();

        /// <summary>
        /// Whether to auto-detect system dark/light mode preference
        /// </summary>
        public bool UseSystemPreference { get; set; } = false;

        /// <summary>
        /// Custom theme settings
        /// </summary>
        public UserThemeSettings Settings { get; set; } = new();

        /// <summary>
        /// When this preference was last modified
        /// </summary>
        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When this preference was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether this is the active theme for the user
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Additional theme settings that users can customize
    /// </summary>
    public class UserThemeSettings
    {
        /// <summary>
        /// Animation speed preference
        /// </summary>
        public AnimationSpeed AnimationSpeed { get; set; } = AnimationSpeed.Normal;

        /// <summary>
        /// Reduced motion preference for accessibility
        /// </summary>
        public bool ReducedMotion { get; set; } = false;

        /// <summary>
        /// High contrast mode for accessibility
        /// </summary>
        public bool HighContrast { get; set; } = false;

        /// <summary>
        /// Font size preference
        /// </summary>
        public FontSize FontSize { get; set; } = FontSize.Normal;

        /// <summary>
        /// Sidebar width preference
        /// </summary>
        public SidebarWidth SidebarWidth { get; set; } = SidebarWidth.Normal;

        /// <summary>
        /// Custom accent color override
        /// </summary>
        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")]
        public string? CustomAccentColor { get; set; }

        /// <summary>
        /// Enable background patterns
        /// </summary>
        public bool EnableBackgroundPatterns { get; set; } = true;

        /// <summary>
        /// Enable hover effects
        /// </summary>
        public bool EnableHoverEffects { get; set; } = true;

        /// <summary>
        /// Enable smooth transitions
        /// </summary>
        public bool EnableTransitions { get; set; } = true;
    }

    /// <summary>
    /// Animation speed preferences
    /// </summary>
    public enum AnimationSpeed
    {
        None = 0,
        Slow = 1,
        Normal = 2,
        Fast = 3
    }

    /// <summary>
    /// Font size preferences
    /// </summary>
    public enum FontSize
    {
        Small = 0,
        Normal = 1,
        Large = 2,
        ExtraLarge = 3
    }

    /// <summary>
    /// Sidebar width preferences
    /// </summary>
    public enum SidebarWidth
    {
        Narrow = 0,
        Normal = 1,
        Wide = 2
    }
}
