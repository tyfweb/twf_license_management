using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.WebUI.ViewModels.Consumer
{
    /// <summary>
    /// Consumer create view model
    /// </summary>
    public class ConsumerCreateViewModel
    {
        [Required(ErrorMessage = "Organization name is required")]
        [StringLength(200, ErrorMessage = "Organization name cannot exceed 200 characters")]
        [Display(Name = "Organization Name")]
        public string OrganizationName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact person is required")]
        [StringLength(100, ErrorMessage = "Contact person cannot exceed 100 characters")]
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        [Display(Name = "Contact Email")]
        public string ContactEmail { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Display(Name = "Contact Phone")]
        public string ContactPhone { get; set; } = string.Empty;


        [StringLength(100, ErrorMessage = "Contact person cannot exceed 100 characters")]
        [Display(Name = "Secondary Contact Person")]
        public string? SecondaryContactPerson { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        [Display(Name = "Secondary Contact Email")]
        public string? SecondaryContactEmail { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Display(Name = "Secondary Contact Phone")]
        public string? SecondaryContactPhone { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "State cannot exceed 100 characters")]
        [Display(Name = "State/Province")]
        public string State { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        [Display(Name = "Country")]
        public string Country { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = string.Empty;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        [Display(Name = "Notes")]
        public string Notes { get; set; } = string.Empty;
    }
}
