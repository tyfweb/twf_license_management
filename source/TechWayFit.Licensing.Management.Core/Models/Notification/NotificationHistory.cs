namespace TechWayFit.Licensing.Management.Core.Models.Notification;

/// <summary>
/// Notification history entry
/// </summary>
public class NotificationHistory
{
    public Guid NotificationId { get; set; } = Guid.NewGuid();
    public Guid EntityId { get; set; } = Guid.NewGuid();
    public string EntityType { get; set; } = string.Empty;
    public NotificationMode NotificationMode { get; set; }
    public Guid NotificationTemplateId { get; set; } = Guid.Empty;
    public NotificationType NotificationType { get; set; }
    public NotificationPreferences Recipients { get; set; } = new NotificationPreferences();
    public DateTime SentDate { get; set; }
    public DeliveryStatus Status { get; set; }
    public string? DeliveryError { get; set; }
}
public enum DeliveryStatus
{
    Unknown,
    Pending,
    Sent,
    Failed
}
