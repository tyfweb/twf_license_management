using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;

namespace TechWayFit.Licensing.Management.Services.Implementations.Product;

/// <summary>
/// Implementation of the Product Feature Tier Mapping service
/// </summary>
public class ProductFeatureTierMappingService : IProductFeatureTierMappingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductFeatureTierMappingService> _logger;

    public ProductFeatureTierMappingService(
        IUnitOfWork unitOfWork,
        ILogger<ProductFeatureTierMappingService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all features mapped to a specific tier
    /// </summary>
    public async Task<IEnumerable<ProductFeatureTierMapping>> GetFeaturesForTierAsync(Guid tierId)
    {
        if (tierId == Guid.Empty)
            throw new ArgumentException("TierId cannot be empty", nameof(tierId));

        try
        {
            var mappings = await _unitOfWork.ProductFeatureTierMappings.GetByTierIdAsync(tierId);
            return mappings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting features for tier: {TierId}", tierId);
            throw;
        }
    }

    /// <summary>
    /// Gets all tiers that a specific feature is mapped to
    /// </summary>
    public async Task<IEnumerable<ProductFeatureTierMapping>> GetTiersForFeatureAsync(Guid featureId)
    {
        if (featureId == Guid.Empty)
            throw new ArgumentException("FeatureId cannot be empty", nameof(featureId));

        try
        {
            var mappings = await _unitOfWork.ProductFeatureTierMappings.GetByFeatureIdAsync(featureId);
            return mappings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tiers for feature: {FeatureId}", featureId);
            throw;
        }
    }

    /// <summary>
    /// Gets all feature-tier mappings for a product
    /// </summary>
    public async Task<IEnumerable<ProductFeatureTierMapping>> GetMappingsForProductAsync(Guid productId)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty", nameof(productId));

        try
        {
            var mappings = await _unitOfWork.ProductFeatureTierMappings.GetByProductIdAsync(productId);
            return mappings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mappings for product: {ProductId}", productId);
            throw;
        }
    }

    /// <summary>
    /// Sets the features for a specific tier (replaces existing mappings)
    /// </summary>
    public async Task<IEnumerable<ProductFeatureTierMapping>> SetTierFeaturesAsync(Guid tierId, IEnumerable<Guid> featureIds)
    {
        if (tierId == Guid.Empty)
            throw new ArgumentException("TierId cannot be empty", nameof(tierId));

        try
        {
            _logger.LogInformation("Setting features for tier {TierId}. Feature IDs: {FeatureIds}", 
                tierId, string.Join(", ", featureIds));

            // Get existing mappings
            var existingMappings = await _unitOfWork.ProductFeatureTierMappings.GetByTierIdAsync(tierId);
            
            // Remove existing mappings
            foreach (var mapping in existingMappings)
            {
                await _unitOfWork.ProductFeatureTierMappings.DeleteAsync(mapping.Id);
            }

            // Create new mappings
            var newMappings = new List<ProductFeatureTierMapping>();
            int displayOrder = 1;

            foreach (var featureId in featureIds)
            {
                var mapping = new ProductFeatureTierMapping
                {
                    Id = Guid.NewGuid(),
                    ProductFeatureId = featureId,
                    ProductTierId = tierId,
                    IsEnabled = true,
                    DisplayOrder = displayOrder++,
                    Configuration = null
                };

                await _unitOfWork.ProductFeatureTierMappings.AddAsync(mapping);
                newMappings.Add(mapping);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully set {Count} features for tier {TierId}", 
                newMappings.Count, tierId);

            return newMappings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting features for tier: {TierId}", tierId);
            throw;
        }
    }

    /// <summary>
    /// Adds a single feature to a tier
    /// </summary>
    public async Task<ProductFeatureTierMapping> AddFeatureToTierAsync(Guid tierId, Guid featureId)
    {
        if (tierId == Guid.Empty)
            throw new ArgumentException("TierId cannot be empty", nameof(tierId));
        if (featureId == Guid.Empty)
            throw new ArgumentException("FeatureId cannot be empty", nameof(featureId));

        try
        {
            // Check if mapping already exists
            var existingMappings = await _unitOfWork.ProductFeatureTierMappings.GetByTierIdAsync(tierId);
            var existingMapping = existingMappings.FirstOrDefault(m => m.ProductFeatureId == featureId);
            
            if (existingMapping != null)
            {
                _logger.LogWarning("Feature {FeatureId} is already mapped to tier {TierId}", featureId, tierId);
                return existingMapping;
            }

            // Get the next display order
            var maxDisplayOrder = existingMappings.Any() ? existingMappings.Max(m => m.DisplayOrder) : 0;

            var mapping = new ProductFeatureTierMapping
            {
                Id = Guid.NewGuid(),
                ProductFeatureId = featureId,
                ProductTierId = tierId,
                IsEnabled = true,
                DisplayOrder = maxDisplayOrder + 1,
                Configuration = null
            };

            await _unitOfWork.ProductFeatureTierMappings.AddAsync(mapping);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Added feature {FeatureId} to tier {TierId}", featureId, tierId);
            return mapping;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding feature {FeatureId} to tier {TierId}", featureId, tierId);
            throw;
        }
    }

    /// <summary>
    /// Removes a feature from a tier
    /// </summary>
    public async Task<bool> RemoveFeatureFromTierAsync(Guid tierId, Guid featureId)
    {
        if (tierId == Guid.Empty)
            throw new ArgumentException("TierId cannot be empty", nameof(tierId));
        if (featureId == Guid.Empty)
            throw new ArgumentException("FeatureId cannot be empty", nameof(featureId));

        try
        {
            var mappings = await _unitOfWork.ProductFeatureTierMappings.GetByTierIdAsync(tierId);
            var mapping = mappings.FirstOrDefault(m => m.ProductFeatureId == featureId);
            
            if (mapping == null)
            {
                _logger.LogWarning("No mapping found for feature {FeatureId} in tier {TierId}", featureId, tierId);
                return false;
            }

            await _unitOfWork.ProductFeatureTierMappings.DeleteAsync(mapping.Id);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Removed feature {FeatureId} from tier {TierId}", featureId, tierId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing feature {FeatureId} from tier {TierId}", featureId, tierId);
            throw;
        }
    }

    /// <summary>
    /// Checks if a feature is available in a specific tier
    /// </summary>
    public async Task<bool> IsFeatureAvailableInTierAsync(Guid tierId, Guid featureId)
    {
        if (tierId == Guid.Empty)
            throw new ArgumentException("TierId cannot be empty", nameof(tierId));
        if (featureId == Guid.Empty)
            throw new ArgumentException("FeatureId cannot be empty", nameof(featureId));

        try
        {
            var mappings = await _unitOfWork.ProductFeatureTierMappings.GetByTierIdAsync(tierId);
            return mappings.Any(m => m.ProductFeatureId == featureId && m.IsEnabled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if feature {FeatureId} is available in tier {TierId}", featureId, tierId);
            throw;
        }
    }

    /// <summary>
    /// Gets features that are available in a tier with their configuration
    /// </summary>
    public async Task<Dictionary<Guid, string?>> GetTierFeatureConfigurationsAsync(Guid tierId)
    {
        if (tierId == Guid.Empty)
            throw new ArgumentException("TierId cannot be empty", nameof(tierId));

        try
        {
            var mappings = await _unitOfWork.ProductFeatureTierMappings.GetByTierIdAsync(tierId);
            return mappings
                .Where(m => m.IsEnabled)
                .ToDictionary(m => m.ProductFeatureId, m => m.Configuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feature configurations for tier: {TierId}", tierId);
            throw;
        }
    }

    /// <summary>
    /// Updates the configuration for a specific feature-tier mapping
    /// </summary>
    public async Task<ProductFeatureTierMapping?> UpdateFeatureConfigurationAsync(Guid tierId, Guid featureId, string? configuration)
    {
        if (tierId == Guid.Empty)
            throw new ArgumentException("TierId cannot be empty", nameof(tierId));
        if (featureId == Guid.Empty)
            throw new ArgumentException("FeatureId cannot be empty", nameof(featureId));

        try
        {
            var mappings = await _unitOfWork.ProductFeatureTierMappings.GetByTierIdAsync(tierId);
            var mapping = mappings.FirstOrDefault(m => m.ProductFeatureId == featureId);
            
            if (mapping == null)
            {
                _logger.LogWarning("No mapping found for feature {FeatureId} in tier {TierId}", featureId, tierId);
                return null;
            }

            mapping.Configuration = configuration;
            await _unitOfWork.ProductFeatureTierMappings.UpdateAsync(mapping.Id, mapping);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Updated configuration for feature {FeatureId} in tier {TierId}", featureId, tierId);
            return mapping;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating configuration for feature {FeatureId} in tier {TierId}", featureId, tierId);
            throw;
        }
    }
}
