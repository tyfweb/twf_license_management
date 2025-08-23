using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Products;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories;
using TechWayFit.Licensing.Management.Core.Contracts;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories.Product;

/// <summary>
/// Product feature tier mapping repository implementation
/// </summary>
public class EfCoreProductFeatureTierMappingRepository : BaseRepository<ProductFeatureTierMapping, ProductFeatureTierMappingEntity>, IProductFeatureTierMappingRepository
{
    public EfCoreProductFeatureTierMappingRepository(EfCoreLicensingDbContext context, IUserContext userContext) 
        : base(context, userContext)
    {
    }

    public async Task<IEnumerable<ProductFeatureTierMapping>> GetByTierIdAsync(Guid tierId)
    {
        var result = await _dbSet
            .Include(m => m.ProductFeature)
            .Where(m => m.ProductTierId == tierId && m.IsActive)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync();
        
        return result.Select(m => m.Map());
    }

    public async Task<IEnumerable<ProductFeatureTierMapping>> GetByFeatureIdAsync(Guid featureId)
    {
        var result = await _dbSet
            .Include(m => m.ProductTier)
            .Where(m => m.ProductFeatureId == featureId && m.IsActive)
            .OrderBy(m => m.ProductTier!.DisplayOrder)
            .ToListAsync();
        
        return result.Select(m => m.Map());
    }

    public async Task<IEnumerable<ProductFeatureTierMapping>> GetByProductIdAsync(Guid productId)
    {
        var result = await _dbSet
            .Include(m => m.ProductFeature)
            .Include(m => m.ProductTier)
            .Where(m => m.ProductFeature!.ProductId == productId && m.IsActive)
            .OrderBy(m => m.ProductTier!.DisplayOrder)
            .ThenBy(m => m.DisplayOrder)
            .ToListAsync();
        
        return result.Select(m => m.Map());
    }

    public async Task<ProductFeatureTierMapping?> GetByFeatureAndTierAsync(Guid featureId, Guid tierId)
    {
        var result = await _dbSet
            .Include(m => m.ProductFeature)
            .Include(m => m.ProductTier)
            .FirstOrDefaultAsync(m => 
                m.ProductFeatureId == featureId && 
                m.ProductTierId == tierId && 
                m.IsActive);
        
        return result?.Map();
    }

    public async Task<IEnumerable<ProductFeatureTierMapping>> SetTierFeaturesAsync(Guid tierId, IEnumerable<Guid> featureIds)
    {
        // Get current mappings for the tier
        var currentMappings = await _dbSet
            .Where(m => m.ProductTierId == tierId && m.IsActive)
            .ToListAsync();

        var featureIdsList = featureIds.ToList();
        var result = new List<ProductFeatureTierMapping>();

        // Remove mappings that are no longer needed
        var toRemove = currentMappings.Where(m => !featureIdsList.Contains(m.ProductFeatureId));
        foreach (var mapping in toRemove)
        {
            mapping.IsDeleted = true;
            mapping.DeletedBy = _userContext.UserName;
            mapping.DeletedOn = DateTime.UtcNow;
        }

        // Add or update mappings
        int displayOrder = 0;
        foreach (var featureId in featureIdsList)
        {
            var existingMapping = currentMappings.FirstOrDefault(m => m.ProductFeatureId == featureId);
            if (existingMapping != null)
            {
                // Update existing mapping
                existingMapping.DisplayOrder = displayOrder++;
                existingMapping.IsEnabled = true;
                existingMapping.UpdatedBy = _userContext.UserName;
                existingMapping.UpdatedOn = DateTime.UtcNow;
                result.Add(existingMapping.Map());
            }
            else
            {
                // Create new mapping
                var newMapping = new ProductFeatureTierMappingEntity
                {
                    Id = Guid.NewGuid(),
                    TenantId = _userContext.TenantId ?? Guid.Empty,
                    ProductFeatureId = featureId,
                    ProductTierId = tierId,
                    IsEnabled = true,
                    DisplayOrder = displayOrder++,
                    IsActive = true,
                    CreatedBy = _userContext.UserName ?? string.Empty,
                    CreatedOn = DateTime.UtcNow
                };
                
                _dbSet.Add(newMapping);
                result.Add(newMapping.Map());
            }
        }

        await _context.SaveChangesAsync();
        return result;
    }

    public async Task<int> RemoveByTierIdAsync(Guid tierId)
    {
        var mappings = await _dbSet
            .Where(m => m.ProductTierId == tierId && m.IsActive)
            .ToListAsync();

        foreach (var mapping in mappings)
        {
            mapping.IsDeleted = true;
            mapping.DeletedBy = _userContext.UserName ?? string.Empty;
            mapping.DeletedOn = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return mappings.Count;
    }

    public async Task<int> RemoveByFeatureIdAsync(Guid featureId)
    {
        var mappings = await _dbSet
            .Where(m => m.ProductFeatureId == featureId && m.IsActive)
            .ToListAsync();

        foreach (var mapping in mappings)
        {
            mapping.IsDeleted = true;
            mapping.DeletedBy = _userContext.UserName ?? string.Empty;
            mapping.DeletedOn = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return mappings.Count;
    }
}
