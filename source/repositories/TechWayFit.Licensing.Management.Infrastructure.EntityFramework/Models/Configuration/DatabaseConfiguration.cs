namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Configuration;

/// <summary>
/// Database provider configuration
/// </summary>
public class DatabaseConfiguration
{
    /// <summary>
    /// Database provider type
    /// </summary>
    public string Provider { get; set; } = "EfCore";
    
    /// <summary>
    /// Connection string for the selected provider
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
    
    /// <summary>
    /// Database options
    /// </summary>
    public DatabaseOptions Options { get; set; } = new();
}

/// <summary>
/// Database configuration options
/// </summary>
public class DatabaseOptions
{
    /// <summary>
    /// Enable database migrations
    /// </summary>
    public bool EnableMigrations { get; set; } = true;
    
    /// <summary>
    /// Seed test data on startup
    /// </summary>
    public bool SeedTestData { get; set; } = false;
    
    /// <summary>
    /// Enable detailed database logging
    /// </summary>
    public bool EnableLogging { get; set; } = true;
    
    /// <summary>
    /// Enable sensitive data logging (development only)
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;
    
    /// <summary>
    /// Command timeout in seconds
    /// </summary>
    public int CommandTimeout { get; set; } = 30;
    
    /// <summary>
    /// Maximum retry attempts
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;
    
    /// <summary>
    /// Connection pool size
    /// </summary>
    public int? PoolSize { get; set; }
}

/// <summary>
/// Supported database providers
/// </summary>
public static class DatabaseProviders
{
    public const string EfCore = "EfCore";
    public const string SqlServer = "SqlServer"; 
    public const string DynamoDB = "DynamoDB";
    public const string InMemory = "InMemory";
    public const string SessionStore = "SessionStore";
}
