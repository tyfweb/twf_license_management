using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Notification;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Notification;
using TechWayFit.Licensing.Management.Core.Models.Notification;
using TechWayFit.Licensing.Management.Core.Contracts;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories.Notification;

/// <summary>
/// Notification history repository implementation
/// </summary>
public class EfCoreNotificationHistoryRepository : BaseRepository<NotificationHistory,NotificationHistoryEntity>, INotificationHistoryRepository
{
    DbSet<NotificationHistoryEntity> _notificationHistorySet;
    public EfCoreNotificationHistoryRepository(EfCoreLicensingDbContext dbContext,IUserContext userContext) : base(dbContext,userContext)
    {
        _notificationHistorySet = dbContext.Set<NotificationHistoryEntity>();
    }

    public Task<Dictionary<string, object>> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _notificationHistorySet.AsQueryable();

        if (fromDate.HasValue)
        {
            query = query.Where(nh => nh.CreatedOn >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(nh => nh.CreatedOn <= toDate.Value);
        }

        var totalCount = query.Count();
        
        // For InMemory provider compatibility: Load data first, then group in memory
        var allNotifications = query.ToList();
        var groupByStatus = allNotifications.GroupBy(nh => nh.Status);
        var sentCount = groupByStatus.Where(g => g.Key == DeliveryStatus.Sent.ToString()).Sum(g => g.Count());
        var pendingCount = groupByStatus.Where(g => g.Key == DeliveryStatus.Pending.ToString()).Sum(g => g.Count());
        var failedCount = groupByStatus.Where(g => g.Key == DeliveryStatus.Failed.ToString()).Sum(g => g.Count());

        var statistics = new Dictionary<string, object>
        {
            { "TotalCount", totalCount },
            { "SentCount", sentCount },
            { "PendingCount", pendingCount },
            { "FailedCount", failedCount }
        };

        return Task.FromResult(statistics);
    }
}
