using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Notification;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Notification;
using TechWayFit.Licensing.Management.Core.Models.Notification;
using TechWayFit.Licensing.Management.Core.Contracts;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories.Notification;

/// <summary>
/// Notification template repository implementation
/// </summary>
public class EfCoreNotificationTemplateRepository : BaseRepository<NotificationTemplate,NotificationTemplateEntity>, INotificationTemplateRepository
{
    public EfCoreNotificationTemplateRepository(EfCoreLicensingDbContext context,IUserContext userContext) : base(context, userContext)
    {
    }

    public async Task<IEnumerable<NotificationTemplate>> GetByTypeAsync(NotificationType type)
    {
        var result = await _dbSet.Where(t => t.NotificationType == type.ToString() && t.IsActive)
                         .ToListAsync();
        return result.Select(t => t.Map());
    }


    public async Task<IEnumerable<NotificationTemplate>> GetActiveTemplatesAsync()
    {
        var result= await _dbSet.Where(t => t.IsActive)
                         .OrderBy(t => t.TemplateName)
                         .ToListAsync();
        return result.Select(t => t.Map());
    }
}
