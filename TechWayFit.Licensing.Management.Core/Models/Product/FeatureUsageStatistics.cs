namespace TechWayFit.Licensing.Management.Core.Models.Product;

/// <summary>
/// Feature usage statistics
/// </summary>
public class FeatureUsageStatistics
{
    public int TotalFeatures { get; set; }
    public Dictionary<string, int> FeaturesByType { get; set; } = new();
    public Dictionary<string, int> MostUsedFeatures { get; set; } = new();
    public Dictionary<string, int> FeaturesByTier { get; set; } = new();
}
