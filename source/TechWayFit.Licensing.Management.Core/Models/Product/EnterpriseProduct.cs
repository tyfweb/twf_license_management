namespace TechWayFit.Licensing.Management.Core.Models.Product;

public class EnterpriseProduct
{
    /// <summary>
    /// Unique identifier for the product
    /// </summary>
    public Guid ProductId { get; set; } = Guid.NewGuid();

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


}
public enum ProductStatus
{
    PreRelease,
    Active,
    Inactive,
    Deprecated,
    Decommissioned
}
