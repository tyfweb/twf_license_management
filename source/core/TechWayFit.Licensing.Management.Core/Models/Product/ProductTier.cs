using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Core.Models.Product;

public class ProductTier
{
    public Guid ProductId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Unique identifier for the product tier
    /// </summary>
    public Guid TierId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tenant identifier for multi-tenant isolation
    /// </summary>
    public Guid TenantId { get; set; } = Guid.Empty;

    /// <summary>
    /// Audit information for the product tier
    /// </summary>
    public AuditInfo Audit { get; set; } = new();

    /// <summary>
    /// Workflow information for the product tier
    /// </summary>
    public WorkflowInfo Workflow { get; set; } = new();

    /// <summary>
    /// Display order for sorting tiers
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Name of the product tier
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the product tier
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Support SLA information for this tier
    /// </summary>
    public ProductSupportSLA SupportSLA { get; set; } = ProductSupportSLA.NoSLA;

    /// <summary>
    /// Features included in this tier
    /// </summary>
    public List<ProductFeature> Features { get; set; } = new();

    /// <summary>
    /// Pricing options for this tier (Monthly, Yearly, Perpetual)
    /// </summary>
    public List<ProductTierPrice> Prices { get; set; } = new();

    public ProductTier()
    {
        Features.Add(ProductFeature.Default);
        MaxUsers = 1;
        MaxDevices = 1;
        Audit.IsActive = true;
    }

    static Guid DefaultTierId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static ProductTier Default => new ProductTier
    {
        TierId = DefaultTierId,
        Name = "Default Tier",
        Description = "Default product tier with basic features"
    };
    
    /// <summary>
    /// Maximum number of users allowed in this tier
    /// </summary>
    public int MaxUsers { get; set; }
    /// <summary>
    /// Maximum number of devices allowed in this tier
    /// </summary>
    public int MaxDevices { get; set; }
}
public class Money
{
    static string[] ValidCurrencies = { "USD", "EUR", "GBP", "SGD", "INR" };
    public decimal Amount { get; set; } = 0.0m;
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    public static implicit operator Money(string amount)
    {
        if (string.IsNullOrWhiteSpace(amount))
            return new Money();
        if (amount.Length > 3 && ValidCurrencies.Contains(amount.Substring(0, 3)))
        {
            var value = amount.Substring(3).Trim();
            if (decimal.TryParse(value, out var parsedAmount))
            {
                return new Money { Amount = parsedAmount, Currency = amount.Substring(0, 3) };
            }
        }
        throw new FormatException("Invalid money format");
    }

    public static implicit operator string(Money money)
    {
        return $"{money.Currency} {money.Amount:0.00}";
    }

    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Cannot add amounts with different currencies");
        return new Money { Amount = a.Amount + b.Amount, Currency = a.Currency };
    }
    public static Money operator -(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Cannot subtract amounts with different currencies");
        return new Money { Amount = a.Amount - b.Amount, Currency = a.Currency };
    }
    public override string ToString() => $"{this.Currency} {this.Amount:0.00}";
    public static Money Parse(string input)
    {
        var parts = input.Split(' ');
        if (parts.Length != 2 || !decimal.TryParse(parts[0], out var amount))
            throw new FormatException("Invalid money format");
        return new Money { Amount = amount, Currency = parts[1] };
    }
    public static Money operator *(Money a, decimal multiplier)
    {
        return new Money { Amount = a.Amount * multiplier, Currency = a.Currency };
    }
    
    public static Money Zero => new Money { Amount = 0.0m, Currency = "USD" };
}
