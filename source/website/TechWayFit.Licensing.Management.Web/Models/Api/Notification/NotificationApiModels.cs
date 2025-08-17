using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Web.Models.Api.Notification;

public class CreateNotificationRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(1000)]
    public string Message { get; set; } = string.Empty;
    
    public NotificationType Type { get; set; } = NotificationType.Info;
    
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    
    public List<Guid>? RecipientIds { get; set; }
    
    public List<string>? RecipientRoles { get; set; }
    
    public DateTime? ScheduledDate { get; set; }
    
    public DateTime? ExpirationDate { get; set; }
    
    public Dictionary<string, string>? Metadata { get; set; }
}

public class NotificationResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; }
    public NotificationStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public DateTime? ReadDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? RecipientId { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class GetNotificationsRequest
{
    public NotificationType? Type { get; set; }
    public NotificationStatus? Status { get; set; }
    public NotificationPriority? Priority { get; set; }
    public bool? IsRead { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetNotificationsResponse
{
    public List<NotificationResponse> Notifications { get; set; } = new();
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class MarkNotificationRequest
{
    public List<Guid> NotificationIds { get; set; } = new();
    public bool IsRead { get; set; }
}

public class NotificationStatsResponse
{
    public int TotalNotifications { get; set; }
    public int UnreadNotifications { get; set; }
    public int TodayNotifications { get; set; }
    public int WeekNotifications { get; set; }
    public Dictionary<string, int> NotificationsByType { get; set; } = new();
    public Dictionary<string, int> NotificationsByPriority { get; set; } = new();
    public Dictionary<string, int> NotificationsByStatus { get; set; } = new();
}

public class NotificationTemplateResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsActive { get; set; }
    public List<string> Variables { get; set; } = new();
}

public class CreateNotificationTemplateRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;
    
    [Required]
    public string Body { get; set; } = string.Empty;
    
    public NotificationType Type { get; set; } = NotificationType.Info;
    
    public bool IsActive { get; set; } = true;
    
    public List<string>? Variables { get; set; }
}

public enum NotificationType
{
    Info,
    Warning,
    Error,
    Success,
    License,
    System,
    User,
    Product
}

public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Critical
}

public enum NotificationStatus
{
    Pending,
    Sent,
    Failed,
    Cancelled
}
