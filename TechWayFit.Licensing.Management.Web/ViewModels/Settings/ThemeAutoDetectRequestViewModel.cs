using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Settings
{
    /// <summary>
    /// View model for theme auto-detect requests
    /// </summary>
    public class ThemeAutoDetectRequestViewModel
    {
        /// <summary>
        /// Whether to enable auto-detection of system theme
        /// </summary>
        [Required]
        public bool AutoDetect { get; set; }
    }
}
