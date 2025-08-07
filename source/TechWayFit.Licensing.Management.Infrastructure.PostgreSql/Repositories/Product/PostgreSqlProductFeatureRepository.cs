using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.Products;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Contracts;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.Product;

/// <summary>
/// Product feature repository implementation
/// </summary>
public class PostgreSqlProductFeatureRepository :   BaseRepository<ProductFeature,ProductFeatureEntity>, IProductFeatureRepository
{ 
    public PostgreSqlProductFeatureRepository(PostgreSqlPostgreSqlLicensingDbContext context,IUserContext userContext) : base(context,userContext)
    { 
    }

    public async Task<IEnumerable<ProductFeature>> GetFeaturesByProductIdAsync(Guid productId)
    {
        var result = await _dbSet.Where(f => f.ProductId == productId && f.IsActive)
                         .OrderBy(f => f.DisplayOrder)
                         .ToListAsync();
        return result.Select(f => f.Map());
    }

    public async Task<IEnumerable<ProductFeature>> GetByTierIdAsync(Guid tierId)
    {
        var result = await _dbSet.Where(f => f.TierId == tierId && f.IsActive)
                         .OrderBy(f => f.DisplayOrder)
                         .ToListAsync();
        return result.Select(f => f.Map());
    }

    public async Task<ProductFeature?> GetFeatureByCodeAsync(Guid productId, string featureCode)
    {
        var result = await _dbSet.FirstOrDefaultAsync(f =>
            f.ProductId == productId &&
            f.Code == featureCode &&
            f.IsActive);
        return result?.Map();
    }

    public async Task<bool> IsFeatureCodeUniqueAsync(Guid productId, string featureCode, Guid? excludeFeatureId = null)
    {
        var query = _dbSet.Where(f => f.ProductId == productId && f.Code == featureCode);

        if (excludeFeatureId.HasValue)
            query = query.Where(f => f.Id != excludeFeatureId.Value);
            
        return !await query.AnyAsync();
    }

    public async Task<IEnumerable<ProductFeature>> GetFeaturesByProductVersionAsync(Guid productId, string productVersion)
    {
        var result = await _dbSet.Where(f => f.ProductId == productId &&
                     string.Compare(f.SupportFromVersion, productVersion) < 0 &&
                     string.Compare(f.SupportToVersion, productVersion) >= 0 &&
                     f.IsActive)
                     .OrderBy(f => f.DisplayOrder)
                     .ToListAsync();
        return result.Select(f => f.Map());
    }

    public Task<bool> IsCodeUniqueAsync(Guid productId, string code, Guid? excludeId)
    {
        var query = _dbSet.Where(f => f.ProductId == productId && f.Code == code);

        if (excludeId.HasValue)
            query = query.Where(f => f.Id != excludeId.Value);

        return query.AnyAsync();
    }

    public async Task<IEnumerable<ProductFeature>> GetByProductIdAsync(Guid productId)
    {
        var query = _dbSet.Where(f => f.ProductId == productId && f.IsActive)
                         .OrderBy(f => f.DisplayOrder);
        var result = await query.ToListAsync();
        return result.Select(f => f.Map());
    }
}
