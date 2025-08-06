using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;

/// <summary>
/// Repository interface for ProductFeature entities
/// </summary>
public interface IProductFeatureRepository : IBaseRepository<ProductFeatureEntity>
{
    Task<IEnumerable<ProductFeatureEntity>> GetFeaturesByProductIdAsync(Guid productId);
    Task<IEnumerable<ProductFeatureEntity>> GetByTierIdAsync(Guid tierId);
    Task<bool> IsCodeUniqueAsync(Guid productId, string code, Guid? excludeId);
    Task<IEnumerable<ProductFeatureEntity>> GetFeaturesByProductVersionAsync(Guid productId, string productVersion);
    Task<ProductFeatureEntity?> GetFeatureByCodeAsync(Guid productId, string featureCode);
    Task<bool> IsFeatureCodeUniqueAsync(Guid productId, string featureCode, Guid? excludeFeatureId = null);
    Task<IEnumerable<ProductFeatureEntity>> GetByProductIdAsync(Guid productId);
}
