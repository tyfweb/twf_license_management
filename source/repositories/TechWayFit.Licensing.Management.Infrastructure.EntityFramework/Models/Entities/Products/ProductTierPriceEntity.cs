using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Common;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Products;

/// <summary>
/// Database entity for ProductTierPrice
/// </summary>
[Table("product_tier_prices")]
public class ProductTierPriceEntity : AuditEntity, IEntityMapper<ProductTierPrice, ProductTierPriceEntity>
{

    /// <summary>
    /// Product identifier
    /// </summary>
    [Required]
    public Guid ProductId { get; set; }

    /// <summary>
    /// Tier identifier
    /// </summary>
    [Required]
    public Guid TierId { get; set; }

    /// <summary>
    /// Price amount stored as decimal
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal PriceAmount { get; set; }

    /// <summary>
    /// Currency code (ISO 4217) stored as separate field
    /// </summary>
    [Required]
    [StringLength(3)]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Type of pricing (Monthly, Yearly, Perpetual)
    /// </summary>
    [Required]
    public int PriceType { get; set; }

    /// <summary>
    /// Navigation property to ProductTier
    /// </summary>
    public virtual ProductTierEntity? ProductTier { get; set; }

    /// <summary>
    /// Navigation property to Product
    /// </summary>
    public virtual ProductEntity? Product { get; set; }

    #region IEntityMapper Implementation

    /// <summary>
    /// Maps from domain model to entity
    /// </summary>
    public ProductTierPriceEntity Map(ProductTierPrice model)
    {
        if (model == null) return null!;

        Id = model.PriceId;
        ProductId = model.ProductId;
        TierId = model.TierId;
        PriceAmount = model.Price.Amount;
        Currency = model.Price.Currency;
        PriceType = (int)model.PriceType;
        IsActive = model.IsActive;
        CreatedBy = "System"; // You might want to get this from context
        CreatedOn = model.CreatedAt;
        UpdatedBy = "System"; // You might want to get this from context
        UpdatedOn = model.UpdatedAt;

        return this;
    }

    /// <summary>
    /// Maps from entity to domain model
    /// </summary>
    public ProductTierPrice Map()
    {
        return new ProductTierPrice
        {
            PriceId = this.Id,
            ProductId = this.ProductId,
            TierId = this.TierId,
            Price = new Money { Amount = this.PriceAmount, Currency = this.Currency },
            PriceType = (TechWayFit.Licensing.Management.Core.Models.Product.PriceType)this.PriceType,
            IsActive = this.IsActive,
            CreatedAt = this.CreatedOn,
            UpdatedAt = this.UpdatedOn ?? this.CreatedOn
        };
    }

    #endregion
}
