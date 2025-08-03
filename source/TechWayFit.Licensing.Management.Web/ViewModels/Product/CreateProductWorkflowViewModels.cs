using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Core.Models;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Product
{
    /// <summary>
    /// Multi-step product creation workflow models
    /// </summary>

    #region Step 1: Create Inactive Product

    /// <summary>
    /// Step 1: Basic product information (creates inactive product)
    /// </summary>
    public class CreateProductStep1ViewModel
    {
        [Required]
        [Display(Name = "Product ID")]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Product ID can only contain letters, numbers, underscores, and hyphens")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Product ID must be between 3 and 50 characters")]
        public string ProductId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Product Name")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 100 characters")]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Product Type")]
        public ProductType ProductType { get; set; }

        [Display(Name = "Description")]
        [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Category")]
        [StringLength(50, ErrorMessage = "Category cannot be longer than 50 characters")]
        public string Category { get; set; } = string.Empty;

        [Display(Name = "Target Audience")]
        [StringLength(200, ErrorMessage = "Target audience cannot be longer than 200 characters")]
        public string TargetAudience { get; set; } = string.Empty;

        // Metadata for additional product information
        public Dictionary<string, string> Metadata { get; set; } = new();

        // Workflow state
        public string WorkflowId { get; set; } = Guid.NewGuid().ToString();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
    }

    #endregion

    #region Step 2: Product Tiers Management

    /// <summary>
    /// Step 2: Product tiers configuration
    /// </summary>
    public class CreateProductStep2ViewModel
    {
        public string WorkflowId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;

        [Display(Name = "Available Tiers")]
        public List<ProductTierConfigurationViewModel> Tiers { get; set; } = new();

        [Display(Name = "Default Tier")]
        public LicenseTier DefaultTier { get; set; } = LicenseTier.Community;

        [Display(Name = "Allow Tier Upgrades")]
        public bool AllowTierUpgrades { get; set; } = true;

        [Display(Name = "Allow Tier Downgrades")]
        public bool AllowTierDowngrades { get; set; } = false;
    }

    /// <summary>
    /// Individual tier configuration
    /// </summary>
    public class ProductTierConfigurationViewModel
    {
        public string TierId { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [Display(Name = "Tier")]
        public LicenseTier Tier { get; set; }

        [Required]
        [Display(Name = "Tier Name")]
        [StringLength(50, ErrorMessage = "Tier name cannot be longer than 50 characters")]
        public string TierName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Display Order")]
        [Range(1, 100, ErrorMessage = "Display order must be between 1 and 100")]
        public int DisplayOrder { get; set; } = 1;

        [Display(Name = "Pricing Information")]
        public ProductTierPricingViewModel Pricing { get; set; } = new();

        [Display(Name = "Limitations")]
        public Dictionary<string, string> Limitations { get; set; } = new();

        [Display(Name = "Metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Tier pricing information
    /// </summary>
    public class ProductTierPricingViewModel
    {
        [Display(Name = "Currency")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be 3 characters (e.g., USD)")]
        public string Currency { get; set; } = "USD";

        [Display(Name = "Monthly Price")]
        [Range(0, 999999.99, ErrorMessage = "Monthly price must be between 0 and 999,999.99")]
        public decimal MonthlyPrice { get; set; } = 0;

        [Display(Name = "Annual Price")]
        [Range(0, 999999.99, ErrorMessage = "Annual price must be between 0 and 999,999.99")]
        public decimal AnnualPrice { get; set; } = 0;

        [Display(Name = "Is Free")]
        public bool IsFree { get; set; } = false;

        [Display(Name = "Trial Period (Days)")]
        [Range(0, 365, ErrorMessage = "Trial period must be between 0 and 365 days")]
        public int TrialPeriodDays { get; set; } = 0;
    }

    #endregion

    #region Step 3: Product Versions Management

    /// <summary>
    /// Step 3: Product versions configuration
    /// </summary>
    public class CreateProductStep3ViewModel
    {
        public string WorkflowId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;

        [Display(Name = "Product Versions")]
        public List<ProductVersionConfigurationViewModel> Versions { get; set; } = new();

        [Display(Name = "Default Version")]
        public string DefaultVersionId { get; set; } = string.Empty;

        [Display(Name = "Version Strategy")]
        public VersionStrategy VersionStrategy { get; set; } = VersionStrategy.SemanticVersioning;
    }

    /// <summary>
    /// Individual version configuration
    /// </summary>
    public class ProductVersionConfigurationViewModel
    {
        public string VersionId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Display(Name = "Version Number")]
        [StringLength(20, ErrorMessage = "Version number cannot be longer than 20 characters")]
        [RegularExpression(@"^\d+\.\d+(\.\d+)?(\.\d+)?(-[a-zA-Z0-9]+)?$", ErrorMessage = "Invalid version format (e.g., 1.0.0, 2.1.0-beta)")]
        public string VersionNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Version Name")]
        [StringLength(100, ErrorMessage = "Version name cannot be longer than 100 characters")]
        public string VersionName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Release Date")]
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Is Beta")]
        public bool IsBeta { get; set; } = false;

        [Display(Name = "Is Deprecated")]
        public bool IsDeprecated { get; set; } = false;

        [Display(Name = "Minimum System Requirements")]
        public ProductVersionRequirementsViewModel Requirements { get; set; } = new();

        [Display(Name = "Change Log")]
        public List<VersionChangeLogItemViewModel> ChangeLog { get; set; } = new();

        [Display(Name = "Compatible Tiers")]
        public List<LicenseTier> CompatibleTiers { get; set; } = new();
    }

    /// <summary>
    /// Version system requirements
    /// </summary>
    public class ProductVersionRequirementsViewModel
    {
        [Display(Name = "Operating System")]
        [StringLength(100)]
        public string OperatingSystem { get; set; } = string.Empty;

        [Display(Name = "Minimum RAM (GB)")]
        [Range(0, 1024)]
        public int MinimumRamGb { get; set; } = 1;

        [Display(Name = "Minimum Storage (GB)")]
        [Range(0, 10240)]
        public int MinimumStorageGb { get; set; } = 1;

        [Display(Name = "Required Dependencies")]
        public List<string> Dependencies { get; set; } = new();
    }

    /// <summary>
    /// Version change log item
    /// </summary>
    public class VersionChangeLogItemViewModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Display(Name = "Change Type")]
        public ChangeType ChangeType { get; set; }

        [Required]
        [Display(Name = "Description")]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Impact Level")]
        public ImpactLevel ImpactLevel { get; set; } = ImpactLevel.Minor;
    }

    #endregion

    #region Step 4: Product Features Management

    /// <summary>
    /// Step 4: Product features configuration
    /// </summary>
    public class CreateProductStep4ViewModel
    {
        public string WorkflowId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;

        [Display(Name = "Product Features")]
        public List<ProductFeatureConfigurationViewModel> Features { get; set; } = new();

        [Display(Name = "Feature Categories")]
        public List<FeatureCategoryViewModel> Categories { get; set; } = new();

        [Display(Name = "Feature Assignments by Tier")]
        public Dictionary<LicenseTier, List<string>> TierFeatureAssignments { get; set; } = new();
    }

    /// <summary>
    /// Individual feature configuration
    /// </summary>
    public class ProductFeatureConfigurationViewModel
    {
        public string FeatureId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Display(Name = "Feature Name")]
        [StringLength(100, ErrorMessage = "Feature name cannot be longer than 100 characters")]
        public string FeatureName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Category")]
        public FeatureCategory Category { get; set; }

        [Display(Name = "Is Core Feature")]
        public bool IsCoreFeature { get; set; } = false;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Minimum Tier")]
        public LicenseTier MinimumTier { get; set; } = LicenseTier.Community;

        [Display(Name = "Feature Dependencies")]
        public List<string> Dependencies { get; set; } = new();

        [Display(Name = "Tier-Specific Limitations")]
        public Dictionary<LicenseTier, FeatureLimitationViewModel> TierLimitations { get; set; } = new();

        [Display(Name = "Metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Feature category definition
    /// </summary>
    public class FeatureCategoryViewModel
    {
        public string CategoryId { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public FeatureCategory Category { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(300)]
        public string Description { get; set; } = string.Empty;

        public int DisplayOrder { get; set; } = 1;
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Feature limitation per tier
    /// </summary>
    public class FeatureLimitationViewModel
    {
        public bool IsEnabled { get; set; } = true;
        public Dictionary<string, object> Limitations { get; set; } = new();
        public string UsageQuota { get; set; } = string.Empty;
        public string RateLimits { get; set; } = string.Empty;
    }

    #endregion

    #region Step 5: Product Activation

    /// <summary>
    /// Step 5: Product activation and finalization
    /// </summary>
    public class CreateProductStep5ViewModel
    {
        public string WorkflowId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;

        // Summary of configuration
        public ProductConfigurationSummaryViewModel ConfigurationSummary { get; set; } = new();

        [Display(Name = "Activate Product")]
        public bool ActivateProduct { get; set; } = true;

        [Display(Name = "Launch Date")]
        [DataType(DataType.DateTime)]
        public DateTime LaunchDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Notification Settings")]
        public ProductNotificationSettingsViewModel NotificationSettings { get; set; } = new();

        [Display(Name = "Final Notes")]
        [StringLength(1000)]
        public string FinalNotes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Complete product configuration summary
    /// </summary>
    public class ProductConfigurationSummaryViewModel
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public ProductType ProductType { get; set; }
        public string Description { get; set; } = string.Empty;

        public int TotalTiers { get; set; }
        public int TotalVersions { get; set; }
        public int TotalFeatures { get; set; }

        public List<string> ConfiguredTiers { get; set; } = new();
        public List<string> ConfiguredVersions { get; set; } = new();
        public List<string> ConfiguredFeatures { get; set; } = new();

        public bool IsValid { get; set; } = false;
        public List<string> ValidationErrors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Product notification settings
    /// </summary>
    public class ProductNotificationSettingsViewModel
    {
        [Display(Name = "Notify on License Creation")]
        public bool NotifyOnLicenseCreation { get; set; } = true;

        [Display(Name = "Notify on License Expiration")]
        public bool NotifyOnLicenseExpiration { get; set; } = true;

        [Display(Name = "Notify on Version Updates")]
        public bool NotifyOnVersionUpdates { get; set; } = false;

        [Display(Name = "Admin Email")]
        [EmailAddress]
        public string AdminEmail { get; set; } = string.Empty;

        [Display(Name = "Notification Recipients")]
        public List<string> NotificationRecipients { get; set; } = new();
    }

    #endregion

    #region Supporting Enums

    /// <summary>
    /// Version strategy for product versioning
    /// </summary>
    public enum VersionStrategy
    {
        SemanticVersioning = 1,
        CalendarVersioning = 2,
        SequentialVersioning = 3,
        CustomVersioning = 4
    }

    /// <summary>
    /// Change type for version changelog
    /// </summary>
    public enum ChangeType
    {
        NewFeature = 1,
        Enhancement = 2,
        BugFix = 3,
        SecurityFix = 4,
        BreakingChange = 5,
        Deprecation = 6,
        Removal = 7
    }

    /// <summary>
    /// Impact level for changes
    /// </summary>
    public enum ImpactLevel
    {
        Minor = 1,
        Moderate = 2,
        Major = 3,
        Critical = 4
    }

    #endregion

    #region Workflow Management

    /// <summary>
    /// Complete workflow state container
    /// </summary>
    public class CreateProductWorkflowViewModel
    {
        public string WorkflowId { get; set; } = Guid.NewGuid().ToString();
        public int CurrentStep { get; set; } = 1;
        public int TotalSteps { get; set; } = 5;

        public CreateProductStep1ViewModel Step1 { get; set; } = new();
        public CreateProductStep2ViewModel Step2 { get; set; } = new();
        public CreateProductStep3ViewModel Step3 { get; set; } = new();
        public CreateProductStep4ViewModel Step4 { get; set; } = new();
        public CreateProductStep5ViewModel Step5 { get; set; } = new();

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;

        public bool IsStepCompleted(int step)
        {
            return step switch
            {
                1 => !string.IsNullOrEmpty(Step1.ProductId) && !string.IsNullOrEmpty(Step1.ProductName),
                2 => Step2.Tiers.Any(t => t.IsActive),
                3 => Step3.Versions.Any(v => v.IsActive),
                4 => Step4.Features.Any(f => f.IsActive),
                5 => Step5.ConfigurationSummary.IsValid,
                _ => false
            };
        }

        public bool CanProceedToStep(int step)
        {
            return step switch
            {
                1 => true,
                2 => IsStepCompleted(1),
                3 => IsStepCompleted(1) && IsStepCompleted(2),
                4 => IsStepCompleted(1) && IsStepCompleted(2) && IsStepCompleted(3),
                5 => IsStepCompleted(1) && IsStepCompleted(2) && IsStepCompleted(3) && IsStepCompleted(4),
                _ => false
            };
        }

        public double CompletionPercentage => (CurrentStep / (double)TotalSteps) * 100;
    }

    #endregion
}
