using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.OperationsDashboard;

/// <summary>
/// Entity for storing system health snapshots
/// </summary>
public class SystemHealthSnapshotEntity : BaseAuditEntity
{
    /// <summary>
    /// Unique identifier for the snapshot
    /// </summary>
    public Guid SnapshotId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp when the snapshot was taken
    /// </summary>
    public DateTime SnapshotTimestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Total requests in the last hour
    /// </summary>
    public int TotalRequestsLastHour { get; set; }

    /// <summary>
    /// Total errors in the last hour
    /// </summary>
    public int TotalErrorsLastHour { get; set; }

    /// <summary>
    /// Error rate percentage
    /// </summary>
    public decimal ErrorRatePercent { get; set; }

    /// <summary>
    /// Average response time in milliseconds
    /// </summary>
    public int AvgResponseTimeMs { get; set; }

    /// <summary>
    /// CPU usage percentage
    /// </summary>
    public decimal? CpuUsagePercent { get; set; }

    /// <summary>
    /// Memory usage in MB
    /// </summary>
    public decimal? MemoryUsageMb { get; set; }

    /// <summary>
    /// Disk usage percentage
    /// </summary>
    public decimal? DiskUsagePercent { get; set; }

    /// <summary>
    /// Number of active database connections
    /// </summary>
    public int? ActiveDbConnections { get; set; }

    /// <summary>
    /// Database connection pool usage percentage
    /// </summary>
    public decimal? DbPoolUsagePercent { get; set; }

    /// <summary>
    /// Average database query time in milliseconds
    /// </summary>
    public int? AvgDbQueryTimeMs { get; set; }

    /// <summary>
    /// Application uptime in minutes
    /// </summary>
    public int? UptimeMinutes { get; set; }

    /// <summary>
    /// Number of active user sessions
    /// </summary>
    public int? ActiveUserSessions { get; set; }

    /// <summary>
    /// Cache hit rate percentage
    /// </summary>
    public decimal? CacheHitRatePercent { get; set; }

    /// <summary>
    /// Total number of active licenses
    /// </summary>
    public int? TotalActiveLicenses { get; set; }

    /// <summary>
    /// Number of licenses validated in the last hour
    /// </summary>
    public int? LicensesValidatedLastHour { get; set; }

    /// <summary>
    /// License validation error rate percentage
    /// </summary>
    public decimal? LicenseValidationErrorRate { get; set; }

    /// <summary>
    /// Overall health status
    /// </summary>
    public string OverallHealthStatus { get; set; } = "Healthy";

    /// <summary>
    /// JSON array of current health issues
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? HealthIssuesJson { get; set; }
}
