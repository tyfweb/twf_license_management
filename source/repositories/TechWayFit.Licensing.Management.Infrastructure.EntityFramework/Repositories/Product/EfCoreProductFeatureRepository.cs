using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Products;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Contracts;
using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories.Product;

/// <summary>
/// Product feature repository implementation
/// </summary>
public class EfCoreProductFeatureRepository :   BaseRepository<ProductFeature,ProductFeatureEntity>, IProductFeatureRepository
{ 
    public EfCoreProductFeatureRepository(EfCoreLicensingDbContext context,IUserContext userContext) : base(context,userContext)
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
        // Note: This method now needs to be updated to work with version IDs.
        // For backward compatibility, we'll return all features for the product
        // TODO: Update this method to properly handle version ranges with foreign keys
        var result = await _dbSet.Where(f => f.ProductId == productId && f.IsActive)
                     .OrderBy(f => f.DisplayOrder)
                     .ToListAsync();
        return result.Select(f => f.Map());
    }

    /// <summary>
    /// Gets features that support a specific version ID
    /// </summary>
    public async Task<IEnumerable<ProductFeature>> GetFeaturesByProductVersionIdAsync(Guid productId, Guid versionId)
    {
        // Get the target version to understand its semantic version for comparison
        var targetVersion = await _context.ProductVersions.FirstOrDefaultAsync(v => v.Id == versionId);
        if (targetVersion == null)
        {
            return Enumerable.Empty<ProductFeature>();
        }
    

        // Parse the target version string to SemanticVersion for comparison
        var targetSemanticVersion = SemanticVersion.Parse(targetVersion.Version);

        var result = await _dbSet
            .Include(f => f.SupportFromVersion)
            .Include(f => f.SupportToVersion)
            .Where(f => f.ProductId == productId && f.IsActive)
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync();
        
        // Filter in memory to handle semantic version comparisons properly
        var filteredFeatures = result.Where(f =>
        {
            // Feature supports this version if:
            var supportsFromVersion = f.SupportFromVersionId == null; // No version restriction
            if (!supportsFromVersion && f.SupportFromVersion != null)
            {
                var fromVersion = SemanticVersion.Parse(f.SupportFromVersion.Version);
                supportsFromVersion = fromVersion < targetSemanticVersion || fromVersion.ToString() == targetSemanticVersion.ToString();
            }

            var supportsToVersion = f.SupportToVersionId == null; // No end version restriction
            if (!supportsToVersion && f.SupportToVersion != null)
            {
                var toVersion = SemanticVersion.Parse(f.SupportToVersion.Version);
                supportsToVersion = toVersion > targetSemanticVersion || toVersion.ToString() == targetSemanticVersion.ToString();
            }

            return supportsFromVersion && supportsToVersion;
        });
        
        return filteredFeatures.Select(f => f.Map());
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
