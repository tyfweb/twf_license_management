using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Settings
{
    /// <summary>
    /// View model for updating a setting value
    /// </summary>
    public class UpdateSettingViewModel
    {
        /// <summary>
        /// ID of the setting to update
        /// </summary>
        [Required]
        public string SettingId { get; set; } = string.Empty;

        /// <summary>
        /// New value for the setting
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// Optional validation message from client
        /// </summary>
        public string? ValidationMessage { get; set; }
    }
}
