using TechWayFit.Licensing.Infrastructure.Models.Entities.License;
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Infrastructure.Models.Entities.Products;

/// <summary>
/// Database entity for ProductFeature
/// </summary>
public class ProductFeatureEntity : BaseAuditEntity
{
    public string ProductId { get; set; } = string.Empty;
    /// <summary>
    /// Unique identifier for the product feature
    /// </summary>
    public string FeatureId { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to Product Tier
    /// </summary>
    public string TierId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the feature
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the feature
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Feature code or identifier
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Whether this feature is enabled by default
    /// </summary>
    public bool IsEnabled { get; set; } = true;
 

    /// <summary>
    /// Display order for this feature
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    public string SupportFromVersion { get; set; } = "1.0.0"; // Default version, adjust as needed
    public string SupportToVersion { get; set; } = "9999.0.0"; // Default to no end version
    public string FeatureUsageJson { get; set; } = "{}"; // Assuming usage is stored as JSON

    /// <summary>
    /// Navigation property to Product Tier
    /// </summary>
    public virtual ProductTierEntity? Tier { get; set; }

    /// <summary>
    /// Navigation property to License Features
    /// </summary>
    public virtual ICollection<ProductLicenseEntity> ProductLicenses { get; set; } = new List<ProductLicenseEntity>();

    public static ProductFeatureEntity FromModel(ProductFeature model)
    {
        return new ProductFeatureEntity
        {
            ProductId = model.ProductId,
            FeatureId = model.FeatureId,
            TierId = model.TierId,
            Name = model.Name,
            Description = model.Description,
            Code = model.Code,
            IsEnabled = model.IsEnabled,
            DisplayOrder = model.DisplayOrder,
            SupportFromVersion = model.SupportFromVersion,
            SupportToVersion = model.SupportToVersion,
            FeatureUsageJson = ToJson(model.Usage), // Assuming Usage is serialized to JSON
            CreatedBy = "system", // Assuming system created, adjust as needed
            CreatedOn = DateTime.UtcNow,
            UpdatedBy = "system", // Assuming system updated, adjust as needed
            UpdatedOn = DateTime.UtcNow
        };
    }
    public ProductFeature ToModel()
    {
        return new ProductFeature
        {
            FeatureId = FeatureId,
            TierId = TierId,
            Name = Name,
            Description = Description,
            Code = Code,
            IsEnabled = IsEnabled,
            DisplayOrder = DisplayOrder,
            SupportFromVersion = SupportFromVersion,
            SupportToVersion = SupportToVersion,
            Usage = FromJson<ProductFeatureUsage>(FeatureUsageJson), // Assuming Usage is deserialized
            ProductId = Tier?.ProductId ?? string.Empty // Assuming ProductId is derived from Tier
        };
    }
}
