using System;

namespace TechWayFit.Licensing.Management.Core.Matrices;

public class SqlMetric
{
    public required string QueryType { get; set; }
    public required int ExecutionTimeMs { get; set; }
    public required string TableName { get; set; }
    public required bool IsSlowQuery { get; set; }
    public required int ParameterCount { get; set; }
    public required string CommandText { get; set; }
    public required DateTime Timestamp { get; set; }
    // Additional properties can be added as needed

    public string? ErrorMessage { get; set; } // Optional: to capture any error messages related to the query
    public string? ErrorLevel { get; set; } // Optional: to capture the user who executed the query
    public string? ExceptionType { get; set; }
}
