using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Management.Core.Models.Consumer;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Consumer
{
    /// <summary>
    /// Consumer view model for list items and details
    /// </summary>
    public class ConsumerViewModel
    {
        public string ConsumerId { get; set; } = string.Empty;

        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;
        
        [Display(Name = "Account Code")]
        public string AccountCode { get; set; } = string.Empty;
        
        [Display(Name = "Primary Contact")]
        public string PrimaryContactName { get; set; } = string.Empty;
        
        [Display(Name = "Contact Email")]
        public string PrimaryContactEmail { get; set; } = string.Empty;
        
        [Display(Name = "Contact Phone")]
        public string PrimaryContactPhone { get; set; } = string.Empty;
        
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;
        
        [Display(Name = "Status")]
        public ConsumerStatus Status { get; set; }
        
        [Display(Name = "Active")]
        public bool IsActive { get; set; }
        
        [Display(Name = "Notes")]
        public string Notes { get; set; } = string.Empty;
        
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; } = string.Empty;
        
        [Display(Name = "Created On")]
        public DateTime CreatedOn { get; set; }
        
        [Display(Name = "Updated By")]
        public string UpdatedBy { get; set; } = string.Empty;
        
        [Display(Name = "Updated On")]
        public DateTime? UpdatedOn { get; set; }
        
        // Summary properties for list view
        public int TotalLicenses { get; set; }
        public int ActiveLicenses { get; set; }
        
        // Alias properties for view compatibility
        public string OrganizationName => CompanyName;
        public string ContactPerson => PrimaryContactName;
        public string ContactEmail => PrimaryContactEmail;
    }
}
