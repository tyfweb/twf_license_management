namespace TechWayFit.Licensing.Management.Core.Models.Audit;

/// <summary>
/// Audit statistics
/// </summary>
public class AuditStatistics
{
    public int TotalEntries { get; set; }
    public Dictionary<string, int> EntriesByAction { get; set; } = new();
    public Dictionary<string, int> EntriesByEntity { get; set; } = new();
    public Dictionary<string, int> EntriesByUser { get; set; } = new();
    public Dictionary<DateTime, int> EntriesByDate { get; set; } = new();
    public int UniqueUsers { get; set; }
    public int UniqueEntities { get; set; }
}
