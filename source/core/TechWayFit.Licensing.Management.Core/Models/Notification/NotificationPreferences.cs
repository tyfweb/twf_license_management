namespace TechWayFit.Licensing.Management.Core.Models.Notification;

/// <summary>
/// Notification preferences
/// </summary>
public class NotificationPreferences
{
    public NotificationMode Mode { get; set; } = NotificationMode.Email; 
}
public enum NotificationMode
{
    Email,
    Sms,
    AppNotification
}
