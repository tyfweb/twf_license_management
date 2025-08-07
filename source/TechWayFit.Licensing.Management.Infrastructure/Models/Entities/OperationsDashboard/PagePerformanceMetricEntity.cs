using TechWayFit.Licensing.Management.Infrastructure.Models.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.OperationsDashboard;

/// <summary>
/// Entity for storing aggregated page performance metrics
/// </summary>
public class PagePerformanceMetricEntity : AuditEntity
{
    /// <summary>
    /// Unique identifier for the performance metric
    /// </summary>
    public Guid PerformanceId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp rounded to the hour for aggregation
    /// </summary>
    public DateTime TimestampHour { get; set; }

    /// <summary>
    /// Controller name
    /// </summary>
    public string Controller { get; set; } = string.Empty;

    /// <summary>
    /// Action name
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Route template for the page
    /// </summary>
    public string? RouteTemplate { get; set; }

    /// <summary>
    /// Total number of requests
    /// </summary>
    public int RequestCount { get; set; }

    /// <summary>
    /// Average response time in milliseconds
    /// </summary>
    public int AvgResponseTimeMs { get; set; }

    /// <summary>
    /// Minimum response time in milliseconds
    /// </summary>
    public int MinResponseTimeMs { get; set; }

    /// <summary>
    /// Maximum response time in milliseconds
    /// </summary>
    public int MaxResponseTimeMs { get; set; }

    /// <summary>
    /// 95th percentile response time in milliseconds
    /// </summary>
    public int P95ResponseTimeMs { get; set; }

    /// <summary>
    /// 99th percentile response time in milliseconds
    /// </summary>
    public int P99ResponseTimeMs { get; set; }

    /// <summary>
    /// Number of slow requests (>2000ms)
    /// </summary>
    public int SlowRequestCount { get; set; }

    /// <summary>
    /// Number of very slow requests (>5000ms)
    /// </summary>
    public int VerySlowRequestCount { get; set; }

    /// <summary>
    /// Number of successful requests (2xx status codes)
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Number of client error requests (4xx status codes)
    /// </summary>
    public int ClientErrorCount { get; set; }

    /// <summary>
    /// Number of server error requests (5xx status codes)
    /// </summary>
    public int ServerErrorCount { get; set; }

    /// <summary>
    /// Average memory usage in MB
    /// </summary>
    public decimal? AvgMemoryUsageMb { get; set; }

    /// <summary>
    /// Average CPU usage percentage
    /// </summary>
    public decimal? AvgCpuUsagePercent { get; set; }

    /// <summary>
    /// Bounce rate percentage
    /// </summary>
    public decimal? BounceRatePercent { get; set; }

    /// <summary>
    /// Conversion rate percentage
    /// </summary>
    public decimal? ConversionRatePercent { get; set; }
}
