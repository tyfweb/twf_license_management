using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.License;

namespace TechWayFit.Licensing.Management.Core.Contracts.Services;

/// <summary>
/// Service interface for managing product licenses
/// </summary>
public interface IProductLicenseService
{
    /// <summary>
    /// Generates a new product license
    /// </summary>
    /// <param name="request">License generation request parameters</param>
    /// <param name="generatedBy">User generating the license</param>
    /// <returns>Generated license with license key</returns>
    Task<ProductLicense> GenerateLicenseAsync(LicenseGenerationRequest request, string generatedBy);

    /// <summary>
    /// Updates license properties
    /// </summary>
    /// <param name="licenseId">License ID to update</param>
    /// <param name="request">License update request parameters</param>
    /// <param name="updatedBy">User updating the license</param>
    /// <returns>Updated license</returns>
    Task<ProductLicense> UpdateLicenseAsync(Guid licenseId, LicenseUpdateRequest request, string updatedBy);

    /// <summary>
    /// Regenerates license key and cryptographic data
    /// </summary>
    /// <param name="licenseId">License ID</param>
    /// <param name="regeneratedBy">User regenerating the license</param>
    /// <param name="reason">Reason for regeneration</param>
    /// <returns>License with new key and cryptographic data</returns>
    Task<ProductLicense> RegenerateLicenseKeyAsync(Guid licenseId, string regeneratedBy, string reason);

    /// <summary>
    /// Validates a license
    /// </summary>
    /// <param name="licenseKey">License key to validate</param>
    /// <param name="productId">Product ID</param>
    /// <param name="checkActivation">Whether to check activation status</param>
    /// <returns>Validation result with license details</returns>
    Task<LicenseValidationResult> ValidateLicenseAsync(string licenseKey, Guid productId, bool checkActivation = true);

    /// <summary>
    /// Gets a license by license key
    /// </summary>
    /// <param name="licenseKey">License key</param>
    /// <returns>License or null if not found</returns>
    Task<ProductLicense?> GetLicenseByKeyAsync(string licenseKey);

    /// <summary>
    /// Gets a license by ID
    /// </summary>
    /// <param name="licenseId">License ID</param>
    /// <returns>License or null if not found</returns>
    Task<ProductLicense?> GetLicenseByIdAsync(Guid licenseId);

    /// <summary>
    /// Gets licenses for a consumer
    /// </summary>
    /// <param name="consumerId">Consumer ID</param>
    /// <param name="status">Filter by license status</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of licenses</returns>
    Task<IEnumerable<ProductLicense>> GetLicensesByConsumerAsync(
        Guid consumerId,
        LicenseStatus? status = null,
        int pageNumber = 1,
        int pageSize = 50);
    /// <summary>
    /// Gets all licenses with optional filtering
    /// </summary>
    /// <param name="status"></param>
    /// <param name="searchTerm"></param>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    Task<IEnumerable<ProductLicense>> GetLicensesAsync(
        LicenseStatus? status = null,
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 50);

    /// <summary>
    /// Gets licenses for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="status">Filter by license status</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of licenses</returns>
    Task<IEnumerable<ProductLicense>> GetLicensesByProductAsync(
        Guid productId,
        LicenseStatus? status = null,
        int pageNumber = 1,
        int pageSize = 50);

    /// <summary>
    /// Updates license status
    /// </summary>
    /// <param name="licenseId">License ID</param>
    /// <param name="status">New status</param>
    /// <param name="updatedBy">User updating the status</param>
    /// <param name="reason">Reason for status change</param>
    /// <returns>True if updated successfully</returns>
    Task<bool> UpdateLicenseStatusAsync(Guid licenseId, LicenseStatus status, string updatedBy, string? reason = null);

    /// <summary>
    /// Activates a license
    /// </summary>
    /// <param name="licenseId">License ID</param>
    /// <param name="activationInfo">Activation information</param>
    /// <returns>True if activated successfully</returns>
    Task<bool> ActivateLicenseAsync(Guid licenseId, ActivationInfo activationInfo);

    /// <summary>
    /// Deactivates a license
    /// </summary>
    /// <param name="licenseId">License ID</param>
    /// <param name="deactivatedBy">User deactivating the license</param>
    /// <param name="reason">Reason for deactivation</param>
    /// <returns>True if deactivated successfully</returns>
    Task<bool> DeactivateLicenseAsync(Guid licenseId, string deactivatedBy, string? reason = null);

    /// <summary>
    /// Revokes a license
    /// </summary>
    /// <param name="licenseId">License ID</param>
    /// <param name="revokedBy">User revoking the license</param>
    /// <param name="reason">Reason for revocation</param>
    /// <returns>True if revoked successfully</returns>
    Task<bool> RevokeLicenseAsync(Guid licenseId, string revokedBy, string reason);

    /// <summary>
    /// Suspends a license
    /// </summary>
    /// <param name="licenseId">License ID</param>
    /// <param name="suspendedBy">User suspending the license</param>
    /// <param name="reason">Reason for suspension</param>
    /// <param name="suspensionEndDate">End date of suspension (optional)</param>
    /// <returns>True if suspended successfully</returns>
    Task<bool> SuspendLicenseAsync(Guid licenseId, string suspendedBy, string reason, DateTime? suspensionEndDate = null);

    /// <summary>
    /// Renews a license
    /// </summary>
    /// <param name="licenseId">License ID</param>
    /// <param name="newExpiryDate">New expiry date</param>
    /// <param name="renewedBy">User renewing the license</param>
    /// <returns>True if renewed successfully</returns>
    Task<bool> RenewLicenseAsync(Guid licenseId, DateTime newExpiryDate, string renewedBy);

    /// <summary>
    /// Gets licenses expiring within specified days
    /// </summary>
    /// <param name="daysFromNow">Number of days from now</param>
    /// <returns>List of expiring licenses</returns>
    Task<IEnumerable<ProductLicense>> GetExpiringLicensesAsync(int daysFromNow = 30);

    /// <summary>
    /// Gets expired licenses
    /// </summary>
    /// <returns>List of expired licenses</returns>
    Task<IEnumerable<ProductLicense>> GetExpiredLicensesAsync();

    /// <summary>
    /// Gets license usage statistics
    /// </summary>
    /// <param name="productId">Product ID (optional)</param>
    /// <param name="consumerId">Consumer ID (optional)</param>
    /// <returns>License usage statistics</returns>
    Task<LicenseUsageStatistics> GetLicenseUsageStatisticsAsync(Guid? productId = null, Guid? consumerId = null);

    /// <summary>
    /// Checks if a license exists
    /// </summary>
    /// <param name="licenseKey">License key</param>
    /// <returns>True if exists</returns>
    Task<bool> LicenseExistsAsync(string licenseKey);

    /// <summary>
    /// Gets license audit history
    /// </summary>
    /// <param name="licenseId">License ID</param>
    /// <returns>List of audit entries</returns>
    Task<IEnumerable<LicenseAuditEntry>> GetLicenseAuditHistoryAsync(Guid licenseId);

    /// <summary>
    /// Validates license generation request data
    /// </summary>
    /// <param name="request">License generation request to validate</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> ValidateLicenseGenerationRequestAsync(LicenseGenerationRequest request);

    /// <summary>
    /// Validates license update request data
    /// </summary>
    /// <param name="licenseId">License ID being updated</param>
    /// <param name="request">License update request to validate</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> ValidateLicenseUpdateRequestAsync(Guid licenseId, LicenseUpdateRequest request);
}
