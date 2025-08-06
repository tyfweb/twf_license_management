using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.OperationsDashboard;

/// <summary>
/// Entity for storing aggregated query performance metrics
/// </summary>
public class QueryPerformanceMetricEntity : BaseDbEntity
{
    /// <summary>
    /// Unique identifier for the query metric
    /// </summary>
    public Guid QueryMetricId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp rounded to the hour for aggregation
    /// </summary>
    public DateTime TimestampHour { get; set; }

    /// <summary>
    /// Hash of normalized query for grouping
    /// </summary>
    public string QueryHash { get; set; } = string.Empty;

    /// <summary>
    /// Type of query (SELECT, INSERT, UPDATE, DELETE)
    /// </summary>
    public string QueryType { get; set; } = string.Empty;

    /// <summary>
    /// Comma-separated list of table names involved
    /// </summary>
    public string? TableNames { get; set; }

    /// <summary>
    /// Controller.Action that triggered the query
    /// </summary>
    public string? OperationContext { get; set; }

    /// <summary>
    /// Number of times this query was executed
    /// </summary>
    public int ExecutionCount { get; set; }

    /// <summary>
    /// Total execution time for all executions in milliseconds
    /// </summary>
    public long TotalExecutionTimeMs { get; set; }

    /// <summary>
    /// Average execution time in milliseconds
    /// </summary>
    public int AvgExecutionTimeMs { get; set; }

    /// <summary>
    /// Minimum execution time in milliseconds
    /// </summary>
    public int MinExecutionTimeMs { get; set; }

    /// <summary>
    /// Maximum execution time in milliseconds
    /// </summary>
    public int MaxExecutionTimeMs { get; set; }

    /// <summary>
    /// Average number of rows affected
    /// </summary>
    public int RowsAffectedAvg { get; set; }

    /// <summary>
    /// Maximum number of rows affected
    /// </summary>
    public int RowsAffectedMax { get; set; }

    /// <summary>
    /// Number of slow query executions (>1000ms)
    /// </summary>
    public int SlowQueryCount { get; set; }

    /// <summary>
    /// Number of very slow query executions (>5000ms)
    /// </summary>
    public int VerySlowQueryCount { get; set; }

    /// <summary>
    /// Number of query timeouts
    /// </summary>
    public int TimeoutCount { get; set; }

    /// <summary>
    /// Sample query text (first occurrence)
    /// </summary>
    public string? QuerySample { get; set; }

    /// <summary>
    /// Sample parameters JSON (first occurrence)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? ParametersSampleJson { get; set; }

    /// <summary>
    /// Whether this query needs optimization
    /// </summary>
    public bool NeedsOptimization { get; set; }

    /// <summary>
    /// Notes about optimization suggestions
    /// </summary>
    public string? OptimizationNotes { get; set; }
}
