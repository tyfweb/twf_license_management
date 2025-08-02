using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.OperationsDashboard;

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.OperationsDashboard;

/// <summary>
/// Repository interface for managing error log summaries
/// </summary>
public interface IErrorLogSummaryRepository : IOperationsDashboardBaseRepository<ErrorLogSummaryEntity>
{
    // Specific query operations for ErrorLogSummary
    Task<ErrorLogSummaryEntity?> GetByMessageHashAsync(string messageHash);
    Task<IEnumerable<ErrorLogSummaryEntity>> GetByErrorTypeAsync(string errorType, DateTime startTime, DateTime endTime);
    Task<IEnumerable<ErrorLogSummaryEntity>> GetBySourceAsync(string source, DateTime startTime, DateTime endTime);
    Task<IEnumerable<ErrorLogSummaryEntity>> GetBySeverityAsync(string severity, DateTime startTime, DateTime endTime);
    Task<IEnumerable<ErrorLogSummaryEntity>> GetTopByOccurrenceCountAsync(int topCount, DateTime startTime, DateTime endTime);
    Task<IEnumerable<ErrorLogSummaryEntity>> GetTopByAffectedUsersAsync(int topCount, DateTime startTime, DateTime endTime);
    Task<IEnumerable<ErrorLogSummaryEntity>> GetUnresolvedErrorsAsync();
    Task<IEnumerable<ErrorLogSummaryEntity>> GetRecentErrorsAsync(int hours);

    // Analytics operations specific to ErrorLogSummary
    Task<int> GetTotalOccurrenceCountAsync(DateTime startTime, DateTime endTime);
    Task<int> GetUniqueErrorCountAsync(DateTime startTime, DateTime endTime);
    Task<int> GetUnresolvedCountAsync(DateTime startTime, DateTime endTime);
    Task<int> GetTotalAffectedUsersAsync(DateTime startTime, DateTime endTime);

    // Error management operations
    Task<ErrorLogSummaryEntity> IncrementOccurrenceAsync(string messageHash);
    Task<ErrorLogSummaryEntity> MarkAsResolvedAsync(string messageHash, string resolvedBy);
}
