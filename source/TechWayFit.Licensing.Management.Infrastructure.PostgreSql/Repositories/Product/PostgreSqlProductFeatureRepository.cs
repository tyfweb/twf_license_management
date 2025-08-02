using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.Product;

/// <summary>
/// Product feature repository implementation
/// </summary>
public class PostgreSqlProductFeatureRepository : PostgreSqlBaseRepository<ProductFeatureEntity>, IProductFeatureRepository
{ 
    public PostgreSqlProductFeatureRepository(PostgreSqlPostgreSqlLicensingDbContext context) : base(context)
    { 
    }

    public async Task<IEnumerable<ProductFeatureEntity>> GetFeaturesByProductIdAsync(string productId)
    {
        return await _dbSet.Where(f => f.ProductId == productId && f.IsActive)
                         .OrderBy(f => f.DisplayOrder)
                         .ToListAsync();
    }

    public async Task<IEnumerable<ProductFeatureEntity>> GetByTierIdAsync(string tierId)
    {
        return await _dbSet.Where(f => f.TierId == tierId && f.IsActive)
                         .OrderBy(f => f.DisplayOrder)
                         .ToListAsync();
    }

    public async Task<ProductFeatureEntity?> GetFeatureByCodeAsync(string productId, string featureCode)
    {
        return await _dbSet.FirstOrDefaultAsync(f => 
            f.ProductId == productId && 
            f.Code == featureCode && 
            f.IsActive);
    }

    public async Task<bool> IsFeatureCodeUniqueAsync(string productId, string featureCode, string? excludeFeatureId = null)
    {
        var query = _dbSet.Where(f => f.ProductId == productId && f.Code == featureCode);
        
        if (!string.IsNullOrEmpty(excludeFeatureId))
            query = query.Where(f => f.FeatureId != excludeFeatureId);
            
        return !await query.AnyAsync();
    }

    public async Task<IEnumerable<ProductFeatureEntity>> GetFeaturesByProductVersionAsync(string productId, string productVersion)
    {
        return await _dbSet.Where(f => f.ProductId == productId &&
                     string.Compare(f.SupportFromVersion, productVersion) < 0 &&
                     string.Compare(f.SupportToVersion, productVersion) >= 0 &&
                     f.IsActive)
                     .OrderBy(f => f.DisplayOrder)
                     .ToListAsync();
    }

    public Task<bool> IsCodeUniqueAsync(string productId, string code, string? excludeId)
    {
        var query = _dbSet.Where(f => f.ProductId == productId && f.Code == code);
        
        if (!string.IsNullOrEmpty(excludeId))
            query = query.Where(f => f.FeatureId != excludeId);
            
        return query.AnyAsync();
    }
}
