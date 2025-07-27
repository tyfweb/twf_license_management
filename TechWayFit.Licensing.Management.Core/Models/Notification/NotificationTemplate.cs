namespace TechWayFit.Licensing.Management.Core.Models.Notification;

/// <summary>
/// Notification template
/// </summary>
public class NotificationTemplate
{
    public string TemplateId { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public NotificationType NotificationType { get; set; }
    public NotificationPreferences Preferences { get; set; } = new NotificationPreferences();
    public string Subject { get; set; } = string.Empty;
    public string MessageTemplate { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } 
    public Dictionary<string, object> TemplateVariables { get; set; } = new();
}
