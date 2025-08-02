using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Core.Models.Audit;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Audit;

/// <summary>
/// Database entity for Audit Entries
/// </summary>
[Table("audit_entries")]
public class AuditEntryEntity : BaseAuditEntity
{
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; } = Guid.NewGuid();
    public string ActionType { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Reason { get; set; }
    public string? Metadata { get; set; } = string.Empty;

    public static AuditEntryEntity FromModel(AuditEntry model)
    {
        return new AuditEntryEntity
        {
            Id = model.EntityId,
            EntityType = model.EntityType,
            EntityId = model.EntityId,
            ActionType = model.ActionType,
            OldValue = model.OldValue,
            NewValue = model.NewValue,
            IpAddress = model.IpAddress,
            UserAgent = model.UserAgent,
            Reason = model.Reason,
            Metadata = ToJson(model.Metadata),
            CreatedBy = model.UserName,
            CreatedOn = model.Timestamp
        };
    }    public AuditEntry ToModel()
    {
        return new AuditEntry
        {
            EntryId = Id,
            EntityType = EntityType,
            EntityId = EntityId,
            ActionType = ActionType,
            OldValue = OldValue,
            NewValue = NewValue,
            IpAddress = IpAddress,
            UserAgent = UserAgent,
            Reason = Reason,
            Metadata = FromDictJson(Metadata),
            UserName = CreatedBy,
            Timestamp = CreatedOn
        };
    }
}
