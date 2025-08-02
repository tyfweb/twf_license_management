using TechWayFit.Licensing.Management.Infrastructure.Models.Entities;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.OperationsDashboard;

/// <summary>
/// Entity for storing aggregated system metrics
/// </summary>
public class SystemMetricEntity : BaseAuditEntity
{
    /// <summary>
    /// Unique identifier for the metric
    /// </summary>
    public Guid MetricId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp rounded to the hour for aggregation
    /// </summary>
    public DateTime TimestampHour { get; set; }

    /// <summary>
    /// Type of metric (request, error, performance, api)
    /// </summary>
    public string MetricType { get; set; } = string.Empty;

    /// <summary>
    /// Controller name that generated the metric
    /// </summary>
    public string? Controller { get; set; }

    /// <summary>
    /// Action name that generated the metric
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// HTTP method (GET, POST, PUT, DELETE)
    /// </summary>
    public string? HttpMethod { get; set; }

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// Total number of requests in this time period
    /// </summary>
    public int RequestCount { get; set; }

    /// <summary>
    /// Total response time for all requests in milliseconds
    /// </summary>
    public long TotalResponseTimeMs { get; set; }

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
    /// Number of errors in this time period
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Number of timeout errors
    /// </summary>
    public int TimeoutCount { get; set; }

    /// <summary>
    /// Number of unique users in this time period
    /// </summary>
    public int UniqueUsers { get; set; }

    /// <summary>
    /// Server name where the metric was collected
    /// </summary>
    public string? ServerName { get; set; }

    /// <summary>
    /// Environment where the metric was collected
    /// </summary>
    public string Environment { get; set; } = "Development";
}
