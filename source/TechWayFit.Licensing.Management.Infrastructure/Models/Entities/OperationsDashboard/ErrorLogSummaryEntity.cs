using TechWayFit.Licensing.Management.Infrastructure.Models.Entities;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.OperationsDashboard;

/// <summary>
/// Entity for storing aggregated error log summaries
/// </summary>
public class ErrorLogSummaryEntity : AuditEntity
{
    /// <summary>
    /// Unique identifier for the error summary
    /// </summary>
    public Guid ErrorSummaryId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp rounded to the hour for aggregation
    /// </summary>
    public DateTime TimestampHour { get; set; }

    /// <summary>
    /// Type of error (Exception, Validation, Authorization, etc.)
    /// </summary>
    public string ErrorType { get; set; } = string.Empty;

    /// <summary>
    /// Source of the error (Controller.Action or service name)
    /// </summary>
    public string? ErrorSource { get; set; }

    /// <summary>
    /// Hash of error message for grouping similar errors
    /// </summary>
    public string ErrorMessageHash { get; set; } = string.Empty;

    /// <summary>
    /// Sample error message (first occurrence)
    /// </summary>
    public string? ErrorMessageSample { get; set; }

    /// <summary>
    /// Number of times this error occurred
    /// </summary>
    public int OccurrenceCount { get; set; } = 1;

    /// <summary>
    /// First time this error occurred
    /// </summary>
    public DateTime FirstOccurrence { get; set; }

    /// <summary>
    /// Most recent time this error occurred
    /// </summary>
    public DateTime LastOccurrence { get; set; }

    /// <summary>
    /// Number of users affected by this error
    /// </summary>
    public int AffectedUsers { get; set; }

    /// <summary>
    /// Severity level of the error
    /// </summary>
    public string SeverityLevel { get; set; } = "Medium";

    /// <summary>
    /// Whether this error has been resolved
    /// </summary>
    public bool IsResolved { get; set; }

    /// <summary>
    /// Who resolved this error
    /// </summary>
    public string? ResolvedBy { get; set; }

    /// <summary>
    /// When this error was resolved
    /// </summary>
    public DateTime? ResolvedOn { get; set; }

    /// <summary>
    /// Resolution notes
    /// </summary>
    public string? ResolutionNotes { get; set; }

    /// <summary>
    /// Sample user agent that encountered this error
    /// </summary>
    public string? UserAgentSample { get; set; }

    /// <summary>
    /// Sample IP address that encountered this error
    /// </summary>
    public string? IpAddressSample { get; set; }

    /// <summary>
    /// Sample correlation ID for this error
    /// </summary>
    public Guid? CorrelationIdSample { get; set; }

    /// <summary>
    /// Hash of the stack trace for grouping
    /// </summary>
    public string? StackTraceHash { get; set; }
}
