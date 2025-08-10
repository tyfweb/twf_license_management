namespace TechWayFit.Licensing.Management.Infrastructure.SqlServer.Configuration;

/// <summary>
/// Configuration options for SQL Server infrastructure
/// </summary>
public class SqlServerOptions
{
    /// <summary>
    /// The configuration section name for SQL Server options
    /// </summary>
    public const string SectionName = "SqlServer";

    /// <summary>
    /// SQL Server connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of retry attempts for transient failures
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Maximum delay between retry attempts
    /// </summary>
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Command timeout in seconds
    /// </summary>
    public int CommandTimeout { get; set; } = 30;

    /// <summary>
    /// Enable sensitive data logging (use only in development)
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;

    /// <summary>
    /// Enable detailed error messages
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = false;

    /// <summary>
    /// Enable automatic migrations
    /// </summary>
    public bool EnableAutomaticMigrations { get; set; } = false;

    /// <summary>
    /// Connection pool settings
    /// </summary>
    public ConnectionPoolOptions ConnectionPool { get; set; } = new();

    /// <summary>
    /// Performance monitoring options
    /// </summary>
    public PerformanceOptions Performance { get; set; } = new();
}

/// <summary>
/// Connection pool configuration options
/// </summary>
public class ConnectionPoolOptions
{
    /// <summary>
    /// Maximum pool size
    /// </summary>
    public int MaxPoolSize { get; set; } = 100;

    /// <summary>
    /// Minimum pool size
    /// </summary>
    public int MinPoolSize { get; set; } = 0;

    /// <summary>
    /// Connection lifetime in seconds
    /// </summary>
    public int ConnectionLifetime { get; set; } = 0;

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public int ConnectionTimeout { get; set; } = 15;
}

/// <summary>
/// Performance monitoring options
/// </summary>
public class PerformanceOptions
{
    /// <summary>
    /// Enable query performance logging
    /// </summary>
    public bool EnableQueryLogging { get; set; } = false;

    /// <summary>
    /// Slow query threshold in milliseconds
    /// </summary>
    public int SlowQueryThresholdMs { get; set; } = 1000;

    /// <summary>
    /// Enable SQL Server performance counters
    /// </summary>
    public bool EnablePerformanceCounters { get; set; } = false;
}
