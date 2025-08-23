using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Management.Core.Contracts.Services;

/// <summary>
/// Service interface for managing product feature tier mappings
/// </summary>
public interface IProductFeatureTierMappingService
{
    /// <summary>
    /// Gets all features mapped to a specific tier
    /// </summary>
    /// <param name="tierId">Product tier ID</param>
    /// <returns>List of features mapped to the tier</returns>
    Task<IEnumerable<ProductFeatureTierMapping>> GetFeaturesForTierAsync(Guid tierId);

    /// <summary>
    /// Gets all tiers that a specific feature is mapped to
    /// </summary>
    /// <param name="featureId">Product feature ID</param>
    /// <returns>List of tiers the feature is mapped to</returns>
    Task<IEnumerable<ProductFeatureTierMapping>> GetTiersForFeatureAsync(Guid featureId);

    /// <summary>
    /// Gets all feature-tier mappings for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>List of all feature-tier mappings for the product</returns>
    Task<IEnumerable<ProductFeatureTierMapping>> GetMappingsForProductAsync(Guid productId);

    /// <summary>
    /// Sets the features for a specific tier (replaces existing mappings)
    /// </summary>
    /// <param name="tierId">Product tier ID</param>
    /// <param name="featureIds">List of feature IDs to map to the tier</param>
    /// <returns>List of created/updated mappings</returns>
    Task<IEnumerable<ProductFeatureTierMapping>> SetTierFeaturesAsync(Guid tierId, IEnumerable<Guid> featureIds);

    /// <summary>
    /// Adds a single feature to a tier
    /// </summary>
    /// <param name="tierId">Product tier ID</param>
    /// <param name="featureId">Product feature ID</param>
    /// <returns>Created mapping</returns>
    Task<ProductFeatureTierMapping> AddFeatureToTierAsync(Guid tierId, Guid featureId);

    /// <summary>
    /// Removes a feature from a tier
    /// </summary>
    /// <param name="tierId">Product tier ID</param>
    /// <param name="featureId">Product feature ID</param>
    /// <returns>True if removed successfully</returns>
    Task<bool> RemoveFeatureFromTierAsync(Guid tierId, Guid featureId);

    /// <summary>
    /// Checks if a feature is available in a specific tier
    /// </summary>
    /// <param name="tierId">Product tier ID</param>
    /// <param name="featureId">Product feature ID</param>
    /// <returns>True if feature is available in the tier</returns>
    Task<bool> IsFeatureAvailableInTierAsync(Guid tierId, Guid featureId);

    /// <summary>
    /// Gets features that are available in a tier with their configuration
    /// </summary>
    /// <param name="tierId">Product tier ID</param>
    /// <returns>Dictionary of feature ID to configuration</returns>
    Task<Dictionary<Guid, string?>> GetTierFeatureConfigurationsAsync(Guid tierId);

    /// <summary>
    /// Updates the configuration for a specific feature-tier mapping
    /// </summary>
    /// <param name="tierId">Product tier ID</param>
    /// <param name="featureId">Product feature ID</param>
    /// <param name="configuration">Configuration JSON</param>
    /// <returns>Updated mapping</returns>
    Task<ProductFeatureTierMapping?> UpdateFeatureConfigurationAsync(Guid tierId, Guid featureId, string? configuration);
}
