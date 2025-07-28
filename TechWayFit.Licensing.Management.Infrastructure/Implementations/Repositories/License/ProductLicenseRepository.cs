using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Core.Helpers;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Infrastructure.Contracts.Repositories.License;
using TechWayFit.Licensing.Infrastructure.Data.Context;
using TechWayFit.Licensing.Infrastructure.Models.Entities.License;
using TechWayFit.Licensing.Management.Core.Models.License;

namespace TechWayFit.Licensing.Infrastructure.Data.Repositories.License;

/// <summary>
/// Product license repository implementation
/// </summary>
public class ProductLicenseRepository : BaseRepository<ProductLicenseEntity>, IProductLicenseRepository
{ 
    public ProductLicenseRepository(LicensingDbContext context) : base(context)
    { 
    }
     /// <summary>
     /// Get expiring licenses in the next specified number of days (default is 30 days)
     /// </summary>
     /// <param name="daysFromNow"></param>
     /// <param name="cancellationToken"></param>
     /// <returns></returns>
    public async Task<IEnumerable<ProductLicenseEntity>> GetExpiringLicensesAsync(int daysFromNow = 30, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(license => license.ValidTo <= DateTime.UtcNow.AddDays(daysFromNow))
            .ToListAsync(cancellationToken);
    }
    /// <summary>
    /// Get license by its unique key
    /// </summary>
    /// <param name="licenseCode"></param>
    /// <returns></returns>
    public async Task<ProductLicenseEntity?> GetByLicenseKeyAsync(string licenseCode)
    {
        return await _dbSet.Include(l => l.Product)
                         .Include(l => l.Consumer)
                         .FirstOrDefaultAsync(l => l.LicenseCode == licenseCode);
    }

    /// <summary>
    /// Get licenses by consumer identifier
    /// </summary>
    /// <param name="consumerId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<ProductLicenseEntity>> GetByConsumerIdAsync(string consumerId)
    {
        return await _dbSet.Where(l => l.ConsumerId == consumerId)
                         .Include(l => l.Product)
                         .OrderByDescending(l => l.CreatedOn)
                         .ToListAsync();
    }

    /// <summary>
    /// Get licenses expiring soon for a specific customer
    /// </summary>
    /// <param name="daysAhead"></param>
    /// <param name="customerId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<ProductLicenseEntity>> GetExpiringLicensesForCustomerAsync(int daysAhead = 30, string customerId = "", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(customerId))
        {
            return await GetExpiringLicensesAsync(daysAhead);
        }

        return await _dbSet.Where(l => l.ConsumerId == customerId && l.ValidTo <= DateTime.UtcNow.AddDays(daysAhead))
                         .Include(l => l.Product)
                         .ToListAsync();

    }

    /// <summary>
    /// Check if a license is valid based on its code
    /// </summary>
    /// <param name="licenseCode"></param>
    /// <returns></returns>
    public async Task<bool> IsLicenseValidAsync(string licenseCode)
    {
        return await _dbSet.AnyAsync(l => l.LicenseCode == licenseCode && 
                                        l.Status == LicenseStatus.Active.ToString() &&
                                        l.ValidFrom <= DateTime.UtcNow &&
                                        l.ValidTo >= DateTime.UtcNow);
    } 

    /// <summary>
    /// Get license usage statistics
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="consumerId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<LicenseUsageStatistics> GetUsageStatisticsAsync(string? productId = null, string? consumerId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrEmpty(productId))
        {
            query = query.Where(license => license.ProductId == productId);
        }

        if (!string.IsNullOrEmpty(consumerId))
        {
            query = query.Where(license => license.ConsumerId == consumerId);
        }

        var expiringIn30Days = await query.CountAsync(
            license => license.ValidTo <= DateTime.UtcNow.AddDays(30) && license.ValidTo > DateTime.UtcNow, cancellationToken);

        Dictionary<LicenseStatus, int> licenseCountGroupByStatus = await query
        .GroupBy(license => license.Status)
            .ToDictionaryAsync(group => group.Key.ToEnum<LicenseStatus>(), group => group.Count(), cancellationToken);

        var expiredLicenses = licenseCountGroupByStatus.GetValueOrDefault(LicenseStatus.Expired, 0);
        expiredLicenses += await query
            .CountAsync(license =>
                license.ValidTo < DateTime.UtcNow && license.Status == LicenseStatus.Active.ToString(), cancellationToken);
        var totalLicenses = licenseCountGroupByStatus.Values.Sum();

        return new LicenseUsageStatistics
        {
            TotalLicenses = totalLicenses,
            ActiveLicenses = licenseCountGroupByStatus.GetValueOrDefault(LicenseStatus.Active, 0),
            ExpiringInNext30Days = expiringIn30Days,
            ExpiredLicenses = expiredLicenses,
            RevokedLicenses = licenseCountGroupByStatus.GetValueOrDefault(LicenseStatus.Revoked, 0),
            SuspendedLicenses = licenseCountGroupByStatus.GetValueOrDefault(LicenseStatus.Suspended, 0),
            LicensesByProduct = await query
                .GroupBy(license => license.ProductId)
                .ToDictionaryAsync(group => group.Key, group => group.Count(), cancellationToken),
            LicensesByStatus = licenseCountGroupByStatus
        };
    } 
}
