using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Core.Helpers;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.License;
using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;

/// <summary>
/// Database entity for EnterpriseProduct
/// </summary>
[Table("products")]
public class ProductEntity : BaseAuditEntity
{

    /// <summary>
    /// Name of the product
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the product
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Release date of the product
    /// </summary>
    public DateTime ReleaseDate { get; set; } = DateTime.UtcNow;

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
    public DateTime? DecommissionDate { get; set; }

    /// <summary>
    /// Status of the product
    /// </summary>
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Navigation property to Product Versions
    /// </summary>
    public virtual ICollection<ProductVersionEntity> Versions { get; set; } = new List<ProductVersionEntity>();

    /// <summary>
    /// Navigation property to Product Tiers
    /// </summary>
    public virtual ICollection<ProductTierEntity> Tiers { get; set; } = new List<ProductTierEntity>();

    /// <summary>
    /// Navigation property to Product Licenses
    /// </summary>
    public virtual ICollection<ProductLicenseEntity> Licenses { get; set; } = new List<ProductLicenseEntity>();

    public virtual ICollection<ProductConsumerEntity> ProductConsumers { get; set; } = new List<ProductConsumerEntity>();

    public static ProductEntity FromModel(EnterpriseProduct model)
    {
        return new ProductEntity
        {
            Id = model.ProductId,
            Name = model.Name,
            Description = model.Description,
            ReleaseDate = model.ReleaseDate,
            SupportEmail = model.SupportEmail,
            SupportPhone = model.SupportPhone,
            DecommissionDate = model.DecommissionDate,
            Status = model.Status.ToString()
        };
    }
    public EnterpriseProduct ToModel()
    {
        return new EnterpriseProduct
        {
            ProductId = this.Id,
            Name = this.Name,
            Description = this.Description,
            ReleaseDate = this.ReleaseDate,
            SupportEmail = this.SupportEmail,
            SupportPhone = this.SupportPhone,
            DecommissionDate = this.DecommissionDate,
            Status = this.Status.ToEnum<ProductStatus>()
        };
    }
}
