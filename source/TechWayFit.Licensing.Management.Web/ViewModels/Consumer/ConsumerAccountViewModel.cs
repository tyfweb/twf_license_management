using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Management.Core.Models.Consumer;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Consumer
{
    /// <summary>
    /// ViewModel for ConsumerAccount operations
    /// </summary>
    public class ConsumerAccountViewModel
    {
        public Guid ConsumerId { get; set; }
        public Guid TenantId { get; set; }

        [Required(ErrorMessage = "Company name is required")]
        [StringLength(200, ErrorMessage = "Company name cannot exceed 200 characters")]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Account code cannot exceed 50 characters")]
        [Display(Name = "Account Code")]
        public string? AccountCode { get; set; }

        [Required(ErrorMessage = "Primary contact name is required")]
        [StringLength(200, ErrorMessage = "Primary contact name cannot exceed 200 characters")]
        [Display(Name = "Primary Contact Name")]
        public string PrimaryContactName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Primary contact email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        [Display(Name = "Primary Contact Email")]
        public string PrimaryContactEmail { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(50, ErrorMessage = "Phone number cannot exceed 50 characters")]
        [Display(Name = "Primary Contact Phone")]
        public string PrimaryContactPhone { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Position cannot exceed 100 characters")]
        [Display(Name = "Primary Contact Position")]
        public string PrimaryContactPosition { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Secondary contact name cannot exceed 200 characters")]
        [Display(Name = "Secondary Contact Name")]
        public string? SecondaryContactName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        [Display(Name = "Secondary Contact Email")]
        public string? SecondaryContactEmail { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(50, ErrorMessage = "Phone number cannot exceed 50 characters")]
        [Display(Name = "Secondary Contact Phone")]
        public string? SecondaryContactPhone { get; set; }

        [StringLength(100, ErrorMessage = "Position cannot exceed 100 characters")]
        [Display(Name = "Secondary Contact Position")]
        public string? SecondaryContactPosition { get; set; }

        [Display(Name = "Activated Date")]
        public DateTime ActivatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Subscription End Date")]
        public DateTime? SubscriptionEnd { get; set; }

        [StringLength(500, ErrorMessage = "Street cannot exceed 500 characters")]
        [Display(Name = "Street Address")]
        public string AddressStreet { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        [Display(Name = "City")]
        public string AddressCity { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "State cannot exceed 100 characters")]
        [Display(Name = "State/Province")]
        public string AddressState { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        [Display(Name = "Postal Code")]
        public string AddressPostalCode { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        [Display(Name = "Country")]
        public string AddressCountry { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        [Display(Name = "Notes")]
        public string Notes { get; set; } = string.Empty;

        [Display(Name = "Status")]
        public ConsumerStatus Status { get; set; } = ConsumerStatus.Prospect;

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
    }
}
