using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Core.Models;

namespace TechWayFit.Licensing.WebUI.Models
{
    /// <summary>
    /// View model for license generation form
    /// </summary>
   public class LicenseGenerationViewModel
    {
        [Required]
        [Display(Name = "Licensed To")]
        public string LicensedTo { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Contact Email")]
        public string ContactEmail { get; set; } = string.Empty;

        [Display(Name = "Secondary Contact Person")]
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
        public LicenseTier Tier { get; set; } = LicenseTier.Community;

        public List<string> SelectedFeatures { get; set; } = new();

        // Add this property to help with feature display
        public Dictionary<string, FeatureInfo> AvailableFeatures => GetAvailableFeatures();

        private static Dictionary<string, FeatureInfo> GetAvailableFeatures()
        {
            return new Dictionary<string, FeatureInfo>
            {
                ["BasicApiGateway"] = new("Basic API Gateway", "Basic API Gateway functionality", LicenseTier.Community),
                ["BasicAuthentication"] = new("Basic Authentication", "JWT and API Key authentication", LicenseTier.Community),
                ["BasicRateLimiting"] = new("Basic Rate Limiting", "Simple rate limiting", LicenseTier.Community),
                ["AdvancedRateLimiting"] = new("Advanced Rate Limiting", "Advanced rate limiting with multiple strategies", LicenseTier.Professional),
                ["LoadBalancing"] = new("Load Balancing", "Load balancing capabilities", LicenseTier.Professional),
                ["AdvancedMonitoring"] = new("Advanced Monitoring", "Comprehensive monitoring and analytics", LicenseTier.Enterprise),
                ["EnterpriseSupport"] = new("Enterprise Support", "24/7 enterprise support", LicenseTier.Enterprise)
            };
        }
    }


    /// <summary>
    /// Information about a feature
    /// </summary>
    public class FeatureInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public LicenseTier MinimumTier { get; set; }

        public FeatureInfo(string name, string description, LicenseTier minimumTier)
        {
            Name = name;
            Description = description;
            MinimumTier = minimumTier;
        }
    }

    /// <summary>
    /// View model for license generation result
    /// </summary>
    public class LicenseGenerationResultViewModel
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public License? License { get; set; }
        public SignedLicense? SignedLicense { get; set; }
        public string LicenseJson { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }

    /// <summary>
    /// View model for license validation
    /// </summary>
    public class LicenseValidationViewModel
    {
        [Required(ErrorMessage = "License JSON is required")]
        [Display(Name = "License JSON")]
        public string LicenseJson { get; set; } = string.Empty;

        public bool? IsValid { get; set; }
        public string ValidationMessage { get; set; } = string.Empty;
        public License? License { get; set; }
    }

    /// <summary>
    /// View model for key management
    /// </summary>
    public class KeyManagementViewModel
    {
        public bool HasCurrentKeys { get; set; }
        public string CurrentPublicKey { get; set; } = string.Empty;
        public string? PrivateKeyFileName { get; set; }
        public string? PrivateKeyPassword { get; set; }
        public bool PasswordProtected { get; set; }
    }
}
