using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.OperationsDashboard;

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.OperationsDashboard;

/// <summary>
/// Repository interface for managing system health snapshots
/// </summary>
public interface ISystemHealthSnapshotRepository : IOperationsDashboardBaseRepository<SystemHealthSnapshotEntity>
{
    // Specific query operations for SystemHealthSnapshot
    Task<SystemHealthSnapshotEntity?> GetLatestAsync();
    Task<IEnumerable<SystemHealthSnapshotEntity>> GetByHealthStatusAsync(string healthStatus, DateTime startTime, DateTime endTime);
    Task<IEnumerable<SystemHealthSnapshotEntity>> GetByCpuUsageThresholdAsync(decimal thresholdPercentage, DateTime startTime, DateTime endTime);
    Task<IEnumerable<SystemHealthSnapshotEntity>> GetByMemoryUsageThresholdAsync(decimal thresholdPercentage, DateTime startTime, DateTime endTime);
    Task<IEnumerable<SystemHealthSnapshotEntity>> GetByDiskUsageThresholdAsync(decimal thresholdPercentage, DateTime startTime, DateTime endTime);
    Task<IEnumerable<SystemHealthSnapshotEntity>> GetByActiveConnectionsThresholdAsync(int thresholdCount, DateTime startTime, DateTime endTime);
    Task<IEnumerable<SystemHealthSnapshotEntity>> GetUnhealthySnapshotsAsync(DateTime startTime, DateTime endTime);
    Task<IEnumerable<SystemHealthSnapshotEntity>> GetRecentSnapshotsAsync(int count);

    // Analytics operations specific to SystemHealthSnapshot
    Task<decimal> GetAverageCpuUsageAsync(DateTime startTime, DateTime endTime);
    Task<decimal> GetAverageMemoryUsageAsync(DateTime startTime, DateTime endTime);
    Task<decimal> GetAverageDiskUsageAsync(DateTime startTime, DateTime endTime);
    Task<decimal> GetAverageActiveConnectionsAsync(DateTime startTime, DateTime endTime);
    Task<decimal> GetMaxCpuUsageAsync(DateTime startTime, DateTime endTime);
    Task<decimal> GetMaxMemoryUsageAsync(DateTime startTime, DateTime endTime);
    Task<decimal> GetMaxDiskUsageAsync(DateTime startTime, DateTime endTime);
    Task<int> GetMaxActiveConnectionsAsync(DateTime startTime, DateTime endTime);
    Task<int> GetHealthySnapshotCountAsync(DateTime startTime, DateTime endTime);
    Task<int> GetUnhealthySnapshotCountAsync(DateTime startTime, DateTime endTime);
    Task<decimal> GetHealthPercentageAsync(DateTime startTime, DateTime endTime);
}
