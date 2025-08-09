using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Management.Core.Models.Notification;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Notification
{
    /// <summary>
    /// Enum for notification status (matches core models)
    /// </summary>
    public enum NotificationStatus
    {
        Pending,
        Delivered,
        Failed
    }

    /// <summary>
    /// ViewModel for NotificationTemplate operations
    /// </summary>
    public class NotificationTemplateViewModel
    {
        public Guid TemplateId { get; set; }
        public Guid TenantId { get; set; }

        [Required(ErrorMessage = "Template name is required")]
        [StringLength(200, ErrorMessage = "Template name cannot exceed 200 characters")]
        [Display(Name = "Template Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Notification type is required")]
        [Display(Name = "Notification Type")]
        public NotificationType Type { get; set; }

        [Required(ErrorMessage = "Subject is required")]
        [StringLength(300, ErrorMessage = "Subject cannot exceed 300 characters")]
        [Display(Name = "Subject")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Body template is required")]
        [Display(Name = "Body Template")]
        public string BodyTemplate { get; set; } = string.Empty;

        [Display(Name = "Is HTML")]
        public bool IsHtml { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created Date")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; } = string.Empty;

        [Display(Name = "Updated Date")]
        public DateTime? UpdatedOn { get; set; }

        [Display(Name = "Updated By")]
        public string? UpdatedBy { get; set; }

        // Available placeholders for the template
        public List<string> AvailablePlaceholders { get; set; } = new();
    }

    /// <summary>
    /// ViewModel for NotificationHistory operations
    /// </summary>
    public class NotificationHistoryViewModel
    {
        public Guid NotificationId { get; set; }
        public Guid TenantId { get; set; }
        public Guid? TemplateId { get; set; }

        [Display(Name = "Recipient")]
        public string Recipient { get; set; } = string.Empty;

        [Display(Name = "Subject")]
        public string Subject { get; set; } = string.Empty;

        [Display(Name = "Body")]
        public string Body { get; set; } = string.Empty;

        [Display(Name = "Notification Type")]
        public NotificationType Type { get; set; }

        [Display(Name = "Notification Mode")]
        public NotificationMode Mode { get; set; }

        [Display(Name = "Status")]
        public NotificationStatus Status { get; set; }

        [Display(Name = "Sent Date")]
        public DateTime? SentDate { get; set; }

        [Display(Name = "Delivered Date")]
        public DateTime? DeliveredDate { get; set; }

        [Display(Name = "Read Date")]
        public DateTime? ReadDate { get; set; }

        [Display(Name = "Failed Date")]
        public DateTime? FailedDate { get; set; }

        [Display(Name = "Error Message")]
        public string? ErrorMessage { get; set; }

        [Display(Name = "Retry Count")]
        public int RetryCount { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Template Name")]
        public string? TemplateName { get; set; }

        [Display(Name = "Is Success")]
        public bool IsSuccess => Status == NotificationStatus.Delivered;

        [Display(Name = "Is Failed")]
        public bool IsFailed => Status == NotificationStatus.Failed;

        [Display(Name = "Is Pending")]
        public bool IsPending => Status == NotificationStatus.Pending;
    }

    /// <summary>
    /// ViewModel for user notification preferences
    /// </summary>
    public class UserNotificationPreferencesViewModel
    {
        public Guid PreferenceId { get; set; }
        public Guid TenantId { get; set; }
        public Guid UserId { get; set; }

        [Display(Name = "Email Notifications")]
        public bool EmailEnabled { get; set; } = true;

        [Display(Name = "SMS Notifications")]
        public bool SmsEnabled { get; set; }

        [Display(Name = "License Expiry Notifications")]
        public bool LicenseExpiryEnabled { get; set; } = true;

        [Display(Name = "System Maintenance Notifications")]
        public bool SystemMaintenanceEnabled { get; set; } = true;

        [Display(Name = "Security Alert Notifications")]
        public bool SecurityAlertEnabled { get; set; } = true;

        [Display(Name = "Marketing Notifications")]
        public bool MarketingEnabled { get; set; }

        [Range(1, 90, ErrorMessage = "Notification days must be between 1 and 90")]
        [Display(Name = "License Expiry Notice Days")]
        public int LicenseExpiryNoticeDays { get; set; } = 30;

        [Display(Name = "Quiet Hours Start")]
        public TimeSpan? QuietHoursStart { get; set; }

        [Display(Name = "Quiet Hours End")]
        public TimeSpan? QuietHoursEnd { get; set; }

        [StringLength(10, ErrorMessage = "Timezone cannot exceed 10 characters")]
        [Display(Name = "Timezone")]
        public string? Timezone { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created Date")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Updated Date")]
        public DateTime? UpdatedOn { get; set; }

        // Related data
        [Display(Name = "User Name")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "User Email")]
        public string UserEmail { get; set; } = string.Empty;
    }
}
