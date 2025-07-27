using TechWayFit.Licensing.Management.Core.Models.Audit;
using TechWayFit.Licensing.Management.Core.Models.License;

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
    /// Gets audit entries for a specific license
    /// </summary>
    /// <param name="licenseId">License ID</param>
    /// <param name="fromDate">Start date filter</param>
    /// <param name="toDate">End date filter</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of audit entries</returns>
    Task<IEnumerable<AuditEntry>> GetLicenseAuditEntriesAsync(
        string licenseId,
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
        string consumerId,
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
    /// Logs license creation audit entry
    /// </summary>
    /// <param name="license">Created license</param>
    /// <param name="createdBy">User who created the license</param>
    /// <returns>Audit entry ID</returns>
    Task<string> LogLicenseCreatedAsync(ProductLicense license, string createdBy);

    /// <summary>
    /// Logs license modification audit entry
    /// </summary>
    /// <param name="license">Modified license</param>
    /// <param name="modifiedBy">User who modified the license</param>
    /// <param name="changes">Dictionary of changes made</param>
    /// <returns>Audit entry ID</returns>
    Task<string> LogLicenseModifiedAsync(ProductLicense license, string modifiedBy, Dictionary<string, object> changes);

    /// <summary>
    /// Logs license status change audit entry
    /// </summary>
    /// <param name="licenseId">License ID</param>
    /// <param name="oldStatus">Old status</param>
    /// <param name="newStatus">New status</param>
    /// <param name="changedBy">User who changed the status</param>
    /// <param name="reason">Reason for change</param>
    /// <returns>Audit entry ID</returns>
    Task<string> LogLicenseStatusChangedAsync(string licenseId, LicenseStatus oldStatus, LicenseStatus newStatus, string changedBy, string? reason = null);

    /// <summary>
    /// Logs license activation audit entry
    /// </summary>
    /// <param name="licenseId">License ID</param>
    /// <param name="activationInfo">Activation information</param>
    /// <returns>Audit entry ID</returns>
    Task<string> LogLicenseActivatedAsync(string licenseId, Dictionary<string, object> activationInfo);

    /// <summary>
    /// Logs license validation audit entry
    /// </summary>
    /// <param name="licenseKey">License key</param>
    /// <param name="validationResult">Validation result</param>
    /// <param name="validatedBy">User or system that performed validation</param>
    /// <returns>Audit entry ID</returns>
    Task<string> LogLicenseValidatedAsync(string licenseKey, bool validationResult, string validatedBy);

    /// <summary>
    /// Gets audit statistics
    /// </summary>
    /// <param name="fromDate">Start date filter</param>
    /// <param name="toDate">End date filter</param>
    /// <returns>Audit statistics</returns>
    Task<AuditStatistics> GetAuditStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Exports audit entries to a specific format
    /// </summary>
    /// <param name="format">Export format (CSV, JSON, XML)</param>
    /// <param name="entityType">Filter by entity type</param>
    /// <param name="entityId">Filter by entity ID</param>
    /// <param name="fromDate">Start date filter</param>
    /// <param name="toDate">End date filter</param>
    /// <returns>Exported data as byte array</returns>
    Task<byte[]> ExportAuditEntriesAsync(
        string format,
        string? entityType = null,
        string? entityId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);
}
