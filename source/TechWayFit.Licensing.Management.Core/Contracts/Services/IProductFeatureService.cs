using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Management.Core.Contracts.Services;

/// <summary>
/// Service interface for managing product features
/// </summary>
public interface IProductFeatureService
{
    /// <summary>
    /// Creates a new product feature
    /// </summary>
    /// <param name="feature">Feature to create</param>
    /// <param name="createdBy">User creating the feature</param>
    /// <returns>Created feature</returns>
    Task<ProductFeature> CreateFeatureAsync(ProductFeature feature, string createdBy);

    /// <summary>
    /// Updates an existing product feature
    /// </summary>
    /// <param name="feature">Feature to update</param>
    /// <param name="updatedBy">User updating the feature</param>
    /// <returns>Updated feature</returns>
    Task<ProductFeature> UpdateFeatureAsync(ProductFeature feature, string updatedBy);

    /// <summary>
    /// Gets a feature by ID
    /// </summary>
    /// <param name="featureId">Feature ID</param>
    /// <returns>Feature or null if not found</returns>
    Task<ProductFeature?> GetFeatureByIdAsync(Guid featureId);

    /// <summary>
    /// Gets features for a specific tier
    /// </summary>
    /// <param name="tierId">Tier ID</param>
    /// <returns>List of features for the tier</returns>
    Task<IEnumerable<ProductFeature>> GetFeaturesByTierAsync(Guid tierId);

    /// <summary>
    /// Gets a feature by tier and feature code
    /// </summary>
    /// <param name="tierId">Tier ID</param>
    /// <param name="featureCode">Feature code</param>
    /// <returns>Feature or null if not found</returns>
    Task<ProductFeature?> GetFeatureByCodeAsync(Guid tierId, string featureCode);

    /// <summary>
    /// Deletes a feature
    /// </summary>
    /// <param name="featureId">Feature ID</param>
    /// <param name="deletedBy">User deleting the feature</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteFeatureAsync(Guid featureId, string deletedBy);

    /// <summary>
    /// Checks if a feature exists
    /// </summary>
    /// <param name="featureId">Feature ID</param>
    /// <returns>True if exists</returns>
    Task<bool> FeatureExistsAsync(Guid featureId);

    /// <summary>
    /// Checks if a feature code exists for a tier
    /// </summary>
    /// <param name="tierId">Tier ID</param>
    /// <param name="featureCode">Feature code</param>
    /// <param name="excludeFeatureId">Feature ID to exclude from check (for updates)</param>
    /// <returns>True if code exists</returns>
    Task<bool> FeatureCodeExistsAsync(Guid tierId, string featureCode, Guid? excludeFeatureId = null);

    /// <summary>
    /// Validates feature data
    /// </summary>
    /// <param name="feature">Feature to validate</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> ValidateFeatureAsync(ProductFeature feature);

    /// <summary>
    /// Gets all features with optional filtering
    /// </summary>
    /// <param name="searchTerm">Search term for feature name or description</param>
    /// <param name="featureType">Filter by feature type</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of features</returns>
    Task<IEnumerable<ProductFeature>> GetAllFeaturesAsync(
        string? searchTerm = null,
        string? featureType = null,
        int pageNumber = 1,
        int pageSize = 50);

    /// <summary>
    /// Gets the total count of features with optional filtering
    /// </summary>
    /// <param name="searchTerm">Search term for feature name or description</param>
    /// <param name="featureType">Filter by feature type</param>
    /// <returns>Total count</returns>
    Task<int> GetFeatureCountAsync(string? searchTerm = null, string? featureType = null);

    /// <summary>
    /// Gets features by type
    /// </summary>
    /// <param name="featureType">Feature type</param>
    /// <returns>List of features of the specified type</returns>
    Task<IEnumerable<ProductFeature>> GetFeaturesByTypeAsync(string featureType);

    /// <summary>
    /// Copies features from one tier to another
    /// </summary>
    /// <param name="sourceTierId">Source tier ID</param>
    /// <param name="targetTierId">Target tier ID</param>
    /// <param name="copiedBy">User copying the features</param>
    /// <returns>Number of features copied</returns>
    Task<int> CopyFeaturesAsync(Guid sourceTierId, Guid targetTierId, string copiedBy);

    /// <summary>
    /// Gets usage statistics for features
    /// </summary>
    /// <returns>Feature usage statistics</returns>
    Task<FeatureUsageStatistics> GetFeatureUsageStatisticsAsync();
}
