using TechWayFit.Licensing.Infrastructure.Contracts.Repositories.Common;
using TechWayFit.Licensing.Infrastructure.Models.Entities.License;
using TechWayFit.Licensing.Management.Core.Models.License;

namespace TechWayFit.Licensing.Infrastructure.Contracts.Repositories.License;

/// <summary>
/// Repository interface for ProductLicense entities
/// </summary>
public interface IProductLicenseRepository : IBaseRepository<ProductLicenseEntity>
{

    /// <summary>
    /// Get expiring licenses
    /// </summary>
    /// <param name="daysFromNow">Number of days from now</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of expiring licenses</returns>
    Task<IEnumerable<ProductLicenseEntity>> GetExpiringLicensesAsync(int daysFromNow = 30, CancellationToken cancellationToken = default);
    /// <summary>
    /// Get license by its unique key
    /// </summary>
    /// <param name="licenseCode"></param>
    /// <returns></returns>
    Task<ProductLicenseEntity?> GetByLicenseKeyAsync(string licenseCode);
    /// <summary>
    /// Get licenses by consumer identifier
    /// </summary>
    /// <param name="consumerId"></param>
    /// <returns></returns>
    Task<IEnumerable<ProductLicenseEntity>> GetByConsumerIdAsync(string consumerId);
    /// <summary>
    /// Get licenses expiring soon for a specific customer
    /// </summary>
    /// <param name="daysAhead"></param>
    /// <param name="customerId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<ProductLicenseEntity>> GetExpiringLicensesForCustomerAsync(int daysAhead = 30, string customerId = "", CancellationToken cancellationToken = default);
    /// <summary>
    /// Validate if a license is valid
    /// </summary>
    /// <param name="licenseCode">License code to validate</param>
    /// <returns>True if valid, otherwise false</returns>
    Task<bool> IsLicenseValidAsync(string licenseCode);

    /// <summary>
    /// Get license usage statistics
    /// </summary>
    /// <param name="productId">Optional product ID filter</param>
    /// <param name="consumerId">Optional consumer ID filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Usage statistics</returns>
    Task<LicenseUsageStatistics> GetUsageStatisticsAsync(string? productId = null, string? consumerId = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Get license by ID with all includes
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ProductLicenseEntity?> GetByIdWithAllIncludesAsync(string id, CancellationToken cancellationToken = default);
}
