using TechWayFit.Licensing.Management.Core.Helpers;
using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Core.Models.Product;

public class ProductFeature
{
    
    /// <summary>
    /// Unique identifier for the product feature
    /// </summary>
    public Guid FeatureId { get; set; } = Guid.Empty;
    /// <summary>
    /// Unique identifier for the product feature
    /// </summary>
    public Guid ProductId { get; set; } = Guid.Empty;
    /// <summary>
    /// Unique identifier for the product feature
    /// </summary>
    public Guid TierId { get; set; } = Guid.Empty;
    /// <summary>
    /// Unique identifier for the product feature
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
