using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Notification;
using TechWayFit.Licensing.Management.Core.Models.Notification;

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Notification;

/// <summary>
/// Repository interface for NotificationTemplate entities
/// </summary>
public interface INotificationTemplateRepository : IBaseRepository<NotificationTemplateEntity>
{
    /// <summary>
    /// Get notification templates by type
    /// </summary>
    /// <param name="type">Notification type</param>
    /// <returns>List of notification templates</returns>
    Task<IEnumerable<NotificationTemplateEntity>> GetByTypeAsync(NotificationType type);

    /// <summary>
    /// Get all active notification templates
    /// </summary>
    /// <returns>List of active notification templates</returns>
    Task<IEnumerable<NotificationTemplateEntity>> GetActiveTemplatesAsync();
  
}
