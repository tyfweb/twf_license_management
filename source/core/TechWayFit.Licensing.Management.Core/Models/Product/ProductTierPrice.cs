using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Core.Models.Product;

/// <summary>
/// Represents pricing information for a product tier
/// </summary>
public class ProductTierPrice
{
    /// <summary>
    /// Unique identifier for the price entry
    /// </summary>
    public Guid PriceId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Product identifier
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Tier identifier
    /// </summary>
    public Guid TierId { get; set; }

    /// <summary>
    /// Price using the Money class
    /// </summary>
    public Money Price { get; set; } = Money.Zero;

    /// <summary>
    /// Type of pricing (Monthly, Yearly, Perpetual)
    /// </summary>
    [Required]
    public PriceType PriceType { get; set; }

    /// <summary>
    /// Whether this price is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When this price was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this price was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether this is the default price for the tier
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Navigation property to ProductTier
    /// </summary>
    public virtual ProductTier? ProductTier { get; set; }

    /// <summary>
    /// Gets price formatted for display
    /// </summary>
    public string PriceDisplay => PriceType switch
    {
        PriceType.Monthly => $"{Price.Currency} {Price.Amount:F2}/month",
        PriceType.Yearly => $"{Price.Currency} {Price.Amount:F2}/year",
        PriceType.Perpetual => $"{Price.Currency} {Price.Amount:F2} (one-time)",
        _ => $"{Price.Currency} {Price.Amount:F2}"
    };

    /// <summary>
    /// Gets price type display text
    /// </summary>
    public string PriceTypeDisplay => PriceType switch
    {
        PriceType.Monthly => "Monthly",
        PriceType.Yearly => "Yearly", 
        PriceType.Perpetual => "One-time",
        _ => "Unknown"
    };

    /// <summary>
    /// Creates a new ProductTierPrice instance
    /// </summary>
    public ProductTierPrice()
    {
        IsActive = true;
    }

    /// <summary>
    /// Creates a new ProductTierPrice with specified values
    /// </summary>
    public static ProductTierPrice Create(Guid productId, Guid tierId, Money price, PriceType priceType, bool isDefault = false)
    {
        return new ProductTierPrice
        {
            ProductId = productId,
            TierId = tierId,
            Price = price,
            PriceType = priceType,
            IsDefault = isDefault,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Types of pricing models
/// </summary>
public enum PriceType
{
    /// <summary>
    /// Monthly recurring billing
    /// </summary>
    Monthly = 1,

    /// <summary>
    /// Yearly recurring billing
    /// </summary>
    Yearly = 2,

    /// <summary>
    /// One-time perpetual license
    /// </summary>
    Perpetual = 3
}
