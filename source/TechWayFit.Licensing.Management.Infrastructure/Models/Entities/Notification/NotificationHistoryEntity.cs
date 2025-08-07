using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Core.Helpers;
using TechWayFit.Licensing.Management.Core.Models.Notification;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Common;
using TechWayFit.Licensing.Management.Infrastructure.Helpers;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Notification;

/// <summary>
/// Database entity for Notification History
/// </summary>
[Table("notification_history")]
public class NotificationHistoryEntity : AuditEntity, IEntityMapper<NotificationHistory, NotificationHistoryEntity>
{
    public Guid EntityId { get; set; } = Guid.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string NotificationMode { get; set; } = string.Empty;
    public Guid NotificationTemplateId { get; set; } = Guid.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public string RecipientsJson { get; set; } = "{}";
    public DateTime SentDate { get; set; }
    public string? Status { get; set; }
    public string? DeliveryError { get; set; }

    /// <summary>
    /// Navigation property to Notification Template
    /// </summary>
    public virtual NotificationTemplateEntity? Template { get; set; }

    #region IEntityMapper Implementation


    public NotificationHistoryEntity Map(NotificationHistory model)
    {
        return new NotificationHistoryEntity
        {
            Id = model.NotificationId,
            EntityId = model.EntityId,
            EntityType = model.EntityType,
            NotificationMode = model.NotificationMode.ToString(),
            NotificationTemplateId = model.NotificationTemplateId,
            NotificationType = model.NotificationType.ToString(),
            RecipientsJson = JsonHelper.ToJson(model.Recipients),
            SentDate = model.SentDate,
            Status = model.Status.ToString(),
            DeliveryError = model.DeliveryError,
            CreatedBy = "system", // Assuming system user for creation
            CreatedOn = DateTime.UtcNow,
            UpdatedBy = "system", // Assuming system user for creation
            UpdatedOn = DateTime.UtcNow // Assuming UpdatedOn is set to current time on creation
        };
    }
    public NotificationHistory Map()
    {
        return new NotificationHistory
        {
            NotificationId = Id,
            EntityId = EntityId,
            EntityType = EntityType,
            NotificationMode = NotificationMode.ToEnum<NotificationMode>(),
            NotificationTemplateId = NotificationTemplateId,
            NotificationType = NotificationType.ToEnum<NotificationType>(),
            Recipients = JsonHelper.FromJson<NotificationPreferences>(RecipientsJson) ?? new NotificationPreferences(),
            SentDate = SentDate,
            Status = Status?.ToEnum<DeliveryStatus>() ?? DeliveryStatus.Unknown,
            DeliveryError = DeliveryError
        };
    }
    #endregion
}
