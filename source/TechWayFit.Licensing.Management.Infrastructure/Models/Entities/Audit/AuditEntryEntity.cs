using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Core.Models.Audit;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Common;
using TechWayFit.Licensing.Management.Infrastructure.Helpers;
using System.Text.Json;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Audit;

/// <summary>
/// Database entity for Audit Entries
/// </summary>
[Table("audit_entries")]
public class AuditEntryEntity : AuditEntity, IEntityMapper<AuditEntry, AuditEntryEntity>
{
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Reason { get; set; }
    public string? Metadata { get; set; } = string.Empty;

    #region IEntityMapper Implementation
    public AuditEntryEntity Map(AuditEntry model)
    {
        if (model == null) return null!;

        return new AuditEntryEntity
        {
            Id = model.EntryId,
            EntityType = model.EntityType,
            EntityId = model.EntityId,
            ActionType = model.ActionType,
            OldValue = model.OldValue,
            NewValue = model.NewValue,
            IpAddress = model.IpAddress,
            UserAgent = model.UserAgent,
            Reason = model.Reason,
            Metadata = model.Metadata?.Count > 0 ? JsonSerializer.Serialize(model.Metadata) : null,
            CreatedBy = model.UserName,
            CreatedOn = model.Timestamp,
            IsActive = true
        };
    }

    /// <summary>
    /// Converts AuditEntryEntity to AuditEntry core model
    /// </summary>
    public AuditEntry Map()
    { 

        var metadata = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(this.Metadata))
        {
            try
            {
                metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(this.Metadata) ?? new Dictionary<string, string>();
            }
            catch (JsonException)
            {
                metadata = new Dictionary<string, string>();
            }
        }

        return new AuditEntry
        {
            EntryId = this.Id,
            EntityType = this.EntityType,
            EntityId = this.EntityId,
            ActionType = this.ActionType,
            OldValue = this.OldValue,
            NewValue = this.NewValue,
            IpAddress = this.IpAddress,
            UserAgent = this.UserAgent,
            Reason = this.Reason,
            Metadata = metadata,
            UserName = this.CreatedBy,
            Timestamp = this.CreatedOn
        };
    }
    #endregion
}
