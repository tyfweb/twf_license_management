using System.Text.Json;
using TechWayFit.Licensing.Management.Core.Models.Seeding;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Seeding;

/// <summary>
/// Database entity for SeedingHistory
/// </summary>
public class SeedingHistoryEntity : BaseEntity,IEntityMapper<SeedingHistory, SeedingHistoryEntity>
{

    /// <summary>
    /// Name of the seeder
    /// </summary>
    public string SeederName { get; set; } = string.Empty;

    /// <summary>
    /// Version of the seeder
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// When the seeding was executed
    /// </summary>
    public DateTime ExecutedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Who executed the seeding
    /// </summary>
    public string ExecutedBy { get; set; } = "System";

    /// <summary>
    /// Whether the seeding was successful
    /// </summary>
    public bool IsSuccessful { get; set; } = true;

    /// <summary>
    /// Error message if seeding failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Metadata as JSON string
    /// </summary>
    public string MetadataJson { get; set; } = "{}";

    /// <summary>
    /// Number of records created
    /// </summary>
    public int RecordsCreated { get; set; } = 0;

    /// <summary>
    /// Duration in milliseconds
    /// </summary>
    public long DurationMs { get; set; } = 0;

    #region IEntityMapper Implementation

    public SeedingHistoryEntity Map(SeedingHistory model)
    {
        if (model == null) return null!;

        Id = model.Id;
        SeederName = model.SeederName;
        Version = model.Version;
        ExecutedOn = model.ExecutedOn;
        ExecutedBy = model.ExecutedBy;
        IsSuccessful = model.IsSuccessful;
        ErrorMessage = model.ErrorMessage;
        MetadataJson = model.Metadata?.Count > 0 
            ? JsonSerializer.Serialize(model.Metadata) 
            : "{}";
        RecordsCreated = model.RecordsCreated;
        DurationMs = model.DurationMs;

        return this;
    }

    public SeedingHistory Map()
    {
        return new SeedingHistory
        {
            Id = this.Id,
            SeederName = this.SeederName,
            Version = this.Version,
            ExecutedOn = this.ExecutedOn,
            ExecutedBy = this.ExecutedBy,
            IsSuccessful = this.IsSuccessful,
            ErrorMessage = this.ErrorMessage,
            Metadata = !string.IsNullOrEmpty(this.MetadataJson) && this.MetadataJson != "{}"
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(this.MetadataJson) ?? new Dictionary<string, object>()
                : new Dictionary<string, object>(),
            RecordsCreated = this.RecordsCreated,
            DurationMs = this.DurationMs
        };
    }

    #endregion
}
