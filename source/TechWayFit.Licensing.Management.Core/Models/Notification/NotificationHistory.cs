namespace TechWayFit.Licensing.Management.Core.Models.Notification;

/// <summary>
/// Notification history entry
/// </summary>
public class NotificationHistory
{
    public string NotificationId { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public NotificationMode NotificationMode { get; set; }
    public string NotificationTemplateId { get; set; } = string.Empty;
    public NotificationType NotificationType { get; set; }
    public NotificationPreferences Recipients { get; set; } = new NotificationPreferences();
    public DateTime SentDate { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; }
    public string? DeliveryError { get; set; }
}
public enum DeliveryStatus
{
    Pending,
    Sent,
    Failed
}
