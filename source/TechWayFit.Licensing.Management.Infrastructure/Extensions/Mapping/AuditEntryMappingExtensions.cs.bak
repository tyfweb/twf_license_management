using System.Text.Json;
using TechWayFit.Licensing.Management.Core.Models.Audit;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Audit;

namespace TechWayFit.Licensing.Management.Infrastructure.Extensions.Mapping;

/// <summary>
/// Extension methods for mapping between AuditEntry core model and AuditEntryEntity
/// </summary>
public static class AuditEntryMappingExtensions
{
    /// <summary>
    /// Converts AuditEntry core model to AuditEntryEntity
    /// </summary>
    public static AuditEntryEntity ToEntity(this AuditEntry model)
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
    public static AuditEntry ToModel(this AuditEntryEntity entity)
    {
        if (entity == null) return null!;

        var metadata = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(entity.Metadata))
        {
            try
            {
                metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(entity.Metadata) ?? new Dictionary<string, string>();
            }
            catch (JsonException)
            {
                metadata = new Dictionary<string, string>();
            }
        }

        return new AuditEntry
        {
            EntryId = entity.Id,
            EntityType = entity.EntityType,
            EntityId = entity.EntityId,
            ActionType = entity.ActionType,
            OldValue = entity.OldValue,
            NewValue = entity.NewValue,
            IpAddress = entity.IpAddress,
            UserAgent = entity.UserAgent,
            Reason = entity.Reason,
            Metadata = metadata,
            UserName = entity.CreatedBy,
            Timestamp = entity.CreatedOn
        };
    }

    /// <summary>
    /// Updates existing AuditEntryEntity with values from AuditEntry core model
    /// </summary>
    public static void UpdateFromModel(this AuditEntryEntity entity, AuditEntry model)
    {
        if (entity == null || model == null) return;

        entity.EntityType = model.EntityType;
        entity.EntityId = model.EntityId;
        entity.ActionType = model.ActionType;
        entity.OldValue = model.OldValue;
        entity.NewValue = model.NewValue;
        entity.IpAddress = model.IpAddress;
        entity.UserAgent = model.UserAgent;
        entity.Reason = model.Reason;
        entity.Metadata = model.Metadata?.Count > 0 ? JsonSerializer.Serialize(model.Metadata) : null;
        entity.CreatedBy = model.UserName;
        entity.CreatedOn = model.Timestamp;
    }
}
