namespace TechWayFit.Licensing.Management.Core.Models.Product;

public class ProductFeatureUsage
{
    /// <summary>
    /// Maximum usage limit for this feature
    /// </summary>
    public int MaxUsage { get; set; } = 0;
    /// <summary>
    /// Maximum Concurrent usage count for this feature
    /// </summary>
    public int MaxConcurrentUsage { get; set; } = 0;
    /// <summary>
    /// Indicates if the feature usage is unlimited 
    /// </summary>
    public bool IsUnlimited { get; set; } = false;
    /// <summary>
    /// Indicates if the feature has a grace period
    /// </summary>
    public bool AllowGracePeriod { get; set; } = false;
    /// <summary>
    /// Feature Expiration date, if applicable
    /// </summary>
    public DateTime? ExpirationDate { get; set; } = null;

    public static ProductFeatureUsage Default => new ProductFeatureUsage
    {
        MaxUsage = 1000,
        MaxConcurrentUsage = 10,
        IsUnlimited = false,
        AllowGracePeriod = false,
        ExpirationDate = null
    };
    public static ProductFeatureUsage NoLimit => new ProductFeatureUsage
    {
        MaxUsage = 0,
        MaxConcurrentUsage = 0,
        IsUnlimited = true,
        AllowGracePeriod = false,
        ExpirationDate = null
    };

}
