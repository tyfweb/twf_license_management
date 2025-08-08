namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;

/// <summary>
/// PostgreSQL database configuration options
/// </summary>
public class PostgreSqlOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "PostgreSQL";

    /// <summary>
    /// Database connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Maximum retry count for transient failures
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Maximum retry delay in seconds
    /// </summary>
    public int MaxRetryDelaySeconds { get; set; } = 30;

    /// <summary>
    /// Command timeout in seconds
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Database schema name
    /// </summary>
    public string SchemaName { get; set; } = "licensing";

    /// <summary>
    /// Migrations history table name
    /// </summary>
    public string MigrationsTableName { get; set; } = "__EFMigrationsHistory";

    /// <summary>
    /// Enable sensitive data logging (for development only)
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;

    /// <summary>
    /// Enable detailed errors (for development only)
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = false;

    /// <summary>
    /// Enable console logging (for development only)
    /// </summary>
    public bool EnableConsoleLogging { get; set; } = false;
}
