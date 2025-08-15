using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common; 

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;

/// <summary>
/// Repository interface for ProductFeature entities
/// </summary>
public interface IProductFeatureRepository : IDataRepository<ProductFeature>
{
    Task<IEnumerable<ProductFeature>> GetFeaturesByProductIdAsync(Guid productId);
    Task<IEnumerable<ProductFeature>> GetByTierIdAsync(Guid tierId);
    Task<bool> IsCodeUniqueAsync(Guid productId, string code, Guid? excludeId);
    Task<IEnumerable<ProductFeature>> GetFeaturesByProductVersionAsync(Guid productId, string productVersion);
    Task<IEnumerable<ProductFeature>> GetFeaturesByProductVersionIdAsync(Guid productId, Guid versionId);
    Task<ProductFeature?> GetFeatureByCodeAsync(Guid productId, string featureCode);
    Task<bool> IsFeatureCodeUniqueAsync(Guid productId, string featureCode, Guid? excludeFeatureId = null);
    Task<IEnumerable<ProductFeature>> GetByProductIdAsync(Guid productId);
}
