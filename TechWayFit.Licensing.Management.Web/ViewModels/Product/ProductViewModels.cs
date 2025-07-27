using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Core.Models;

namespace TechWayFit.Licensing.WebUI.ViewModels.Product
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
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Product details view model
    /// </summary>
    public class ProductDetailViewModel
    {
        public ProductConfiguration Product { get; set; } = new();
        public List<ConsumerSummaryViewModel> Consumers { get; set; } = new();
        public List<LicenseSummaryViewModel> RecentLicenses { get; set; } = new();
        public ProductStatisticsViewModel Statistics { get; set; } = new();
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

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Feature Tiers")]
        public Dictionary<LicenseTier, List<FeatureDefinitionViewModel>> FeatureTiers { get; set; } = new();

        [Display(Name = "Available Features")]
        public List<FeatureDefinitionViewModel> AvailableFeatures { get; set; } = new();

        [Display(Name = "Default Limitations")]
        public Dictionary<string, string> DefaultLimitations { get; set; } = new();

        [Display(Name = "Metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new();

        public bool IsEditMode => !string.IsNullOrEmpty(ProductId);
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
}
