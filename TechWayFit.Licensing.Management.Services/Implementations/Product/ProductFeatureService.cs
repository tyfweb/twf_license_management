using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Infrastructure.Contracts.Repositories.Product;
using TechWayFit.Licensing.Infrastructure.Models.Entities.Products;
using TechWayFit.Licensing.Infrastructure.Models.Search;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Management.Services.Implementations.Product;

/// <summary>
/// Implementation of the Product Feature service
/// </summary>
public class ProductFeatureService : IProductFeatureService
{
    private readonly IProductFeatureRepository _productFeatureRepository;
    private readonly ILogger<ProductFeatureService> _logger;

    public ProductFeatureService(
        IProductFeatureRepository productFeatureRepository,
        ILogger<ProductFeatureService> logger)
    {
        _productFeatureRepository = productFeatureRepository ?? throw new ArgumentNullException(nameof(productFeatureRepository));
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
            // Generate ID if not provided
            if (string.IsNullOrWhiteSpace(feature.FeatureId))
            {
                feature.FeatureId = Guid.NewGuid().ToString();
            }

            // Map to entity
            var entity = ProductFeatureEntity.FromModel(feature);
            entity.CreatedBy = createdBy;
            entity.CreatedOn = DateTime.UtcNow;
            entity.UpdatedBy = createdBy;
            entity.UpdatedOn = DateTime.UtcNow;

            // Save to repository
            var createdEntity = await _productFeatureRepository.AddAsync(entity);
            
            // Map back to model
            var result = createdEntity.ToModel();
            
            _logger.LogInformation("Successfully created product feature with ID: {FeatureId}", result.FeatureId);
            return result;
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
        if (string.IsNullOrWhiteSpace(feature.FeatureId))
            throw new ArgumentException("FeatureId cannot be null or empty", nameof(feature.FeatureId));

        // Check if exists
        var existingEntity = await _productFeatureRepository.GetByIdAsync(feature.FeatureId);
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
            var updatedData = ProductFeatureEntity.FromModel(feature);
            
            // Update properties manually to preserve audit fields
            existingEntity.Name = updatedData.Name;
            existingEntity.Description = updatedData.Description;
            existingEntity.Code = updatedData.Code;
            existingEntity.IsEnabled = updatedData.IsEnabled;
            existingEntity.ProductId = updatedData.ProductId;
            existingEntity.TierId = updatedData.TierId;
            existingEntity.DisplayOrder = updatedData.DisplayOrder;
            existingEntity.SupportFromVersion = updatedData.SupportFromVersion;
            existingEntity.SupportToVersion = updatedData.SupportToVersion;
            existingEntity.FeatureUsageJson = updatedData.FeatureUsageJson;
            
            existingEntity.UpdatedBy = updatedBy;
            existingEntity.UpdatedOn = DateTime.UtcNow;

            // Update in repository
            var updatedEntity = await _productFeatureRepository.UpdateAsync(existingEntity);
            
            // Map back to model
            var result = updatedEntity.ToModel();
            
            _logger.LogInformation("Successfully updated product feature: {FeatureId}", result.FeatureId);
            return result;
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
    public async Task<ProductFeature?> GetFeatureByIdAsync(string featureId)
    {
        if (string.IsNullOrWhiteSpace(featureId))
            throw new ArgumentException("FeatureId cannot be null or empty", nameof(featureId));

        try
        {
            var entity = await _productFeatureRepository.GetByIdAsync(featureId);
            return entity?.ToModel();
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
    public async Task<IEnumerable<ProductFeature>> GetFeaturesByTierAsync(string tierId)
    {
        if (string.IsNullOrWhiteSpace(tierId))
            throw new ArgumentException("TierId cannot be null or empty", nameof(tierId));

        try
        {
            // TODO: Implement GetByTierIdAsync in repository
            _logger.LogWarning("GetFeaturesByTierAsync not fully implemented - repository method GetByTierIdAsync missing");
            
            var searchRequest = new SearchRequest<ProductFeatureEntity>
            {
                Filters = new List<Func<ProductFeatureEntity, bool>>
                {
                    f => f.TierId == tierId && f.IsEnabled
                }
            };
            
            var searchResult = await _productFeatureRepository.SearchAsync(searchRequest);
            return searchResult.Results.Select(e => e.ToModel());
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
    public async Task<ProductFeature?> GetFeatureByCodeAsync(string tierId, string featureCode)
    {
        if (string.IsNullOrWhiteSpace(tierId))
            throw new ArgumentException("TierId cannot be null or empty", nameof(tierId));
        if (string.IsNullOrWhiteSpace(featureCode))
            throw new ArgumentException("FeatureCode cannot be null or empty", nameof(featureCode));

        try
        {
            // TODO: Implement GetByCodeAsync in repository
            _logger.LogWarning("GetFeatureByCodeAsync not fully implemented - repository method GetByCodeAsync missing");
            
            var searchRequest = new SearchRequest<ProductFeatureEntity>
            {
                Filters = new List<Func<ProductFeatureEntity, bool>>
                {
                    f => f.TierId == tierId && f.Code == featureCode
                }
            };
            
            var searchResult = await _productFeatureRepository.SearchAsync(searchRequest);
            var entity = searchResult.Results.FirstOrDefault();
            return entity?.ToModel();
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
    public async Task<bool> DeleteFeatureAsync(string featureId, string deletedBy)
    {
        if (string.IsNullOrWhiteSpace(featureId))
            throw new ArgumentException("FeatureId cannot be null or empty", nameof(featureId));
        if (string.IsNullOrWhiteSpace(deletedBy))
            throw new ArgumentException("DeletedBy cannot be null or empty", nameof(deletedBy));

        try
        {
            var entity = await _productFeatureRepository.GetByIdAsync(featureId);
            if (entity == null)
            {
                _logger.LogWarning("Product feature not found for deletion: {FeatureId}", featureId);
                return false;
            }

            // TODO: Check for related license features before deletion
            _logger.LogWarning("Delete operation should check for related license features first");

            await _productFeatureRepository.DeleteAsync(featureId);
            
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
    public async Task<bool> FeatureExistsAsync(string featureId)
    {
        if (string.IsNullOrWhiteSpace(featureId))
            return false;

        try
        {
            var entity = await _productFeatureRepository.GetByIdAsync(featureId);
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
    public async Task<bool> FeatureCodeExistsAsync(string tierId, string featureCode, string? excludeFeatureId = null)
    {
        if (string.IsNullOrWhiteSpace(tierId) || string.IsNullOrWhiteSpace(featureCode))
            return false;

        try
        {
            // TODO: Implement IsCodeUniqueAsync in repository
            _logger.LogWarning("FeatureCodeExistsAsync not fully implemented - repository method IsCodeUniqueAsync missing");
            
            var searchRequest = new SearchRequest<ProductFeatureEntity>
            {
                Filters = new List<Func<ProductFeatureEntity, bool>>
                {
                    f => f.TierId == tierId && f.Code == featureCode
                }
            };

            if (!string.IsNullOrWhiteSpace(excludeFeatureId))
            {
                searchRequest.Filters.Add(f => f.FeatureId != excludeFeatureId);
            }
            
            var searchResult = await _productFeatureRepository.SearchAsync(searchRequest);
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

        if (string.IsNullOrWhiteSpace(feature.ProductId))
            errors.Add("Product ID is required");

        if (string.IsNullOrWhiteSpace(feature.TierId))
            errors.Add("Tier ID is required");

        // Business rule validations
        if (!string.IsNullOrWhiteSpace(feature.Code) && !string.IsNullOrWhiteSpace(feature.TierId))
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
            var searchRequest = new SearchRequest<ProductFeatureEntity>
            {
                Filters = new List<Func<ProductFeatureEntity, bool>>(),
                Page = pageNumber,
                PageSize = pageSize
            };
            
            // Apply search term filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchRequest.Filters.Add(f => 
                    f.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    f.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    f.Code.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // TODO: Apply feature type filter when available in entity
            if (!string.IsNullOrWhiteSpace(featureType))
            {
                _logger.LogWarning("Feature type filtering not implemented - FeatureType property missing in entity");
            }
            
            var searchResult = await _productFeatureRepository.SearchAsync(searchRequest);
            return searchResult.Results.Select(e => e.ToModel());
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
            var searchRequest = new SearchRequest<ProductFeatureEntity>
            {
                Filters = new List<Func<ProductFeatureEntity, bool>>()
            };
            
            // Apply search term filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchRequest.Filters.Add(f => 
                    f.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    f.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    f.Code.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // TODO: Apply feature type filter when available in entity
            if (!string.IsNullOrWhiteSpace(featureType))
            {
                _logger.LogWarning("Feature type filtering not implemented - FeatureType property missing in entity");
            }
            
            var searchResult = await _productFeatureRepository.SearchAsync(searchRequest);
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
    public async Task<int> CopyFeaturesAsync(string sourceTierId, string targetTierId, string copiedBy)
    {
        if (string.IsNullOrWhiteSpace(sourceTierId))
            throw new ArgumentException("SourceTierId cannot be null or empty", nameof(sourceTierId));
        if (string.IsNullOrWhiteSpace(targetTierId))
            throw new ArgumentException("TargetTierId cannot be null or empty", nameof(targetTierId));
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
                    FeatureId = Guid.NewGuid().ToString(),
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
            
            var searchRequest = new SearchRequest<ProductFeatureEntity>
            {
                Filters = new List<Func<ProductFeatureEntity, bool>>()
            };
            
            var searchResult = await _productFeatureRepository.SearchAsync(searchRequest);
            var features = searchResult.Results;

            // Basic statistics
            var statistics = new FeatureUsageStatistics
            {
                TotalFeatures = features.Count(),
                FeaturesByType = new Dictionary<string, int>(), // TODO: Group by feature type when available
                MostUsedFeatures = new Dictionary<string, int>(), // TODO: Implement usage tracking
                FeaturesByTier = features.GroupBy(f => f.TierId).ToDictionary(g => g.Key, g => g.Count())
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
}
