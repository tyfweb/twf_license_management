namespace TechWayFit.Licensing.Management.Core.Models.Configuration;

/// <summary>
/// Configuration model for operations dashboard settings
/// </summary>
public class OperationsDashboardConfiguration
{
    /// <summary>
    /// Whether the operations dashboard is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Metrics collection settings
    /// </summary>
    public MetricsCollectionSettings MetricsCollection { get; set; } = new();

    /// <summary>
    /// Data retention settings
    /// </summary>
    public DataRetentionSettings DataRetention { get; set; } = new();

    /// <summary>
    /// Performance threshold settings
    /// </summary>
    public PerformanceThresholdSettings PerformanceThresholds { get; set; } = new();

    /// <summary>
    /// Feature settings
    /// </summary>
    public FeatureSettings Features { get; set; } = new();
}

/// <summary>
/// Metrics collection configuration
/// </summary>
public class MetricsCollectionSettings
{
    /// <summary>
    /// Size of the in-memory buffer for metrics
    /// </summary>
    public int BufferSize { get; set; } = 1000;

    /// <summary>
    /// How often to flush metrics to database in seconds
    /// </summary>
    public int FlushIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to enable real-time metrics collection
    /// </summary>
    public bool EnableRealTimeMetrics { get; set; } = true;

    /// <summary>
    /// Whether to track errors
    /// </summary>
    public bool EnableErrorTracking { get; set; } = true;

    /// <summary>
    /// Whether to track performance metrics
    /// </summary>
    public bool EnablePerformanceTracking { get; set; } = true;

    /// <summary>
    /// Whether to track query performance
    /// </summary>
    public bool EnableQueryTracking { get; set; } = true;

    /// <summary>
    /// Whether to track system health
    /// </summary>
    public bool EnableSystemHealthTracking { get; set; } = true;
}

/// <summary>
/// Data retention configuration
/// </summary>
public class DataRetentionSettings
{
    /// <summary>
    /// How long to keep system metrics in days
    /// </summary>
    public int SystemMetricsRetentionDays { get; set; } = 90;

    /// <summary>
    /// How long to keep error summaries in days
    /// </summary>
    public int ErrorSummariesRetentionDays { get; set; } = 180;

    /// <summary>
    /// How long to keep performance metrics in days
    /// </summary>
    public int PerformanceMetricsRetentionDays { get; set; } = 90;

    /// <summary>
    /// How long to keep health snapshots in days
    /// </summary>
    public int HealthSnapshotsRetentionDays { get; set; } = 365;
}

/// <summary>
/// Performance threshold configuration
/// </summary>
public class PerformanceThresholdSettings
{
    /// <summary>
    /// Threshold for slow requests in milliseconds
    /// </summary>
    public int SlowRequestThresholdMs { get; set; } = 2000;

    /// <summary>
    /// Threshold for very slow requests in milliseconds
    /// </summary>
    public int VerySlowRequestThresholdMs { get; set; } = 5000;

    /// <summary>
    /// Threshold for slow queries in milliseconds
    /// </summary>
    public int SlowQueryThresholdMs { get; set; } = 1000;

    /// <summary>
    /// Threshold for very slow queries in milliseconds
    /// </summary>
    public int VerySlowQueryThresholdMs { get; set; } = 5000;

    /// <summary>
    /// High error rate percentage threshold
    /// </summary>
    public double HighErrorRatePercent { get; set; } = 5.0;

    /// <summary>
    /// Critical error rate percentage threshold
    /// </summary>
    public double CriticalErrorRatePercent { get; set; } = 10.0;
}

/// <summary>
/// Feature settings configuration
/// </summary>
public class FeatureSettings
{
    /// <summary>
    /// Whether to enable the real-time dashboard
    /// </summary>
    public bool EnableRealTimeDashboard { get; set; } = true;

    /// <summary>
    /// Whether to enable error alerts
    /// </summary>
    public bool EnableErrorAlerts { get; set; } = true;

    /// <summary>
    /// Whether to enable performance alerts
    /// </summary>
    public bool EnablePerformanceAlerts { get; set; } = true;

    /// <summary>
    /// Whether to enable system health alerts
    /// </summary>
    public bool EnableSystemHealthAlerts { get; set; } = true;

    /// <summary>
    /// Whether to enable data export functionality
    /// </summary>
    public bool EnableDataExport { get; set; } = true;

    /// <summary>
    /// Whether to enable detailed reports
    /// </summary>
    public bool EnableDetailedReports { get; set; } = true;
}
