using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Core.Models.Product;

public class EnterpriseProduct : BaseAuditModel
{
    /// <summary>
    /// Unique identifier for the product
    /// </summary>
    public Guid ProductId { 
        get => Id; 
        set => Id = value; 
    }

    /// <summary>
    /// Name of the product
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the product
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Version of the product
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Release date of the product
    /// </summary>
    public DateTime ReleaseDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// List of product versions
    /// </summary>
    public IEnumerable<ProductVersion> Versions { get; set; } = new List<ProductVersion>();
    /// <summary>
    /// List of product tiers
    /// </summary>
    public IEnumerable<ProductTier> Tiers { get; set; } = new List<ProductTier>();
    /// <summary>
    /// Support contact email for the product
    /// </summary>
    public string SupportEmail { get; set; } = string.Empty;
    /// <summary>
    /// Support contact phone number for the product
    /// </summary>
    public string SupportPhone { get; set; } = string.Empty;
    /// <summary>
    /// Product decommission date, if applicable
    /// </summary>
    public DateTime? DecommissionDate { get; set; } = DateTime.MaxValue;
    /// <summary>
    /// Status of the product
    /// </summary>
    public ProductStatus Status { get; set; } = ProductStatus.Active;
    /// <summary>
    /// Gets or sets a collection of metadata key-value pairs associated with the object.
    /// </summary>
    /// <remarks>Use this property to store additional information about the object in a flexible, extensible
    /// manner. Keys are case-sensitive and must be unique within the dictionary.</remarks>
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();


}
public enum ProductStatus
{
    PreRelease,
    Active,
    Inactive,
    Deprecated,
    Decommissioned
}
