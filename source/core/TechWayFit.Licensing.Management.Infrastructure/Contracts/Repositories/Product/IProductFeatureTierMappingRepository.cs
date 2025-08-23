using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;

/// <summary>
/// Repository interface for ProductFeatureTierMapping entities
/// </summary>
public interface IProductFeatureTierMappingRepository : IDataRepository<ProductFeatureTierMapping>
{
    /// <summary>
    /// Gets all feature mappings for a specific tier
    /// </summary>
    /// <param name="tierId">Product tier ID</param>
    /// <returns>List of feature mappings for the tier</returns>
    Task<IEnumerable<ProductFeatureTierMapping>> GetByTierIdAsync(Guid tierId);

    /// <summary>
    /// Gets all tier mappings for a specific feature
    /// </summary>
    /// <param name="featureId">Product feature ID</param>
    /// <returns>List of tier mappings for the feature</returns>
    Task<IEnumerable<ProductFeatureTierMapping>> GetByFeatureIdAsync(Guid featureId);

    /// <summary>
    /// Gets all mappings for a specific product (across all tiers and features)
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>List of all feature-tier mappings for the product</returns>
    Task<IEnumerable<ProductFeatureTierMapping>> GetByProductIdAsync(Guid productId);

    /// <summary>
    /// Gets a specific mapping by feature and tier IDs
    /// </summary>
    /// <param name="featureId">Product feature ID</param>
    /// <param name="tierId">Product tier ID</param>
    /// <returns>The feature-tier mapping or null if not found</returns>
    Task<ProductFeatureTierMapping?> GetByFeatureAndTierAsync(Guid featureId, Guid tierId);

    /// <summary>
    /// Adds or updates multiple mappings for a tier
    /// </summary>
    /// <param name="tierId">Product tier ID</param>
    /// <param name="featureIds">List of feature IDs to map to the tier</param>
    /// <returns>List of created/updated mappings</returns>
    Task<IEnumerable<ProductFeatureTierMapping>> SetTierFeaturesAsync(Guid tierId, IEnumerable<Guid> featureIds);

    /// <summary>
    /// Removes all feature mappings for a specific tier
    /// </summary>
    /// <param name="tierId">Product tier ID</param>
    /// <returns>Number of mappings removed</returns>
    Task<int> RemoveByTierIdAsync(Guid tierId);

    /// <summary>
    /// Removes all tier mappings for a specific feature
    /// </summary>
    /// <param name="featureId">Product feature ID</param>
    /// <returns>Number of mappings removed</returns>
    Task<int> RemoveByFeatureIdAsync(Guid featureId);
}
