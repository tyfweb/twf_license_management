using TechWayFit.Licensing.Management.Core.Helpers;
using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Core.Models.Product;

public class ProductFeature
{
    /// <summary>
    /// Unique identifier for the product feature
    /// </summary>
    public Guid FeatureId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tenant identifier for multi-tenant isolation
    /// </summary>
    public Guid TenantId { get; set; } = Guid.Empty;

    /// <summary>
    /// Audit information for the product feature
    /// </summary>
    public AuditInfo Audit { get; set; } = new();

    /// <summary>
    /// Workflow information for the product feature
    /// </summary>
    public WorkflowInfo Workflow { get; set; } = new();
    /// <summary>
    /// Unique identifier for the product this feature belongs to
    /// </summary>
    public Guid ProductId { get; set; } = Guid.Empty;
    
    /// <summary>
    /// Unique code for the product feature
    /// </summary>
    public string Code { get; set; } = string.Empty;

    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Name of the product feature
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the product feature
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the feature is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Usage limits for this feature
    /// </summary>
    public ProductFeatureUsage Usage { get; set; } = new();
    
    /// <summary>
    /// Foreign key to ProductVersion for minimum supported version
    /// </summary>
    public Guid? SupportFromVersionId { get; set; }

    /// <summary>
    /// Foreign key to ProductVersion for maximum supported version (nullable)
    /// </summary>
    public Guid? SupportToVersionId { get; set; }
    
    /// <summary>
    /// The version from which this feature is supported
    /// </summary>
    public SemanticVersion SupportFromVersion { get; set; } = SemanticVersion.Default;
    
    /// <summary>
    /// The version until which this feature is supported
    /// </summary>
    public SemanticVersion SupportToVersion { get; set; } = SemanticVersion.Max;
 
    /// <summary>
    /// Default product feature with basic functionality
    /// </summary>
    public static ProductFeature Default => new ProductFeature
    {
        FeatureId = IdConstants.DefaultFeatureId,
        Name = "Default Feature",
        Description = "Default product feature with basic functionality",
        IsEnabled = true,
        Usage = ProductFeatureUsage.NoLimit,  //Basic usage without limits
        SupportFromVersion = SemanticVersion.Default
    };
}
