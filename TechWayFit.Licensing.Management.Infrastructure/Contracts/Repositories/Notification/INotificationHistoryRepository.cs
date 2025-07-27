using TechWayFit.Licensing.Infrastructure.Contracts.Repositories.Common;
using TechWayFit.Licensing.Infrastructure.Models.Entities.Notification;

namespace TechWayFit.Licensing.Infrastructure.Contracts.Repositories.Notification;

/// <summary>
/// Repository interface for NotificationHistory entities
/// </summary>
public interface INotificationHistoryRepository : IBaseRepository<NotificationHistoryEntity>
{   
    /// <summary>
    /// Get notification statistics
    /// </summary>
    /// <param name="fromDate">Optional from date filter</param>
    /// <param name="toDate">Optional to date filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Notification statistics</returns>
    Task<Dictionary<string, object>> GetStatisticsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);
}
