using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Shared
{
    /// <summary>
    /// View model for individual stats tiles used across product management pages
    /// </summary>
    public class StatsTileViewModel
    {
        /// <summary>
        /// The icon class (FontAwesome) to display in the tile
        /// </summary>
        [Required]
        public string IconClass { get; set; } = string.Empty;

        /// <summary>
        /// The main value/number to display
        /// </summary>
        [Required]
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// The label/description for the stat
        /// </summary>
        [Required]
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// CSS class for styling the tile (e.g., stats-card-success, stats-card-info, etc.)
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// CSS class for the value text color (e.g., text-primary, text-success, etc.)
        /// </summary>
        public string? ValueColorClass { get; set; }

        /// <summary>
        /// Whether the value should be displayed with small text
        /// </summary>
        public bool IsSmallText { get; set; } = false;

        /// <summary>
        /// Optional ID for dynamic content updates via JavaScript
        /// </summary>
        public string? ElementId { get; set; }
    }
}
