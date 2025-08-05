using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Management.Web.ViewModels.Shared;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Product
{
    /// <summary>
    /// Product view model for list items
    /// </summary>
    public class ProductViewModel
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public ProductType ProductType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public int TotalLicenses { get; set; }
        public int ActiveLicenses { get; set; }
    }

    /// <summary>
    /// Product listing view model
    /// </summary>
    public class ProductListViewModel
    {
        public List<ProductItemViewModel> Products { get; set; } = new();
        public string SearchTerm { get; set; } = string.Empty;
        public bool ShowInactiveProducts { get; set; }
    }

    /// <summary>
    /// Individual product item for listing
    /// </summary>
    public class ProductItemViewModel
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public ProductType ProductType { get; set; }
        public string Version { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int LicenseCount { get; set; }
        public int ConsumerCount { get; set; }
        public int TierCount { get; set; } = 3; // Default to 3 tiers
        public int FeatureCount { get; set; } = 12; // Default to 12 features
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }    /// <summary>
    /// Product details view model
    /// </summary>
    public class ProductDetailViewModel
    {
        public ProductConfiguration Product { get; set; } = new();
        public List<ConsumerSummaryViewModel> Consumers { get; set; } = new();
        public List<LicenseSummaryViewModel> RecentLicenses { get; set; } = new();
        public ProductStatisticsViewModel Statistics { get; set; } = new();
        public List<StatsTileViewModel> StatsTiles { get; set; } = new();
    }

    /// <summary>
    /// Consumer summary for product view
    /// </summary>
    public class ConsumerSummaryViewModel
    {
        public string ConsumerId { get; set; } = string.Empty;
        public string OrganizationName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public int LicenseCount { get; set; }
        public DateTime LastLicenseDate { get; set; }
        public LicenseStatus? LastLicenseStatus { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// License summary for product view
    /// </summary>
    public class LicenseSummaryViewModel
    {
        public string LicenseId { get; set; } = string.Empty;
        public string ConsumerName { get; set; } = string.Empty;
        public LicenseTier Tier { get; set; }
        public LicenseStatus Status { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public int DaysUntilExpiry { get; set; }
        public bool IsExpiringSoon => DaysUntilExpiry <= 30 && DaysUntilExpiry >= 0;
    }

    /// <summary>
    /// Product statistics
    /// </summary>
    public class ProductStatisticsViewModel
    {
        public int TotalLicenses { get; set; }
        public int ActiveLicenses { get; set; }
        public int ExpiredLicenses { get; set; }
        public int SuspendedLicenses { get; set; }
        public int RevokedLicenses { get; set; }
        public int LicensesExpiringSoon { get; set; }
        public Dictionary<LicenseTier, int> LicensesByTier { get; set; } = new();
        public Dictionary<string, int> FeatureUsage { get; set; } = new();
    }
    public class ProductCreateViewModel
    {       
        [Required]
        [Display(Name = "Product Name")]
        [StringLength(100, ErrorMessage = "Product name cannot be longer than 100 characters")]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Product Type")]
        public ProductType ProductType { get; set; }

        [Display(Name = "Version")]
        [StringLength(20, ErrorMessage = "Version cannot be longer than 20 characters")]
        public string Version { get; set; } = "1.0";

        [Display(Name = "Description")]
        [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Release Date")]
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Support Email")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string SupportEmail { get; set; } = string.Empty;

        [Display(Name = "Support Phone")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string SupportPhone { get; set; } = string.Empty;

        [Display(Name = "Feature Tiers")]
        public Dictionary<LicenseTier, List<FeatureDefinitionViewModel>> FeatureTiers { get; set; } = new();

        [Display(Name = "Available Features")]
        public List<FeatureDefinitionViewModel> AvailableFeatures { get; set; } = new();

        [Display(Name = "Default Limitations")]
        public Dictionary<string, string> DefaultLimitations { get; set; } = new();

        [Display(Name = "Metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new();

        [Display(Name = "Created Date")]
        public DateTime? CreatedDate { get; set; }
 
    }
    /// <summary>
    /// Create/Edit product view model
    /// </summary>
    public class ProductEditViewModel
    {
        [Required]
        [Display(Name = "Product ID")]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Product ID can only contain letters, numbers, underscores, and hyphens")]
        public string ProductId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Product Name")]
        [StringLength(100, ErrorMessage = "Product name cannot be longer than 100 characters")]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Product Type")]
        public ProductType ProductType { get; set; }

        [Display(Name = "Version")]
        [StringLength(20, ErrorMessage = "Version cannot be longer than 20 characters")]
        public string Version { get; set; } = "1.0";

        [Display(Name = "Description")]
        [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Release Date")]
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Support Email")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string SupportEmail { get; set; } = string.Empty;

        [Display(Name = "Support Phone")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string SupportPhone { get; set; } = string.Empty;

        [Display(Name = "Feature Tiers")]
        public Dictionary<LicenseTier, List<FeatureDefinitionViewModel>> FeatureTiers { get; set; } = new();

        [Display(Name = "Available Features")]
        public List<FeatureDefinitionViewModel> AvailableFeatures { get; set; } = new();

        [Display(Name = "Default Limitations")]
        public Dictionary<string, string> DefaultLimitations { get; set; } = new();

        [Display(Name = "Metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new();

        [Display(Name = "Created Date")]
        public DateTime? CreatedDate { get; set; }

        public bool IsEditMode => !string.IsNullOrWhiteSpace(ProductId);
    }

    /// <summary>
    /// Feature definition view model
    /// </summary>
    public class FeatureDefinitionViewModel
    {
        [Required]
        public string FeatureId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public FeatureCategory Category { get; set; }
        public bool IsEnabledByDefault { get; set; }
        public LicenseTier MinimumTier { get; set; } = LicenseTier.Community;
        public Dictionary<string, string> Limitations { get; set; } = new();
        public List<string> Dependencies { get; set; } = new();
    }

    /// <summary>
    /// Enhanced Product Management View Model
    /// </summary>
    public class ProductManagementViewModel
    {
        public Guid ProductId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string ProductName { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public ProductType ProductType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Management sections
        public List<ProductTierViewModel> ProductTiers { get; set; } = new();
        public List<ProductVersionViewModel> ProductVersions { get; set; } = new();
        public List<ProductFeatureViewModel> ProductFeatures { get; set; } = new();
    }

    /// <summary>
    /// Product Tier View Model
    /// </summary>
    public class ProductTierViewModel
    {
        public Guid? TierId { get; set; }
        [Required]
        public Guid ProductId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string TierName { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;
        
        public LicenseTier Tier { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal? MonthlyPrice { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal? AnnualPrice { get; set; }
        
        public bool IsFree { get; set; }
        
        [Range(0, 365)]
        public int TrialPeriodDays { get; set; }
        
        [Range(1, int.MaxValue)]
        public int? MaxUsers { get; set; }
        
        [Range(1, int.MaxValue)]
        public int? MaxProjects { get; set; }
        
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }

    /// <summary>
    /// Product Version View Model
    /// </summary>
    public class ProductVersionViewModel
    {
        public Guid VersionId { get; set; }
        public Guid ProductId { get; set; }
        
        [Required]
        [StringLength(20)]
        public string VersionNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string VersionName { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public DateTime ReleaseDate { get; set; } = DateTime.UtcNow;
        public bool IsStable { get; set; } = true;
        public bool IsBeta { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public bool IsApproved { get; set; } = false;
        
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime? EndOfLifeDate { get; internal set; }
    }

    /// <summary>
    /// Product Feature View Model
    /// </summary>
    public class ProductFeatureViewModel
    {
        public Guid FeatureId { get; set; }
        public Guid ProductId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public FeatureCategory Category { get; set; }
        public bool IsEnabled { get; set; } = true;
        
        [Range(1, int.MaxValue)]
        public int? MaxUsage { get; set; }
        
        public string? ConfigurationSchema { get; set; }
        public LicenseTier MinimumTier { get; set; } = LicenseTier.Community;
        
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }    /// <summary>
    /// Enhanced Product Edit View Model with tabbed sections
    /// </summary>
    public class ProductEnhancedEditViewModel
    {
        // Basic Product Information
        public Guid ProductId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string ProductName { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public ProductType ProductType { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Version { get; set; } = "1.0.0";
        
        public bool IsActive { get; set; } = true;
        
        // Section indicators for UI
        public bool HasTiers { get; set; } = false;
        public bool HasVersions { get; set; } = false;
        public bool HasFeatures { get; set; } = false;
        
        // Tab state management
        public string ActiveTab { get; set; } = "basic";
        public string ActiveSection { get; set; } = "basic";

        // Stats tiles for reusable components
        public List<StatsTileViewModel> StatsTiles { get; set; } = new();
    }
}
