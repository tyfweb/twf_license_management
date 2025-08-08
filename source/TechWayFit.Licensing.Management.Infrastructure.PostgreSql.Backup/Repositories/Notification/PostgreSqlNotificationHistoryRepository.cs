using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Notification;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.Notification;
using TechWayFit.Licensing.Management.Core.Models.Notification;
using TechWayFit.Licensing.Management.Core.Contracts;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.Notification;

/// <summary>
/// Notification history repository implementation
/// </summary>
public class PostgreSqlNotificationHistoryRepository : BaseRepository<NotificationHistory,NotificationHistoryEntity>, INotificationHistoryRepository
{
    DbSet<NotificationHistoryEntity> _notificationHistorySet;
    public PostgreSqlNotificationHistoryRepository(PostgreSqlPostgreSqlLicensingDbContext dbContext,IUserContext userContext) : base(dbContext,userContext)
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
        var groupByStatus = query.GroupBy(nh => nh.Status);
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
