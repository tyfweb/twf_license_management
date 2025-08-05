using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Management.Core.Models.Consumer;

public class ProductConsumer
{
    /// <summary>
    /// Unique identifier for the product consumer relationship
    /// </summary>
    public Guid ProductConsumerId { get; set; } = Guid.NewGuid();
    /// <summary>
    /// Consumer account associated with this product
    /// </summary>
    public ConsumerAccount Consumer { get; set; } = new();
    /// <summary>
    /// Product associated with this consumer
    /// </summary>
    public EnterpriseProduct Product { get; set; } = new();
    /// <summary>
    /// Product Tier associated with this consumer
    /// </summary>
    public ContactPerson AccountManager { get; set; } = new();
    /// <summary>
    /// Features included in this license
    /// </summary>
    public IEnumerable<ProductFeature> Features { get; set; } = new List<ProductFeature>();
    /// <summary>
    /// Foreign key to Product Tier (optional - for tier-based licensing)
    /// </summary>
    public ProductTier ProductTier { get; set; } = new();

}
