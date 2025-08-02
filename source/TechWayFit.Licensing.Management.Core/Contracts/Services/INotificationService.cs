using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Core.Models.Notification;

namespace TechWayFit.Licensing.Management.Core.Contracts.Services;

/// <summary>
/// Service interface for license notifications and alerts
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends license expiration notification
    /// </summary>
    /// <param name="license">License that is expiring</param>
    /// <param name="daysUntilExpiry">Days until expiry</param>
    /// <returns>True if notification sent successfully</returns>
    Task<bool> SendLicenseExpirationNotificationAsync(ProductLicense license, int daysUntilExpiry);

    /// <summary>
    /// Sends license activation notification
    /// </summary>
    /// <param name="license">Activated license</param>
    /// <param name="activationInfo">Activation information</param>
    /// <returns>True if notification sent successfully</returns>
    Task<bool> SendLicenseActivationNotificationAsync(ProductLicense license, Dictionary<string, object> activationInfo);

    /// <summary>
    /// Sends license revocation notification
    /// </summary>
    /// <param name="license">Revoked license</param>
    /// <param name="reason">Revocation reason</param>
    /// <returns>True if notification sent successfully</returns>
    Task<bool> SendLicenseRevocationNotificationAsync(ProductLicense license, string reason);

    /// <summary>
    /// Sends license renewal notification
    /// </summary>
    /// <param name="license">Renewed license</param>
    /// <param name="previousExpiryDate">Previous expiry date</param>
    /// <returns>True if notification sent successfully</returns>
    Task<bool> SendLicenseRenewalNotificationAsync(ProductLicense license, DateTime previousExpiryDate);

    /// <summary>
    /// Sends license suspension notification
    /// </summary>
    /// <param name="license">Suspended license</param>
    /// <param name="reason">Suspension reason</param>
    /// <param name="suspensionEndDate">End date of suspension</param>
    /// <returns>True if notification sent successfully</returns>
    Task<bool> SendLicenseSuspensionNotificationAsync(ProductLicense license, string reason, DateTime? suspensionEndDate);

    /// <summary>
    /// Sends bulk expiration alert to administrators
    /// </summary>
    /// <param name="expiringLicenses">List of expiring licenses</param>
    /// <param name="daysUntilExpiry">Days until expiry</param>
    /// <returns>True if alert sent successfully</returns>
    Task<bool> SendBulkExpirationAlertAsync(IEnumerable<ProductLicense> expiringLicenses, int daysUntilExpiry);

    /// <summary>
    /// Sends license validation failure alert
    /// </summary>
    /// <param name="licenseKey">License key that failed validation</param>
    /// <param name="validationError">Validation error details</param>
    /// <param name="attemptInfo">Information about the validation attempt</param>
    /// <returns>True if alert sent successfully</returns>
    Task<bool> SendValidationFailureAlertAsync(string licenseKey, string validationError, Dictionary<string, object> attemptInfo);

    /// <summary>
    /// Sends license usage threshold alert
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="currentUsage">Current usage count</param>
    /// <param name="threshold">Usage threshold</param>
    /// <returns>True if alert sent successfully</returns>
    Task<bool> SendUsageThresholdAlertAsync(string productId, int currentUsage, int threshold);

    /// <summary>
    /// Sends custom notification
    /// </summary>
    /// <param name="recipients">List of recipients</param>
    /// <param name="subject">Notification subject</param>
    /// <param name="message">Notification message</param>
    /// <param name="notificationType">Type of notification</param>
    /// <param name="metadata">Additional metadata</param>
    /// <returns>True if notification sent successfully</returns>
    Task<bool> SendCustomNotificationAsync(
        IEnumerable<string> recipients,
        string subject,
        string message,
        NotificationType notificationType,
        Dictionary<string, object>? metadata = null);

    /// <summary>
    /// Gets notification history for a license
    /// </summary>
    /// <param name="licenseId">License ID</param>
    /// <param name="fromDate">Start date filter</param>
    /// <param name="toDate">End date filter</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of notification history entries</returns>
    Task<IEnumerable<NotificationHistory>> GetLicenseNotificationHistoryAsync(
        string licenseId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50);

    /// <summary>
    /// Gets notification history for a consumer
    /// </summary>
    /// <param name="consumerId">Consumer ID</param>
    /// <param name="fromDate">Start date filter</param>
    /// <param name="toDate">End date filter</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of notification history entries</returns>
    Task<IEnumerable<NotificationHistory>> GetConsumerNotificationHistoryAsync(
        string consumerId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50);

    /// <summary>
    /// Gets notification templates
    /// </summary>
    /// <param name="notificationType">Filter by notification type</param>
    /// <returns>List of notification templates</returns>
    Task<IEnumerable<NotificationTemplate>> GetNotificationTemplatesAsync(NotificationType? notificationType = null);

    /// <summary>
    /// Creates or updates a notification template
    /// </summary>
    /// <param name="template">Notification template</param>
    /// <param name="createdBy">User creating/updating the template</param>
    /// <returns>Created/updated template</returns>
    Task<NotificationTemplate> SaveNotificationTemplateAsync(NotificationTemplate template, string createdBy);

    /// <summary>
    /// Gets notification preferences for a consumer
    /// </summary>
    /// <param name="consumerId">Consumer ID</param>
    /// <returns>Notification preferences</returns>
    Task<NotificationPreferences> GetNotificationPreferencesAsync(string consumerId);

    /// <summary>
    /// Updates notification preferences for a consumer
    /// </summary>
    /// <param name="consumerId">Consumer ID</param>
    /// <param name="preferences">Notification preferences</param>
    /// <returns>Updated preferences</returns>
    Task<NotificationPreferences> UpdateNotificationPreferencesAsync(string consumerId, NotificationPreferences preferences);

    /// <summary>
    /// Gets notification statistics
    /// </summary>
    /// <param name="fromDate">Start date filter</param>
    /// <param name="toDate">End date filter</param>
    /// <returns>Notification statistics</returns>
    Task<NotificationStatistics> GetNotificationStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
}
