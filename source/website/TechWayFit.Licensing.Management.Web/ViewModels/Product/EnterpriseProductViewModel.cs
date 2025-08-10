using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Product
{
    /// <summary>
    /// ViewModel for EnterpriseProduct operations
    /// </summary>
    public class EnterpriseProductViewModel
    {
        public Guid ProductId { get; set; }
        public Guid TenantId { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Version is required")]
        [StringLength(50, ErrorMessage = "Version cannot exceed 50 characters")]
        [Display(Name = "Version")]
        public string Version { get; set; } = "1.0.0";

        [Required(ErrorMessage = "Release date is required")]
        [Display(Name = "Release Date")]
        public DateTime ReleaseDate { get; set; } = DateTime.UtcNow;

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(255, ErrorMessage = "Support email cannot exceed 255 characters")]
        [Display(Name = "Support Email")]
        public string SupportEmail { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(50, ErrorMessage = "Support phone cannot exceed 50 characters")]
        [Display(Name = "Support Phone")]
        public string SupportPhone { get; set; } = string.Empty;

        [Display(Name = "Decommission Date")]
        public DateTime? DecommissionDate { get; set; }

        [Display(Name = "Status")]
        public ProductStatus Status { get; set; } = ProductStatus.Active;

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

        // Collections
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}
