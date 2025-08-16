namespace TechWayFit.Licensing.Management.Web.Configuration
{
    /// <summary>
    /// Configuration for product related settings
    /// </summary>
    public class ProductConfiguration
    {
        public const string SectionName = "ProductConfiguration";
        
        public List<Currency> SupportedCurrencies { get; set; } = new();
        public List<SLATemplate> SLATemplates { get; set; } = new();
        public ProductFeatures ProductFeatures { get; set; } = new();
    }

    /// <summary>
    /// Configuration for product tier related settings
    /// </summary>
    public class ProductTierConfiguration
    {
        public const string SectionName = "ProductTierConfiguration";
        
        public List<SLATemplate> PreConfiguredSLATemplates { get; set; } = new();
    }

    /// <summary>
    /// Supported currency definition
    /// </summary>
    public class Currency
    {
        public string Code { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }

    /// <summary>
    /// Pre-configured SLA template for quick tier setup
    /// </summary>
    public class SLATemplate
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double CriticalResponseHours { get; set; }
        public double HighPriorityResponseHours { get; set; }
        public double MediumPriorityResponseHours { get; set; }
        public double LowPriorityResponseHours { get; set; }
    }

    /// <summary>
    /// Product features configuration
    /// </summary>
    public class ProductFeatures
    {
        public DisplaySettings DisplaySettings { get; set; } = new();
        public List<FeatureCategory> Categories { get; set; } = new();
    }

    /// <summary>
    /// Display settings for product features
    /// </summary>
    public class DisplaySettings
    {
        public bool UseListView { get; set; } = true;
        public bool UseGridView { get; set; } = false;
        public int ItemsPerPage { get; set; } = 10;
        public bool ShowDescriptions { get; set; } = true;
        public bool ShowIcons { get; set; } = true;
    }

    /// <summary>
    /// Feature category definition
    /// </summary>
    public class FeatureCategory
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }
}
