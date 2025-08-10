using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Core.Models.Consumer;
using LicenseModels = TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Management.Web.ViewModels.License
{
    /// <summary>
    /// License generation view model
    /// </summary>
    public class LicenseGenerationViewModel
    {
        [Required]
        [Display(Name = "Product")]
        public string ProductId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Consumer")]
        public string ConsumerId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "ProductTier")]
        public string ProductTierId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Licensed To")]
        [StringLength(200)]
        public string LicensedTo { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Contact Person")]
        [StringLength(100)]
        public string ContactPerson { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Contact Email")]
        public string ContactEmail { get; set; } = string.Empty;

        [Display(Name = "Secondary Contact Person")]
        [StringLength(100)]
        public string? SecondaryContactPerson { get; set; }

        [EmailAddress]
        [Display(Name = "Secondary Contact Email")]
        public string? SecondaryContactEmail { get; set; }

        [Required]
        [Display(Name = "Valid From")]
        [DataType(DataType.DateTime)]
        public DateTime ValidFrom { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Valid To")]
        [DataType(DataType.DateTime)]
        public DateTime ValidTo { get; set; } = DateTime.Now.AddYears(1);

        [Required]
        [Display(Name = "License Tier")]
        public LicenseTier Tier { get; set; } = LicenseTier.Community;

        [Display(Name = "Max API Calls Per Month")]
        [Range(0, long.MaxValue, ErrorMessage = "API calls must be a positive number")]
        public long? MaxApiCallsPerMonth { get; set; }

        [Display(Name = "Max Concurrent Connections")]
        [Range(0, int.MaxValue, ErrorMessage = "Concurrent connections must be a positive number")]
        public int? MaxConcurrentConnections { get; set; }

        [Display(Name = "Selected Features")]
        public List<string> SelectedFeatures { get; set; } = new();

        [Display(Name = "Additional Metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new();

        // Helper properties for UI
        public List<ProductViewModel> AvailableProducts { get; set; } = new();
        public List<ConsumerViewModel> AvailableConsumers { get; set; } = new();
        public List<FeatureSelectionViewModel> AvailableFeatures { get; set; } = new();
        public string CreatedBy { get; set; } = string.Empty;

        public bool IsValidDateRange => ValidTo > ValidFrom;
    }

    /// <summary>
    /// License listing view model
    /// </summary>
    public class LicenseListViewModel
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public List<LicenseItemViewModel> Licenses { get; set; } = new();
        public LicenseFilterViewModel Filter { get; set; } = new();
        public PaginationViewModel Pagination { get; set; } = new();
    }

    /// <summary>
    /// Individual license item
    /// </summary>
    public class LicenseItemViewModel
    {
        public string LicenseId { get; set; } = string.Empty;
        public string LicenseCode { get; set; } = string.Empty;
        public string ConsumerName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public LicenseTier Tier { get; set; }
        public LicenseStatus Status { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public int Version { get; set; }
        public int DaysUntilExpiry { get; set; }
        public bool CanRenew { get; set; }
        public bool CanRevoke { get; set; }
        public bool CanSuspend { get; set; }
        public string StatusCssClass => GetStatusCssClass();

        private string GetStatusCssClass()
        {
            return Status switch
            {
                LicenseStatus.Active => "badge-success",
                LicenseStatus.Expired => "badge-danger",
                LicenseStatus.Suspended => "badge-warning",
                LicenseStatus.Revoked => "badge-dark",
                LicenseStatus.Pending => "badge-info",
                LicenseStatus.GracePeriod => "badge-warning",
                _ => "badge-secondary"
            };
        }
    }

    /// <summary>
    /// License details view model
    /// </summary>
    public class LicenseDetailViewModel
    {
        public ProductLicense License { get; set; } = new();
        public ConsumerAccount Consumer { get; set; } = new();
        public EnterpriseProduct Product { get; set; } = new();
        public List<object> AuditHistory { get; set; } = new(); // TODO: Replace with proper LicenseAuditEntry when available
        public LicenseValidationResult ValidationResult { get; set; } = new();
        public List<LicenseVersionViewModel> VersionHistory { get; set; } = new();
        public bool CanEdit { get; set; }
        public bool CanRenew { get; set; }
        public bool CanRevoke { get; set; }
        public bool CanSuspend { get; set; }
        public bool CanReactivate { get; set; }
    }

    /// <summary>
    /// License version information
    /// </summary>
    public class LicenseVersionViewModel
    {
        public string LicenseId { get; set; } = string.Empty;
        public int Version { get; set; }
        public LicenseStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? ReasonForChange { get; set; }
        public bool IsCurrent { get; set; }
    }

    /// <summary>
    /// License renewal view model
    /// </summary>
    public class LicenseRenewalViewModel
    {
        [Required]
        public string CurrentLicenseId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "New Expiry Date")]
        [DataType(DataType.DateTime)]
        public DateTime NewExpiryDate { get; set; } = DateTime.Now.AddYears(1);

        [Display(Name = "New Tier (optional)")]
        public LicenseTier? NewTier { get; set; }

        [Display(Name = "Feature Changes")]
        public List<string> FeatureChanges { get; set; } = new();

        [Required]
        [Display(Name = "Reason for Renewal")]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        [Display(Name = "Additional Metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new();

        // Current license information for display
        public LicenseModels.License CurrentLicense { get; set; } = new();
        public LicenseModels.Consumer Consumer { get; set; } = new();
        public List<FeatureSelectionViewModel> AvailableFeatures { get; set; } = new();
        public string RequestedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// License revocation view model
    /// </summary>
    public class LicenseRevocationViewModel
    {
        [Required]
        public string LicenseId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Revocation Reason")]
        public RevocationReason Reason { get; set; }

        [Required]
        [Display(Name = "Detailed Explanation")]
        [StringLength(1000)]
        public string ReasonDescription { get; set; } = string.Empty;

        [Display(Name = "Effective Date (optional)")]
        [DataType(DataType.DateTime)]
        public DateTime? EffectiveDate { get; set; }

        [Display(Name = "Notify Consumer")]
        public bool NotifyConsumer { get; set; } = true;

        // Current license information for display
        public LicenseModels.License License { get; set; } = new();
        public LicenseModels.Consumer Consumer { get; set; } = new();
        public string RevokedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// License filter options
    /// </summary>
    public class LicenseFilterViewModel
    {
        public string SearchTerm { get; set; } = string.Empty;
        public LicenseStatus? Status { get; set; }
        public LicenseTier? Tier { get; set; }
        public DateTime? ValidFromStart { get; set; }
        public DateTime? ValidFromEnd { get; set; }
        public DateTime? ValidToStart { get; set; }
        public DateTime? ValidToEnd { get; set; }
        public bool ShowExpiring { get; set; }
        public int ExpiringWithinDays { get; set; } = 30;
    }

    /// <summary>
    /// Feature selection for license generation
    /// </summary>
    public class FeatureSelectionViewModel
    {
        public string FeatureId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public FeatureCategory Category { get; set; }
        public bool IsSelected { get; set; }
        public bool IsRequired { get; set; }
        public LicenseTier MinimumTier { get; set; }
        public bool IsAvailableForTier { get; set; }
        public List<string> Dependencies { get; set; } = new();
    }

    /// <summary>
    /// Helper view models
    /// </summary>
    public class ProductViewModel
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public ProductType ProductType { get; set; }
        public bool IsActive { get; set; }
    }

    public class ConsumerViewModel
    {
        public string ConsumerId { get; set; } = string.Empty;
        public string OrganizationName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class PaginationViewModel
    {
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; } = 10;
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    /// <summary>
    /// License generation result view model
    /// </summary>
    public class LicenseGenerationResultViewModel
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public LicenseModels.License? License { get; set; }
        public SignedLicense? SignedLicense { get; set; }
        public string LicenseJson { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }

    /// <summary>
    /// License validation view model
    /// </summary>
    public class LicenseValidationViewModel
    {
        [Required]
        [Display(Name = "License JSON")]
        public string LicenseJson { get; set; } = string.Empty;
        
        public bool IsValid { get; set; }
        public string ValidationMessage { get; set; } = string.Empty;
        public LicenseModels.License? License { get; set; }
    }

    /// <summary>
    /// Key management view model
    /// </summary>
    public class KeyManagementViewModel
    {
        public bool HasCurrentKeys { get; set; }
        public string CurrentPublicKey { get; set; } = string.Empty;
        public string? NewPublicKey { get; set; }
        public bool ShowKeyGeneration { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// View model for license summary in lists (simplified for controller compatibility)
    /// </summary>
    public class LicenseSummaryViewModel
    {
        public string LicenseId { get; set; } = string.Empty;
        public string LicensedTo { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public LicenseTier Tier { get; set; }
        public LicenseStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsExpired => DateTime.UtcNow > ValidTo;
        public bool IsExpiringSoon => DateTime.UtcNow.AddDays(30) > ValidTo && !IsExpired;
    }

    /// <summary>
    /// View model for license suspension
    /// </summary>
    public class LicenseSuspensionViewModel
    {
        public string LicenseId { get; set; } = string.Empty;
        public string LicensedTo { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Suspension Reason")]
        public string Reason { get; set; } = string.Empty;
        
        [Display(Name = "Additional Notes")]
        public string? Notes { get; set; }
        
        [DataType(DataType.Date)]
        [Display(Name = "Suspension End Date (Optional)")]
        public DateTime? SuspensionEndDate { get; set; }
        
        [Display(Name = "Notify License Holder")]
        public bool NotifyLicenseHolder { get; set; } = true;
    }
}
