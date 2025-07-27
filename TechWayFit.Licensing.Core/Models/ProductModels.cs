namespace TechWayFit.Licensing.Core.Models
{
    /// <summary>
    /// Product configuration containing features, keys, and metadata
    /// </summary>
    public class ProductConfiguration
    {
        /// <summary>
        /// Unique product identifier
        /// </summary>
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// Product display name
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Product description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Product type enumeration
        /// </summary>
        public ProductType ProductType { get; set; }

        /// <summary>
        /// Product version
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// Available feature tiers for this product
        /// </summary>
        public Dictionary<LicenseTier, List<ProductFeatureDefinition>> FeatureTiers { get; set; } = new();

        /// <summary>
        /// Available standalone features
        /// </summary>
        public List<ProductFeatureDefinition> AvailableFeatures { get; set; } = new();

        /// <summary>
        /// Default limitations for this product
        /// </summary>
        public Dictionary<string, object> DefaultLimitations { get; set; } = new();

        /// <summary>
        /// Product-specific metadata
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new();

        /// <summary>
        /// Product-specific settings and configuration
        /// </summary>
        public Dictionary<string, object> ProductSettings { get; set; } = new();

        /// <summary>
        /// Available feature categories for this product
        /// </summary>
        public List<ProductFeatureCategory> FeatureCategories { get; set; } = new();

        /// <summary>
        /// Available tiers for this product
        /// </summary>
        public List<ProductTier> AvailableTiers { get; set; } = new();

        /// <summary>
        /// Whether this product is active for licensing
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// User who created this product
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Feature definition specific to a product
    /// </summary>
    public class ProductFeatureDefinition
    {
        /// <summary>
        /// Feature identifier
        /// </summary>
        public string FeatureId { get; set; } = string.Empty;

        /// <summary>
        /// Feature display name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Feature description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Feature category
        /// </summary>
        public FeatureCategory Category { get; set; }

        /// <summary>
        /// Whether this feature is enabled by default
        /// </summary>
        public bool IsEnabledByDefault { get; set; }

        /// <summary>
        /// Feature-specific limitations
        /// </summary>
        public Dictionary<string, object> Limitations { get; set; } = new();

        /// <summary>
        /// Dependencies on other features
        /// </summary>
        public List<string> Dependencies { get; set; } = new();

        /// <summary>
        /// Minimum tier required for this feature
        /// </summary>
        public LicenseTier MinimumTier { get; set; } = LicenseTier.Community;
    }

    /// <summary>
    /// Consumer information and licensing history
    /// </summary>
    public class Consumer
    {
        /// <summary>
        /// Unique consumer identifier
        /// </summary>
        public string ConsumerId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Consumer organization name
        /// </summary>
        public string OrganizationName { get; set; } = string.Empty;

        /// <summary>
        /// Primary contact person
        /// </summary>
        public string ContactPerson { get; set; } = string.Empty;

        /// <summary>
        /// Primary contact email
        /// </summary>
        public string ContactEmail { get; set; } = string.Empty;

        /// <summary>
        /// Secondary contact person
        /// </summary>
        public string? SecondaryContactPerson { get; set; }

        /// <summary>
        /// Secondary contact email
        /// </summary>
        public string? SecondaryContactEmail { get; set; }

        /// <summary>
        /// Consumer address
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Consumer phone number
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// License files associated with this consumer
        /// </summary>
        public List<ConsumerLicenseInfo> Licenses { get; set; } = new();

        /// <summary>
        /// Consumer-specific metadata
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new();

        /// <summary>
        /// Whether this consumer is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Information about a license file for a specific consumer
    /// </summary>
    public class ConsumerLicenseInfo
    {
        /// <summary>
        /// License identifier
        /// </summary>
        public string LicenseId { get; set; } = string.Empty;

        /// <summary>
        /// Product this license is for
        /// </summary>
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// Consumer ID this license belongs to
        /// </summary>
        public string ConsumerId { get; set; } = string.Empty;

        /// <summary>
        /// Path to the license file
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// License status
        /// </summary>
        public LicenseStatus Status { get; set; } = LicenseStatus.Pending;

        /// <summary>
        /// License tier
        /// </summary>
        public LicenseTier Tier { get; set; }

        /// <summary>
        /// Valid from date
        /// </summary>
        public DateTime ValidFrom { get; set; }

        /// <summary>
        /// Valid to date
        /// </summary>
        public DateTime ValidTo { get; set; }

        /// <summary>
        /// License creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Version of this license
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// Previous license ID if this is a renewal/update
        /// </summary>
        public string? PreviousLicenseId { get; set; }
    }

    /// <summary>
    /// Product tier configuration
    /// </summary>
    public class ProductTier
    {
        /// <summary>
        /// Tier identifier
        /// </summary>
        public string TierId { get; set; } = string.Empty;

        /// <summary>
        /// Tier name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tier description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// License tier enumeration
        /// </summary>
        public LicenseTier LicenseTier { get; set; }

        /// <summary>
        /// Product this tier belongs to
        /// </summary>
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// Tier-specific settings
        /// </summary>
        public Dictionary<string, object> Settings { get; set; } = new();

        /// <summary>
        /// Whether this tier is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Product feature category configuration
    /// </summary>
    public class ProductFeatureCategory
    {
        /// <summary>
        /// Category identifier
        /// </summary>
        public string CategoryId { get; set; } = string.Empty;

        /// <summary>
        /// Category name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Category description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Feature category enumeration
        /// </summary>
        public FeatureCategory Category { get; set; }

        /// <summary>
        /// Product this category belongs to
        /// </summary>
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// Display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Whether this category is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Category-specific metadata
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new();

        /// <summary>
        /// Creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
