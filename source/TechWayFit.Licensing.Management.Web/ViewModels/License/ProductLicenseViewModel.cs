using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Core.Models; 

namespace TechWayFit.Licensing.Management.Web.ViewModels.License
{
    /// <summary>
    /// ViewModel for ProductLicense operations
    /// </summary>
    public class ProductLicenseViewModel
    {
        public Guid LicenseId { get; set; }
        public Guid TenantId { get; set; }
        public Guid ProductId { get; set; }
        public Guid ConsumerId { get; set; }
        public Guid TierId { get; set; }

        [Required(ErrorMessage = "License key is required")]
        [StringLength(500, ErrorMessage = "License key cannot exceed 500 characters")]
        [Display(Name = "License Key")]
        public string LicenseKey { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start date is required")]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "End date is required")]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddYears(1);

        [Display(Name = "Status")]
        public LicenseStatus Status { get; set; } = LicenseStatus.Active;

        [Range(1, int.MaxValue, ErrorMessage = "Max users must be greater than 0")]
        [Display(Name = "Max Users")]
        public int? MaxUsers { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Max installations must be greater than 0")]
        [Display(Name = "Max Installations")]
        public int? MaxInstallations { get; set; }

        [Display(Name = "Is Trial")]
        public bool IsTrial { get; set; }

        [StringLength(100, ErrorMessage = "Trial period description cannot exceed 100 characters")]
        [Display(Name = "Trial Period")]
        public string? TrialPeriod { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created Date")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; } = string.Empty;

        [Display(Name = "Updated Date")]
        public DateTime? UpdatedOn { get; set; }

        [Display(Name = "Updated By")]
        public string? UpdatedBy { get; set; }

        // Workflow properties
        [Display(Name = "Entity Status")]
        public string EntityStatus { get; set; } = "Draft";

        [Display(Name = "Submitted By")]
        public string? SubmittedBy { get; set; }

        [Display(Name = "Submitted On")]
        public DateTime? SubmittedOn { get; set; }

        [Display(Name = "Reviewed By")]
        public string? ReviewedBy { get; set; }

        [Display(Name = "Reviewed On")]
        public DateTime? ReviewedOn { get; set; }

        [StringLength(500, ErrorMessage = "Review comments cannot exceed 500 characters")]
        [Display(Name = "Review Comments")]
        public string? ReviewComments { get; set; }

        // Related data for display
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [Display(Name = "Consumer Name")]
        public string ConsumerName { get; set; } = string.Empty;

        [Display(Name = "Tier Name")]
        public string TierName { get; set; } = string.Empty;

        [Display(Name = "Days Until Expiry")]
        public int DaysUntilExpiry => (EndDate - DateTime.UtcNow).Days;

        [Display(Name = "Is Expired")]
        public bool IsExpired => DateTime.UtcNow > EndDate;

        [Display(Name = "Is Near Expiry")]
        public bool IsNearExpiry => DaysUntilExpiry <= 30 && DaysUntilExpiry > 0;
    }
}
