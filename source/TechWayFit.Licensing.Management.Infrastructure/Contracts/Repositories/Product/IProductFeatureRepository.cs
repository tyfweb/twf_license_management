using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;

/// <summary>
/// Repository interface for ProductFeature entities
/// </summary>
public interface IProductFeatureRepository : IBaseRepository<ProductFeatureEntity>
{
    Task<IEnumerable<ProductFeatureEntity>> GetFeaturesByProductIdAsync(string productId);
    Task<IEnumerable<ProductFeatureEntity>> GetByTierIdAsync(string tierId);
    Task<bool> IsCodeUniqueAsync(string productId, string code, string? excludeId);
    Task<IEnumerable<ProductFeatureEntity>> GetFeaturesByProductVersionAsync(string productId, string productVersion);
    Task<ProductFeatureEntity?> GetFeatureByCodeAsync(string productId, string featureCode);
    Task<bool> IsFeatureCodeUniqueAsync(string productId, string featureCode, string? excludeFeatureId = null); 
    
}
