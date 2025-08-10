using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Settings
{
    /// <summary>
    /// View model for theme requests
    /// </summary>
    public class ThemeRequestViewModel
    {
        /// <summary>
        /// The theme name to apply
        /// </summary>
        [Required]
        public string Theme { get; set; } = string.Empty;
    }
}
