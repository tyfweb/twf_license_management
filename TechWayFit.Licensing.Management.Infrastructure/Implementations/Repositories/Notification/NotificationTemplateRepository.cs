using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Infrastructure.Contracts.Repositories.Notification;
using TechWayFit.Licensing.Infrastructure.Data.Context;
using TechWayFit.Licensing.Infrastructure.Models.Entities.Notification;
using TechWayFit.Licensing.Management.Core.Models.Notification;

namespace TechWayFit.Licensing.Infrastructure.Data.Repositories.Notification;

/// <summary>
/// Notification template repository implementation
/// </summary>
public class NotificationTemplateRepository : BaseRepository<NotificationTemplateEntity>, INotificationTemplateRepository
{
    public NotificationTemplateRepository(LicensingDbContext context) : base(context)
    {
    }
   
    public async Task<IEnumerable<NotificationTemplateEntity>> GetByTypeAsync(NotificationType type)
    {
        return await _dbSet.Where(t => t.NotificationType == type.ToString() && t.IsActive)
                         .ToListAsync();
    }

  
    public async Task<IEnumerable<NotificationTemplateEntity>> GetActiveTemplatesAsync()
    {
        return await _dbSet.Where(t => t.IsActive)
                         .OrderBy(t => t.TemplateName)
                         .ToListAsync();
    }
}
