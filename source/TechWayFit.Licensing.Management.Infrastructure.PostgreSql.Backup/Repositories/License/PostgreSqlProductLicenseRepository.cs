using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Core.Helpers;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.License;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.License;
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Core.Contracts;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.License;

/// <summary>
/// PostgreSQL implementation of Product License repository
/// </summary>
public class PostgreSqlProductLicenseRepository : BaseRepository<ProductLicense, ProductLicenseEntity>, IProductLicenseRepository
{  
    public PostgreSqlProductLicenseRepository(PostgreSqlPostgreSqlLicensingDbContext context,IUserContext userContext) : base(context, userContext)
    {         
    }
     /// <summary>
     /// Get expiring licenses in the next specified number of days (default is 30 days)
     /// </summary>
     /// <param name="daysFromNow"></param>
     /// <param name="cancellationToken"></param>
     /// <returns></returns>
    public async Task<IEnumerable<ProductLicense>> GetExpiringLicensesAsync(int daysFromNow = 30, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet
            .Where(license => license.ValidTo <= DateTime.UtcNow.AddDays(daysFromNow))
            .ToListAsync(cancellationToken);
        return result.Select(license => license.Map());
    }
    /// <summary>
    /// Get license by its unique key
    /// </summary>
    /// <param name="licenseCode"></param>
    /// <returns></returns>
    public async Task<ProductLicense?> GetByLicenseKeyAsync(string licenseCode)
    {
        var result = await _dbSet.Include(l => l.Product)
                         .Include(l => l.Consumer)
                         .FirstOrDefaultAsync(l => l.LicenseCode == licenseCode);
        return result?.Map();
    }

    /// <summary>
    /// Get licenses by consumer identifier
    /// </summary>
    /// <param name="consumerId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<ProductLicense>> GetByConsumerIdAsync(Guid consumerId)
    {
        var result = await _dbSet.Where(l => l.ConsumerId == consumerId)
                         .Include(l => l.Product)
                         .OrderByDescending(l => l.CreatedOn)
                         .ToListAsync();
        return result.Select(license => license.Map());
    }

    /// <summary>
    /// Get licenses expiring soon for a specific customer
    /// </summary>
    /// <param name="daysAhead"></param>
    /// <param name="customerId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<ProductLicense>> GetExpiringLicensesForCustomerAsync(int daysAhead = 30, Guid customerId = default, CancellationToken cancellationToken = default)
    {
        if (customerId == default)
        {
            return await GetExpiringLicensesAsync(daysAhead);
        }

        var result= await _dbSet.Where(l => l.ConsumerId == customerId && l.ValidTo <= DateTime.UtcNow.AddDays(daysAhead))
                         .Include(l => l.Product)
                         .ToListAsync();
        return result.Select(license => license.Map());
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
    public async Task<LicenseUsageStatistics> GetUsageStatisticsAsync(Guid? productId = null, Guid? consumerId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (productId.HasValue)
        {
            query = query.Where(license => license.ProductId == productId);
        }

        if (consumerId.HasValue)
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
                .ToDictionaryAsync(group => group.Key.ToString(), group => group.Count(), cancellationToken),
            LicensesByStatus = licenseCountGroupByStatus
        };
    }
    /// <summary>
    /// Get license by its unique identifier with all related data
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<ProductLicense?> GetByIdWithAllIncludesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet.Include(l => l.Product).Include(l => l.Consumer)
                      .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        return result?.Map();
    }

    protected override IQueryable<ProductLicenseEntity> SearchIncludesQuery(IQueryable<ProductLicenseEntity> query)
    {
        query = query.Include(l => l.Product)
                     .Include(l => l.Consumer);
        return base.SearchIncludesQuery(query);
    }

    
}
