namespace TechWayFit.Licensing.Management.Core.Models.Notification;

/// <summary>
/// Notification statistics
/// </summary>
public class NotificationStatistics
{
    public int TotalNotifications { get; set; }
    public int SuccessfulDeliveries { get; set; }
    public int FailedDeliveries { get; set; }
    public Dictionary<NotificationType, int> NotificationsByType { get; set; } = new();
    public Dictionary<DateTime, int> NotificationsByDate { get; set; } = new();
    public double DeliverySuccessRate { get; set; }
}
