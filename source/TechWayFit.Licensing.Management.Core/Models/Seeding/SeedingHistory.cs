using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Core.Models.Seeding;

/// <summary>
/// Tracks database seeding operations
/// </summary>
public class SeedingHistory
{
    /// <summary>
    /// Unique identifier for the seeding record
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Name of the seeder that was executed
    /// </summary>
    public string SeederName { get; set; } = string.Empty;

    /// <summary>
    /// Version of the seeder (for future migrations)
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// When the seeding was executed
    /// </summary>
    public DateTime ExecutedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Who executed the seeding (system/user)
    /// </summary>
    public string ExecutedBy { get; set; } = "System";

    /// <summary>
    /// Whether the seeding was successful
    /// </summary>
    public bool IsSuccessful { get; set; } = true;

    /// <summary>
    /// Any error message if seeding failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Additional metadata about the seeding operation
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Number of records created during seeding
    /// </summary>
    public int RecordsCreated { get; set; } = 0;

    /// <summary>
    /// Duration of the seeding operation in milliseconds
    /// </summary>
    public long DurationMs { get; set; } = 0;
}
