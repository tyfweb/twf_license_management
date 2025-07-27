using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Core.Helpers;
using TechWayFit.Licensing.Management.Core.Models.Notification;

namespace TechWayFit.Licensing.Infrastructure.Models.Entities.Notification;

/// <summary>
/// Database entity for Notification History
/// </summary>
[Table("notification_history")]
public class NotificationHistoryEntity : BaseAuditEntity
{
    public string NotificationId { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string NotificationMode { get; set; } = string.Empty;
    public string NotificationTemplateId { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public string RecipientsJson { get; set; } = "{}";
    public DateTime SentDate { get; set; }
    public string DeliveryStatus { get; set; }
    public string? DeliveryError { get; set; }

    /// <summary>
    /// Navigation property to Notification Template
    /// </summary>
    public virtual NotificationTemplateEntity? Template { get; set; }

    public static NotificationHistoryEntity FromModel(NotificationHistory model)
    {
        return new NotificationHistoryEntity
        {
            NotificationId = model.NotificationId,
            EntityId = model.EntityId,
            EntityType = model.EntityType,
            NotificationMode = model.NotificationMode.ToString(),
            NotificationTemplateId = model.NotificationTemplateId,
            NotificationType = model.NotificationType.ToString(),
            RecipientsJson = ToJson(model.Recipients),
            SentDate = model.SentDate,
            DeliveryStatus = model.DeliveryStatus.ToString(),
            DeliveryError = model.DeliveryError,
            CreatedBy = "system", // Assuming system user for creation
            CreatedOn = DateTime.UtcNow,
            UpdatedBy = "system", // Assuming system user for creation
            UpdatedOn = DateTime.UtcNow // Assuming UpdatedOn is set to current time on creation
        };
    }
    public NotificationHistory ToModel()
    {
        return new NotificationHistory
        {
            NotificationId = NotificationId,
            EntityId = EntityId,
            EntityType = EntityType,
            NotificationMode =  NotificationMode.ToEnum<NotificationMode>(),
            NotificationTemplateId = NotificationTemplateId,
            NotificationType = NotificationType.ToEnum<NotificationType>(),
            Recipients = FromJson<NotificationPreferences>(RecipientsJson),
            SentDate = SentDate,
            DeliveryStatus = DeliveryStatus.ToEnum<DeliveryStatus>(),
            DeliveryError = DeliveryError
        };
    }
}
