using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Management.Core.Models.Notification;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Notification
{
    /// <summary>
    /// Notification listing view model
    /// </summary>
    public class NotificationListViewModel
    {
        public List<NotificationItemViewModel> Notifications { get; set; } = new();
        public NotificationFilterViewModel Filter { get; set; } = new();
        public PaginationViewModel Pagination { get; set; } = new();
        public NotificationStatisticsViewModel Statistics { get; set; } = new();
    }

    /// <summary>
    /// Individual notification item
    /// </summary>
    public class NotificationItemViewModel
    {
        public string NotificationId { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public NotificationMode NotificationMode { get; set; }
        public string NotificationTemplateId { get; set; } = string.Empty;
        public NotificationType NotificationType { get; set; }
        public NotificationPreferences Recipients { get; set; } = new();
        public DateTime SentDate { get; set; }
        public DeliveryStatus DeliveryStatus { get; set; }
        public string? DeliveryError { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string RecipientDisplay { get; set; } = string.Empty;
        
        public string StatusCssClass => GetStatusCssClass();
        public string TypeDisplayName => GetTypeDisplayName();
        public string ModeDisplayName => GetModeDisplayName();

        private string GetStatusCssClass()
        {
            return DeliveryStatus switch
            {
                DeliveryStatus.Sent => "badge-success",
                DeliveryStatus.Failed => "badge-danger",
                DeliveryStatus.Pending => "badge-warning",
                _ => "badge-secondary"
            };
        }

        private string GetTypeDisplayName()
        {
            return NotificationType switch
            {
                NotificationType.LicenseExpiration => "License Expiration",
                NotificationType.LicenseActivation => "License Activation",
                NotificationType.LicenseRevocation => "License Revocation",
                NotificationType.LicenseRenewal => "License Renewal",
                NotificationType.LicenseSuspension => "License Suspension",
                NotificationType.ValidationFailure => "Validation Failure",
                NotificationType.UsageThreshold => "Usage Threshold",
                NotificationType.SystemAlert => "System Alert",
                NotificationType.Custom => "Custom",
                _ => NotificationType.ToString()
            };
        }

        private string GetModeDisplayName()
        {
            return NotificationMode switch
            {
                NotificationMode.Email => "Email",
                NotificationMode.Sms => "SMS",
                NotificationMode.AppNotification => "App Notification",
                _ => NotificationMode.ToString()
            };
        }
    }

    /// <summary>
    /// Notification filter options
    /// </summary>
    public class NotificationFilterViewModel
    {
        public string SearchTerm { get; set; } = string.Empty;
        public NotificationType? NotificationType { get; set; }
        public NotificationMode? NotificationMode { get; set; }
        public DeliveryStatus? DeliveryStatus { get; set; }
        public DateTime? SentFromDate { get; set; }
        public DateTime? SentToDate { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
    }

    /// <summary>
    /// Notification statistics view model
    /// </summary>
    public class NotificationStatisticsViewModel
    {
        public int TotalNotifications { get; set; }
        public int SuccessfulDeliveries { get; set; }
        public int FailedDeliveries { get; set; }
        public int PendingDeliveries { get; set; }
        public double DeliverySuccessRate { get; set; }
        public Dictionary<NotificationType, int> NotificationsByType { get; set; } = new();
        public Dictionary<NotificationMode, int> NotificationsByMode { get; set; } = new();
        public Dictionary<DateTime, int> NotificationsByDate { get; set; } = new();
        public Dictionary<string, int> NotificationsByEntity { get; set; } = new();
    }

    /// <summary>
    /// Notification template listing view model
    /// </summary>
    public class NotificationTemplateListViewModel
    {
        public List<NotificationTemplateItemViewModel> Templates { get; set; } = new();
        public string SearchTerm { get; set; } = string.Empty;
        public NotificationType? FilterType { get; set; }
        public bool ShowInactiveTemplates { get; set; }
    }    /// <summary>
    /// Individual notification template item
    /// </summary>
    public class NotificationTemplateItemViewModel
    {
        public string TemplateId { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        public NotificationType NotificationType { get; set; }
        public NotificationMode NotificationMode { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string MessageTemplate { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public int UsageCount { get; set; }
        public DateTime? LastUsed { get; set; }

        public string TypeDisplayName => GetTypeDisplayName();
        public string ModeDisplayName => GetModeDisplayName();
        public string StatusCssClass => IsActive ? "badge-success" : "badge-secondary";

        private string GetTypeDisplayName()
        {
            return NotificationType switch
            {
                NotificationType.LicenseExpiration => "License Expiration",
                NotificationType.LicenseActivation => "License Activation",
                NotificationType.LicenseRevocation => "License Revocation",
                NotificationType.LicenseRenewal => "License Renewal",
                NotificationType.LicenseSuspension => "License Suspension",
                NotificationType.ValidationFailure => "Validation Failure",
                NotificationType.UsageThreshold => "Usage Threshold",
                NotificationType.SystemAlert => "System Alert",
                NotificationType.Custom => "Custom",
                _ => NotificationType.ToString()
            };
        }

        private string GetModeDisplayName()
        {
            return NotificationMode switch
            {
                NotificationMode.Email => "Email",
                NotificationMode.Sms => "SMS",
                NotificationMode.AppNotification => "App",
                _ => NotificationMode.ToString()
            };
        }
    }

    /// <summary>
    /// Notification template details view model
    /// </summary>
    public class NotificationTemplateDetailViewModel
    {
        public string TemplateId { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        public NotificationType NotificationType { get; set; }
        public NotificationPreferences Preferences { get; set; } = new();
        public string Subject { get; set; } = string.Empty;
        public string MessageTemplate { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public Dictionary<string, object> TemplateVariables { get; set; } = new();
        public List<NotificationItemViewModel> RecentUsage { get; set; } = new();
        public NotificationTemplateStatisticsViewModel Statistics { get; set; } = new();
    }

    /// <summary>
    /// Notification template create/edit view model
    /// </summary>
    public class NotificationTemplateEditViewModel
    {
        public string TemplateId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Template Name")]
        [StringLength(100, MinimumLength = 3)]
        public string TemplateName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Notification Type")]
        public NotificationType NotificationType { get; set; }

        [Required]
        [Display(Name = "Notification Mode")]
        public NotificationMode NotificationMode { get; set; } = NotificationMode.Email;

        [Required]
        [Display(Name = "Subject")]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Message Template")]
        public string MessageTemplate { get; set; } = string.Empty;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Template Variables")]
        public Dictionary<string, string> TemplateVariables { get; set; } = new();

        public List<string> AvailableVariables { get; set; } = new();
        public string PreviewRecipient { get; set; } = string.Empty;
        public string RenderedPreview { get; set; } = string.Empty;
    }

    /// <summary>
    /// Notification template statistics
    /// </summary>
    public class NotificationTemplateStatisticsViewModel
    {
        public int TotalUsage { get; set; }
        public int SuccessfulDeliveries { get; set; }
        public int FailedDeliveries { get; set; }       
        public DateTime? LastUsed { get; set; }
        public double SuccessRate { get; set; }
        public TimeSpan? AverageDeliveryTime { get; set; }
        public Dictionary<DateTime, int> UsageByDate { get; set; } = new();
        public Dictionary<DeliveryStatus, int> DeliveriesByStatus { get; set; } = new();
    }

    /// <summary>
    /// Send custom notification view model
    /// </summary>
    public class SendNotificationViewModel
    {
        [Required]
        [Display(Name = "Recipients")]
        public List<string> Recipients { get; set; } = new();

        [Required]
        [Display(Name = "Subject")]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Message")]
        public string Message { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Notification Type")]
        public NotificationType NotificationType { get; set; } = NotificationType.Custom;

        [Required]
        [Display(Name = "Notification Mode")]
        public NotificationMode NotificationMode { get; set; } = NotificationMode.Email;

        [Display(Name = "Template")]
        public Guid? TemplateId { get; set; }

        [Display(Name = "Schedule Send")]
        public DateTime? ScheduledSendTime { get; set; }

        [Display(Name = "Additional Metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new();

        public List<NotificationTemplateItemViewModel> AvailableTemplates { get; set; } = new();
        public List<string> SuggestedRecipients { get; set; } = new();
    }

    /// <summary>
    /// Dashboard view model for notifications
    /// </summary>
    public class NotificationDashboardViewModel
    {
        public NotificationStatisticsViewModel Statistics { get; set; } = new();
        public List<NotificationItemViewModel> RecentNotifications { get; set; } = new();
        public List<NotificationTemplateItemViewModel> PopularTemplates { get; set; } = new();
        public Dictionary<string, int> AlertsByProduct { get; set; } = new();
        public Dictionary<string, int> AlertsByConsumer { get; set; } = new();
        public List<SystemAlertViewModel> SystemAlerts { get; set; } = new();
        public NotificationTrendsViewModel Trends { get; set; } = new();
    }

    /// <summary>
    /// System alert view model
    /// </summary>
    public class SystemAlertViewModel
    {
        public string AlertId { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }
        public string? ActionRequired { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();

        public string SeverityBadgeClass => Severity.ToLower() switch
        {
            "critical" => "badge-danger",
            "high" => "badge-warning",
            "medium" => "badge-info",
            "low" => "badge-secondary",
            _ => "badge-secondary"
        };
    }

    /// <summary>
    /// Notification trends view model
    /// </summary>
    public class NotificationTrendsViewModel
    {
        public Dictionary<DateTime, int> DeliveryTrends { get; set; } = new();
        public Dictionary<NotificationType, int> TypeTrends { get; set; } = new();
        public Dictionary<string, double> SuccessRateTrends { get; set; } = new();
        public int TotalGrowthPercentage { get; set; }
        public int SuccessRateChange { get; set; }
        public string TrendDirection { get; set; } = "stable";
    }

    /// <summary>
    /// Pagination view model
    /// </summary>
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; } = 25;
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int StartItem => ((CurrentPage - 1) * PageSize) + 1;
        public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);
    }

    /// <summary>
    /// Notification delivery test view model
    /// </summary>
    public class NotificationTestViewModel
    {
        [Required]
        [Display(Name = "Template")]
        public string TemplateId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Test Recipient")]
        [EmailAddress]
        public string TestRecipient { get; set; } = string.Empty;

        [Display(Name = "Test Data")]
        public Dictionary<string, string> TestData { get; set; } = new();

        public List<NotificationTemplateItemViewModel> AvailableTemplates { get; set; } = new();
        public string RenderedPreview { get; set; } = string.Empty;
        public bool TestSent { get; set; }
        public string? TestResult { get; set; }
    }
}
