using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.WebUI.ViewModels.Consumer
{
    /// <summary>
    /// Consumer view model for list items
    /// </summary>
    public class ConsumerViewModel
    {
        public string ConsumerId { get; set; } = string.Empty;
        public string OrganizationName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public int TotalLicenses { get; set; }
        public int ActiveLicenses { get; set; }
    }
}
