using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Infrastructure.Models.Search;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Management.Services.Implementations.Product;

/// <summary>
/// Implementation of the Product Feature service
/// </summary>
public class ProductFeatureService : IProductFeatureService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductFeatureService> _logger;

    public ProductFeatureService(
        IUnitOfWork unitOfWork,
        ILogger<ProductFeatureService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new product feature
    /// </summary>
    public async Task<ProductFeature> CreateFeatureAsync(ProductFeature feature, string createdBy)
    {
        _logger.LogInformation("Creating product feature: {FeatureName} for product: {ProductId}", feature.Name, feature.ProductId);

        // Input validation
        if (feature == null)
            throw new ArgumentNullException(nameof(feature));
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("CreatedBy cannot be null or empty", nameof(createdBy));

        // Business validation
        var validationResult = await ValidateFeatureAsync(feature);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors);
            throw new InvalidOperationException($"Product feature validation failed: {errors}");
        }

        try
        {
            
            // Save to repository
            var createdEntity = await _unitOfWork.ProductFeatures.AddAsync(feature);             
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Successfully created product feature with ID: {FeatureId}", createdEntity.FeatureId);
            return createdEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product feature: {FeatureName}", feature.Name);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing product feature
    /// </summary>
    public async Task<ProductFeature> UpdateFeatureAsync(ProductFeature feature, string updatedBy)
    {
        _logger.LogInformation("Updating product feature: {FeatureId}", feature.FeatureId);

        // Input validation
        if (feature == null)
            throw new ArgumentNullException(nameof(feature));
        if (string.IsNullOrWhiteSpace(updatedBy))
            throw new ArgumentException("UpdatedBy cannot be null or empty", nameof(updatedBy));
        if (Guid.Empty.Equals(feature.FeatureId))
            throw new ArgumentException("FeatureId cannot be empty", nameof(feature.FeatureId));

        // Check if exists
        var existingEntity = await _unitOfWork.ProductFeatures.GetByIdAsync(feature.FeatureId);
        if (existingEntity == null)
        {
            throw new InvalidOperationException($"Product feature with ID {feature.FeatureId} not found");
        }

        // Business validation
        var validationResult = await ValidateFeatureAsync(feature);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors);
            throw new InvalidOperationException($"Product feature validation failed: {errors}");
        }

        try
        {
            // Map updates to existing entity 
            
            // Update properties manually to preserve audit fields
            existingEntity.Name = feature.Name;
            existingEntity.Description = feature.Description;
            existingEntity.Code = feature.Code;
            existingEntity.IsEnabled = feature.IsEnabled;
            existingEntity.ProductId = feature.ProductId;
            existingEntity.TierId = feature.TierId;
            existingEntity.DisplayOrder = feature.DisplayOrder;
            existingEntity.SupportFromVersion = feature.SupportFromVersion;
            existingEntity.SupportToVersion = feature.SupportToVersion;  

            // Update in repository
            var updatedEntity = await _unitOfWork.ProductFeatures.UpdateAsync(existingEntity.FeatureId, existingEntity);

            _logger.LogInformation("Successfully updated product feature: {FeatureId}", updatedEntity.FeatureId);
            return updatedEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product feature: {FeatureId}", feature.FeatureId);
            throw;
        }
    }

    /// <summary>
    /// Gets a feature by ID
    /// </summary>
    public async Task<ProductFeature?> GetFeatureByIdAsync(Guid featureId)
    {
        if (featureId == Guid.Empty)
            throw new ArgumentException("FeatureId cannot be empty", nameof(featureId));

        try
        {
            var entity = await _unitOfWork.ProductFeatures.GetByIdAsync(featureId);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product feature by ID: {FeatureId}", featureId);
            throw;
        }
    }

    /// <summary>
    /// Gets features for a specific tier
    /// </summary>
    public async Task<IEnumerable<ProductFeature>> GetFeaturesByTierAsync(Guid tierId)
    {
        if (tierId == Guid.Empty)
            throw new ArgumentException("TierId cannot be empty", nameof(tierId));

        try
        {
            // TODO: Implement GetByTierIdAsync in repository
            _logger.LogWarning("GetFeaturesByTierAsync not fully implemented - repository method GetByTierIdAsync missing");
            
            var searchRequest = new SearchRequest<ProductFeature>
            {
                Filters = {
                    { "TierId", tierId},
                    { "IsEnabled", true} }
            };
            
            var searchResult = await _unitOfWork.ProductFeatures.SearchAsync(searchRequest);
            return searchResult.Results ;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting features by tier: {TierId}", tierId);
            throw;
        }
    }

    /// <summary>
    /// Gets a feature by tier and feature code
    /// </summary>
    public async Task<ProductFeature?> GetFeatureByCodeAsync(Guid tierId, string featureCode)
    {
        if (tierId == Guid.Empty)
            throw new ArgumentException("TierId cannot be empty", nameof(tierId));
        if (string.IsNullOrWhiteSpace(featureCode))
            throw new ArgumentException("FeatureCode cannot be null or empty", nameof(featureCode));

        try
        {
            // TODO: Implement GetByCodeAsync in repository
            _logger.LogWarning("GetFeatureByCodeAsync not fully implemented - repository method GetByCodeAsync missing");
            
            var searchRequest = new SearchRequest<ProductFeature>
            {
                Filters = {
                    { "TierId", tierId},
                    { "Code", featureCode}
                }
            };
            
            var searchResult = await _unitOfWork.ProductFeatures.FindOneAsync(searchRequest);
            var entity = searchResult;
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feature by code: {TierId}/{FeatureCode}", tierId, featureCode);
            throw;
        }
    }

    /// <summary>
    /// Deletes a feature
    /// </summary>
    public async Task<bool> DeleteFeatureAsync(Guid featureId, string deletedBy)
    {
        if (featureId == Guid.Empty)
            throw new ArgumentException("FeatureId cannot be empty", nameof(featureId));
        if (string.IsNullOrWhiteSpace(deletedBy))
            throw new ArgumentException("DeletedBy cannot be null or empty", nameof(deletedBy));

        try
        {
            var entity = await _unitOfWork.ProductFeatures.GetByIdAsync(featureId);
            if (entity == null)
            {
                _logger.LogWarning("Product feature not found for deletion: {FeatureId}", featureId);
                return false;
            }

            // TODO: Check for related license features before deletion
            _logger.LogWarning("Delete operation should check for related license features first");

            await _unitOfWork.ProductFeatures.DeleteAsync(featureId);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Successfully deleted product feature: {FeatureId}", featureId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product feature: {FeatureId}", featureId);
            throw;
        }
    }

    /// <summary>
    /// Checks if a feature exists
    /// </summary>
    public async Task<bool> FeatureExistsAsync(Guid featureId)
    {
        if (featureId == Guid.Empty)
            return false;

        try
        {
            var entity = await _unitOfWork.ProductFeatures.GetByIdAsync(featureId);
            return entity != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if product feature exists: {FeatureId}", featureId);
            throw;
        }
    }

    /// <summary>
    /// Checks if a feature code exists for a tier
    /// </summary>
    public async Task<bool> FeatureCodeExistsAsync(Guid tierId, string featureCode, Guid? excludeFeatureId = null)
    {
        if (tierId == Guid.Empty || string.IsNullOrWhiteSpace(featureCode))
            return false;

        try
        {
            // TODO: Implement IsCodeUniqueAsync in repository
            _logger.LogWarning("FeatureCodeExistsAsync not fully implemented - repository method IsCodeUniqueAsync missing");
            
            var searchRequest = new SearchRequest<ProductFeature>
            {
                Filters = { {"TierId", tierId}, {"Code", featureCode} }
            };

           /* if (excludeFeatureId.HasValue)
            {
                searchRequest.Filters.Add("Id", excludeFeatureId.Value);
            }
            */
            
            var searchResult = await _unitOfWork.ProductFeatures.SearchAsync(searchRequest);
            return searchResult.Results.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if feature code exists: {TierId}/{FeatureCode}", tierId, featureCode);
            throw;
        }
    }

    /// <summary>
    /// Validates feature data
    /// </summary>
    public async Task<ValidationResult> ValidateFeatureAsync(ProductFeature feature)
    {
        var errors = new List<string>();

        if (feature == null)
        {
            errors.Add("Product feature cannot be null");
            return ValidationResult.Failure(errors.ToArray());
        }

        // Required field validations
        if (string.IsNullOrWhiteSpace(feature.Name))
            errors.Add("Feature name is required");

        if (string.IsNullOrWhiteSpace(feature.Code))
            errors.Add("Feature code is required");

        if (feature.ProductId == Guid.Empty)
            errors.Add("Product ID is required");

        if (feature.TierId == Guid.Empty)
            errors.Add("Tier ID is required");

        // Business rule validations
        if (!string.IsNullOrWhiteSpace(feature.Code) && feature.TierId != Guid.Empty)
        {
            // Check for duplicate feature code within the tier
            try
            {
                var codeExists = await FeatureCodeExistsAsync(feature.TierId, feature.Code, feature.FeatureId);
                if (codeExists)
                {
                    errors.Add($"Feature code '{feature.Code}' already exists in tier '{feature.TierId}'");
                }
            }
            catch
            {
                // Ignore errors during validation lookup
            }
        }

        // Display order validation
        if (feature.DisplayOrder < 0)
            errors.Add("Display order must be non-negative");

        return errors.Any() ? ValidationResult.Failure(errors.ToArray()) : ValidationResult.Success();
    }

    /// <summary>
    /// Gets all features with optional filtering
    /// </summary>
    public async Task<IEnumerable<ProductFeature>> GetAllFeaturesAsync(
        string? searchTerm = null,
        string? featureType = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        try
        {
            var searchRequest = new SearchRequest<ProductFeature>
            {
                Query=searchTerm,
                Page = pageNumber,
                PageSize = pageSize,
                Filters = new Dictionary<string, object>()
            };
            
            // TODO: Apply feature type filter when available in entity
            if (!string.IsNullOrWhiteSpace(featureType))
            {
                _logger.LogWarning("Feature type filtering not implemented - FeatureType property missing in entity");
            }

            var searchResult = await _unitOfWork.ProductFeatures.SearchAsync(searchRequest);
            return searchResult.Results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all features");
            throw;
        }
    }

    /// <summary>
    /// Gets the total count of features with optional filtering
    /// </summary>
    public async Task<int> GetFeatureCountAsync(string? searchTerm = null, string? featureType = null)
    {
        try
        {
            var searchRequest = new SearchRequest<ProductFeature>
            {
                Query = searchTerm,
                Filters = new Dictionary<string, object>()             
            };
            var searchResult = await _unitOfWork.ProductFeatures.SearchAsync(searchRequest);
            return searchResult.TotalCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feature count");
            throw;
        }
    }

    /// <summary>
    /// Gets features by type
    /// </summary>
    public async Task<IEnumerable<ProductFeature>> GetFeaturesByTypeAsync(string featureType)
    {
        if (string.IsNullOrWhiteSpace(featureType))
            throw new ArgumentException("FeatureType cannot be null or empty", nameof(featureType));

        try
        {
            // TODO: Implement when FeatureType property is available in entity
            _logger.LogWarning("GetFeaturesByTypeAsync not implemented - FeatureType property missing in entity");
            await Task.CompletedTask;
            return Enumerable.Empty<ProductFeature>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting features by type: {FeatureType}", featureType);
            throw;
        }
    }

    /// <summary>
    /// Copies features from one tier to another
    /// </summary>
    public async Task<int> CopyFeaturesAsync(Guid sourceTierId, Guid targetTierId, string copiedBy)
    {
        if (sourceTierId == Guid.Empty)
            throw new ArgumentException("SourceTierId cannot be empty", nameof(sourceTierId));
        if (targetTierId == Guid.Empty)
            throw new ArgumentException("TargetTierId cannot be empty", nameof(targetTierId));
        if (string.IsNullOrWhiteSpace(copiedBy))
            throw new ArgumentException("CopiedBy cannot be null or empty", nameof(copiedBy));

        try
        {
            _logger.LogInformation("Copying features from tier {SourceTierId} to tier {TargetTierId}", sourceTierId, targetTierId);

            // Get source features
            var sourceFeatures = await GetFeaturesByTierAsync(sourceTierId);
            
            int copiedCount = 0;
            foreach (var sourceFeature in sourceFeatures)
            {
                // Create a copy with new ID
                var copiedFeature = new ProductFeature
                {
                    FeatureId = Guid.NewGuid(),
                    ProductId = sourceFeature.ProductId,
                    TierId = targetTierId,
                    Name = sourceFeature.Name,
                    Description = sourceFeature.Description,
                    Code = sourceFeature.Code,
                    IsEnabled = sourceFeature.IsEnabled,
                    DisplayOrder = sourceFeature.DisplayOrder,
                    SupportFromVersion = sourceFeature.SupportFromVersion,
                    SupportToVersion = sourceFeature.SupportToVersion,
                    Usage = sourceFeature.Usage
                };

                // Check if feature code already exists in target tier
                var codeExists = await FeatureCodeExistsAsync(targetTierId, copiedFeature.Code);
                if (!codeExists)
                {
                    await CreateFeatureAsync(copiedFeature, copiedBy);
                    copiedCount++;
                }
                else
                {
                    _logger.LogWarning("Skipping feature copy - code {Code} already exists in target tier {TargetTierId}", 
                        copiedFeature.Code, targetTierId);
                }
            }

            _logger.LogInformation("Successfully copied {Count} features from tier {SourceTierId} to tier {TargetTierId}", 
                copiedCount, sourceTierId, targetTierId);
            
            return copiedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying features from tier {SourceTierId} to tier {TargetTierId}", sourceTierId, targetTierId);
            throw;
        }
    }

    /// <summary>
    /// Gets usage statistics for features
    /// </summary>
    public async Task<FeatureUsageStatistics> GetFeatureUsageStatisticsAsync()
    {
        try
        {
            // TODO: Implement when proper statistics queries are available
            _logger.LogWarning("GetFeatureUsageStatisticsAsync not fully implemented - requires complex statistics queries");
            
            var searchRequest = new SearchRequest<ProductFeature>
            {
                Filters = new Dictionary<string, object>()
            };
            
            var searchResult = await _unitOfWork.ProductFeatures.SearchAsync(searchRequest);
            var features = searchResult.Results;

            // Basic statistics
            var statistics = new FeatureUsageStatistics
            {
                TotalFeatures = features.Count(),
                FeaturesByType = new Dictionary<string, int>(), // TODO: Group by feature type when available
                MostUsedFeatures = new Dictionary<string, int>(), // TODO: Implement usage tracking
                FeaturesByTier = features.GroupBy(f => f.TierId).ToDictionary(g => g.Key.ToString(), g => g.Count())
            };

            await Task.CompletedTask;
            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feature usage statistics");
            throw;
        }
    }

    public async Task<IEnumerable<ProductFeature>> GetFeaturesByproductIdAsync(Guid productId)
    {
        var data = await _unitOfWork.ProductFeatures.GetByProductIdAsync(productId);
        return data.Select(f => f);
    }

    /// <summary>
    /// Gets features that are applicable to a specific version (current version or previous versions)
    /// </summary>
    public async Task<IEnumerable<ProductFeature>> GetFeaturesByProductVersionIdAsync(Guid productId, Guid versionId)
    {
        _logger.LogInformation("Getting features for product {ProductId} and version {VersionId}", productId, versionId);

        try
        {
            return await _unitOfWork.ProductFeatures.GetFeaturesByProductVersionIdAsync(productId, versionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting features for product {ProductId} and version {VersionId}", productId, versionId);
            throw;
        }
    }
}
