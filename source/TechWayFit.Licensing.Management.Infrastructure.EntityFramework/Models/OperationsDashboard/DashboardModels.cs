namespace TechWayFit.Licensing.Management.Infrastructure.Models.OperationsDashboard;

/// <summary>
/// Configuration settings for the operations dashboard
/// </summary>
public class DashboardConfiguration
{
    /// <summary>
    /// Whether the dashboard is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Auto-refresh interval in seconds
    /// </summary>
    public int RefreshIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Whether metrics collection is enabled
    /// </summary>
    public bool MetricsCollectionEnabled { get; set; } = true;

    /// <summary>
    /// Buffer size for metrics collection
    /// </summary>
    public int BufferSize { get; set; } = 1000;

    /// <summary>
    /// Buffer flush interval in seconds
    /// </summary>
    public int BufferFlushIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Whether performance monitoring is enabled
    /// </summary>
    public bool PerformanceMonitoringEnabled { get; set; } = true;

    /// <summary>
    /// Whether error tracking is enabled
    /// </summary>
    public bool ErrorTrackingEnabled { get; set; } = true;

    /// <summary>
    /// Whether query performance tracking is enabled
    /// </summary>
    public bool QueryPerformanceTrackingEnabled { get; set; } = true;

    /// <summary>
    /// Default time range for views in hours
    /// </summary>
    public int DefaultTimeRangeHours { get; set; } = 24;

    /// <summary>
    /// Maximum number of items to show in top lists
    /// </summary>
    public int MaxTopItems { get; set; } = 20;
}

/// <summary>
/// Performance trend data point
/// </summary>
public class PerformanceTrend
{
    /// <summary>
    /// Timestamp of the data point
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Average response time in milliseconds
    /// </summary>
    public double AverageResponseTime { get; set; }

    /// <summary>
    /// Request count
    /// </summary>
    public int RequestCount { get; set; }

    /// <summary>
    /// Error rate as a percentage
    /// </summary>
    public double ErrorRate { get; set; }
}

/// <summary>
/// Error trend data point
/// </summary>
public class ErrorTrend
{
    /// <summary>
    /// Timestamp of the data point
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Error count
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Error rate as a percentage
    /// </summary>
    public double ErrorRate { get; set; }

    /// <summary>
    /// Total request count
    /// </summary>
    public int TotalRequests { get; set; }
}

/// <summary>
/// Health trend data point
/// </summary>
public class HealthTrend
{
    /// <summary>
    /// Timestamp of the data point
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// CPU usage percentage
    /// </summary>
    public double CpuUsage { get; set; }

    /// <summary>
    /// Memory usage percentage
    /// </summary>
    public double MemoryUsage { get; set; }

    /// <summary>
    /// Disk usage percentage
    /// </summary>
    public double DiskUsage { get; set; }

    /// <summary>
    /// Response time in milliseconds
    /// </summary>
    public double ResponseTime { get; set; }

    /// <summary>
    /// Overall health status
    /// </summary>
    public string HealthStatus { get; set; } = string.Empty;
}

/// <summary>
/// Buffer status information
/// </summary>
public class BufferStatus
{
    /// <summary>
    /// Number of items currently in buffer
    /// </summary>
    public int CurrentCount { get; set; }

    /// <summary>
    /// Maximum buffer capacity
    /// </summary>
    public int MaxCapacity { get; set; }

    /// <summary>
    /// Buffer utilization as a percentage
    /// </summary>
    public double UtilizationPercentage { get; set; }

    /// <summary>
    /// Last flush timestamp
    /// </summary>
    public DateTime LastFlushed { get; set; }

    /// <summary>
    /// Next scheduled flush timestamp
    /// </summary>
    public DateTime NextFlush { get; set; }

    /// <summary>
    /// Total items flushed since start
    /// </summary>
    public long TotalFlushed { get; set; }

    /// <summary>
    /// Number of flush operations since start
    /// </summary>
    public int FlushCount { get; set; }
}
