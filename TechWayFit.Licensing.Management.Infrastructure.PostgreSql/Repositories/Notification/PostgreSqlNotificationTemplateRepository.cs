using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Notification;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Notification;
using TechWayFit.Licensing.Management.Core.Models.Notification;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.Notification;

/// <summary>
/// Notification template repository implementation
/// </summary>
public class PostgreSqlNotificationTemplateRepository : PostgreSqlBaseRepository<NotificationTemplateEntity>, INotificationTemplateRepository
{
    public PostgreSqlNotificationTemplateRepository(PostgreSqlPostgreSqlLicensingDbContext context) : base(context)
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
