using TechWayFit.Licensing.Management.Core.Models.Audit;

namespace TechWayFit.Licensing.Management.Core.Contracts.Services;

/// <summary>
/// Service interface for audit operations in license management
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs an audit entry
    /// </summary>
    /// <param name="entry">Audit entry to log</param>
    /// <returns>Created audit entry ID</returns>
    Task<string> LogAuditEntryAsync(AuditEntry entry);
    /// <summary>
    /// Gets audit entries based on search criteria
    /// </summary>
    /// <param name="searchRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<AuditEntry>> GetAuditEntriesAsync(
       AuditSearchRequest searchRequest,
       CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit entries for a specific license
    /// </summary>
    /// <param name="licenseId">License ID</param>
    /// <param name="fromDate">Start date filter</param>
    /// <param name="toDate">End date filter</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of audit entries</returns>
    Task<IEnumerable<AuditEntry>> GetLicenseAuditEntriesAsync(
        Guid licenseId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50);

    /// <summary>
    /// Gets audit entries for a specific consumer
    /// </summary>
    /// <param name="consumerId">Consumer ID</param>
    /// <param name="fromDate">Start date filter</param>
    /// <param name="toDate">End date filter</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of audit entries</returns>
    Task<IEnumerable<AuditEntry>> GetConsumerAuditEntriesAsync(
        Guid consumerId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50);

    /// <summary>
    /// Gets audit entries for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="fromDate">Start date filter</param>
    /// <param name="toDate">End date filter</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of audit entries</returns>
    Task<IEnumerable<AuditEntry>> GetProductAuditEntriesAsync(
        string productId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50);

    /// <summary>
    /// Gets audit entries by user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="fromDate">Start date filter</param>
    /// <param name="toDate">End date filter</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of audit entries</returns>
    Task<IEnumerable<AuditEntry>> GetUserAuditEntriesAsync(
        string userId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50);

    /// <summary>
    /// Gets audit entries by action type
    /// </summary>
    /// <param name="actionType">Action type</param>
    /// <param name="fromDate">Start date filter</param>
    /// <param name="toDate">End date filter</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of audit entries</returns>
    Task<IEnumerable<AuditEntry>> GetAuditEntriesByActionAsync(
        string actionType,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50);

    /// <summary>
    /// Gets all audit entries with optional filtering
    /// </summary>
    /// <param name="entityType">Filter by entity type</param>
    /// <param name="entityId">Filter by entity ID</param>
    /// <param name="actionType">Filter by action type</param>
    /// <param name="userId">Filter by user ID</param>
    /// <param name="fromDate">Start date filter</param>
    /// <param name="toDate">End date filter</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of audit entries</returns>
    Task<IEnumerable<AuditEntry>> GetAuditEntriesAsync(
        string? entityType = null,
        string? entityId = null,
        string? actionType = null,
        string? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50);

    /// <summary>
    /// Gets audit entry count with optional filtering
    /// </summary>
    /// <param name="entityType">Filter by entity type</param>
    /// <param name="entityId">Filter by entity ID</param>
    /// <param name="actionType">Filter by action type</param>
    /// <param name="userId">Filter by user ID</param>
    /// <param name="fromDate">Start date filter</param>
    /// <param name="toDate">End date filter</param>
    /// <returns>Total count of audit entries</returns>
    Task<int> GetAuditEntryCountAsync(
        string? entityType = null,
        string? entityId = null,
        string? actionType = null,
        string? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);

   

    /// <summary>
    /// Gets audit statistics
    /// </summary>
    /// <param name="fromDate">Start date filter</param>
    /// <param name="toDate">End date filter</param>
    /// <returns>Audit statistics</returns>
    Task<AuditStatistics> GetAuditStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Gets distinct entity types from audit entries
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<string>> GetDistinctEntityTypesAsync();

    /// <summary>
    /// Gets distinct entity IDs from audit entries
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<string>> GetDistinctActionsAsync();

    /// <summary>
    /// Gets security-related audit entries
    /// </summary>
    /// <param name="fromDate">Start date filter</param>
    /// <param name="toDate">End date filter</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of security audit entries</returns>
    Task<IEnumerable<AuditEntry>> GetSecurityAuditEntriesAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50);
}
