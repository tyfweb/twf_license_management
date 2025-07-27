namespace TechWayFit.Licensing.Management.Core.Models.Product;

public class ProductSupportSLA
{
    /// <summary>
    /// Unique identifier for the support SLA
    /// </summary>
    public string SlaId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the support SLA
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the support SLA
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Response time for critical issues
    /// </summary>
    public TimeSpan CriticalResponseTime { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Response time for high priority issues
    /// </summary>
    public TimeSpan HighPriorityResponseTime { get; set; } = TimeSpan.FromHours(4);

    /// <summary>
    /// Response time for medium priority issues
    /// </summary>
    public TimeSpan MediumPriorityResponseTime { get; set; } = TimeSpan.FromHours(8);

    /// <summary>
    /// Response time for low priority issues
    /// </summary>
    public TimeSpan LowPriorityResponseTime { get; set; } = TimeSpan.FromDays(1);


    public static ProductSupportSLA NoSLA => new ProductSupportSLA
    {
        SlaId = "no-sla",
        Name = "No SLA",
        Description = "This product does not include any support SLA.",
        CriticalResponseTime = TimeSpan.MaxValue,
        HighPriorityResponseTime = TimeSpan.MaxValue,
        MediumPriorityResponseTime = TimeSpan.MaxValue,
        LowPriorityResponseTime = TimeSpan.MaxValue
    };
}
