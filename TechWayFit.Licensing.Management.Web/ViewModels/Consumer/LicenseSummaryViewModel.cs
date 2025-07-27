using TechWayFit.Licensing.Core.Models;

namespace TechWayFit.Licensing.WebUI.ViewModels.Consumer
{
    /// <summary>
    /// License summary view model
    /// </summary>
    public class LicenseSummaryViewModel
    {
        public string LicenseId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public LicenseTier Tier { get; set; }
        public LicenseStatus Status { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
