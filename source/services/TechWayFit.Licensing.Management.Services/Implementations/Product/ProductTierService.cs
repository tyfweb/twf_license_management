using System.Formats.Asn1;
using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data; 

public class ProductTierService : IProductTierService
{
    IUnitOfWork _unitOfWork;
    ILogger<ProductTierService> _logger;
    public ProductTierService(IUnitOfWork unitOfWork, ILogger<ProductTierService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    public async Task<ProductTier> CreateTierAsync(ProductTier tier, string createdBy)
    {
        if (tier == null) throw new ArgumentNullException(nameof(tier));
        if (string.IsNullOrWhiteSpace(createdBy)) throw new ArgumentException("Created by cannot be null or empty", nameof(createdBy));

        // Validate tier
        var validationResult = ValidateTierAsync(tier).Result;
        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation failed for tier: {TierName}. Errors: {Errors}", tier.Name, validationResult.Errors);
            throw new InvalidOperationException("Tier validation failed");
        }


        await _unitOfWork.ProductTiers.AddAsync(tier);
        await _unitOfWork.SaveChangesAsync();

        return tier;
    }
    public async Task<bool> DeleteTierAsync(Guid tierId, string deletedBy)
    {
        if (tierId == Guid.Empty) throw new ArgumentException("Tier ID cannot be empty", nameof(tierId));
        if (string.IsNullOrWhiteSpace(deletedBy)) throw new ArgumentException("Deleted by cannot be null or empty", nameof(deletedBy));

        try
        {
            _logger.LogInformation("Deleting tier {TierId} by {DeletedBy}", tierId, deletedBy);

            // Check if tier exists
            var tierExists = await TierExistsAsync(tierId);
            if (!tierExists)
            {
                _logger.LogWarning("Attempted to delete non-existing tier with ID: {TierId}", tierId);
                return false;
            }

            // Soft delete the tier
            var result = await _unitOfWork.ProductTiers.DeleteAsync(tierId);
            
            if (result)
            {
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Tier {TierId} deleted successfully", tierId);
            }
            else
            {
                _logger.LogWarning("Failed to delete tier {TierId}", tierId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tier {TierId}", tierId);
            throw;
        }
    }


    public async Task<ProductTier?> GetTierByIdAsync(Guid tierId)
    {
        if (tierId == Guid.Empty) throw new ArgumentException("Tier ID cannot be empty", nameof(tierId));

        // Fetch tier by ID
        var tierEntity = await _unitOfWork.ProductTiers.GetByIdAsync(tierId);
        if (tierEntity == null)
        {
            _logger.LogWarning("Tier with ID {TierId} not found", tierId);
            return null;
        }

        return tierEntity;
    }

    public async Task<ProductTier?> GetTierByNameAsync(Guid productId, string tierName)
    {
        if (productId == Guid.Empty) 
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));
        if (string.IsNullOrWhiteSpace(tierName))
            throw new ArgumentException("Tier name cannot be null or empty", nameof(tierName));

        try
        {
            _logger.LogInformation("Retrieving tier by name {TierName} for product {ProductId}", tierName, productId);

            var tiers = await _unitOfWork.ProductTiers.GetByProductIdAsync(productId);
            var tier = tiers.FirstOrDefault(t => 
                string.Equals(t.Name, tierName, StringComparison.OrdinalIgnoreCase));

            if (tier == null)
            {
                _logger.LogWarning("Tier with name {TierName} not found for product {ProductId}", tierName, productId);
            }

            return tier;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tier by name {TierName} for product {ProductId}", tierName, productId);
            throw;
        }
    }
 

    public Task<IEnumerable<ProductTier>> GetTiersByProductAsync(Guid productId)
    {
        if (productId == Guid.Empty) throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        // Fetch tiers by product ID
        var tiers = _unitOfWork.ProductTiers.GetByProductIdAsync(productId);
        return tiers;
    }

    public async Task<bool> TierExistsAsync(Guid tierId)
    {
        if (tierId == Guid.Empty)
            throw new ArgumentException("Tier ID cannot be empty", nameof(tierId));

        try
        {
            _logger.LogInformation("Checking if tier {TierId} exists", tierId);
            
            var exists = await _unitOfWork.ProductTiers.ExistsAsync(tierId);
            
            _logger.LogInformation("Tier {TierId} exists: {Exists}", tierId, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if tier {TierId} exists", tierId);
            throw;
        }
    }

    public async Task<bool> TierNameExistsAsync(Guid productId, string tierName, Guid? excludeTierId = null)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));
        if (string.IsNullOrWhiteSpace(tierName))
            throw new ArgumentException("Tier name cannot be null or empty", nameof(tierName));

        try
        {
            _logger.LogInformation("Checking if tier name {TierName} exists for product {ProductId}", tierName, productId);

            var tiers = await _unitOfWork.ProductTiers.GetByProductIdAsync(productId);
            var matchingTiers = tiers.Where(t => 
                string.Equals(t.Name, tierName, StringComparison.OrdinalIgnoreCase));

            if (excludeTierId.HasValue)
            {
                matchingTiers = matchingTiers.Where(t => t.TierId != excludeTierId.Value);
            }

            var exists = matchingTiers.Any();

            _logger.LogInformation("Tier name {TierName} exists for product {ProductId}: {Exists}", 
                tierName, productId, exists);
            
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if tier name {TierName} exists for product {ProductId}", 
                tierName, productId);
            throw;
        }
    }

    public Task<ProductTier> UpdateTierAsync(ProductTier tier, string updatedBy)
    {
        if (tier == null) throw new ArgumentNullException(nameof(tier));
        if (string.IsNullOrWhiteSpace(updatedBy)) throw new ArgumentException("Updated by cannot be null or empty", nameof(updatedBy));

        // Validate tier
        var validationResult = ValidateTierAsync(tier).Result;
        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation failed for tier: {TierName}. Errors: {Errors}", tier.Name, validationResult.Errors);
            throw new InvalidOperationException("Tier validation failed");
        }

        // Fetch existing tier
        var existingTier = GetTierByIdAsync(tier.TierId).Result;
        if (existingTier == null)
        {
            _logger.LogWarning("Attempted to update non-existing tier with ID: {TierId}", tier.TierId);
            throw new KeyNotFoundException($"Tier with ID {tier.TierId} not found");
        }

        // Map to entity and update
       
        existingTier.ProductId = tier.ProductId; // Ensure product ID is set
        existingTier.TierId = tier.TierId; // Ensure tier ID is set
        existingTier.Name = tier.Name;
        existingTier.Description = tier.Description; 

        _unitOfWork.ProductTiers.UpdateAsync(existingTier.TierId, existingTier);
        _unitOfWork.SaveChangesAsync();

        return Task.FromResult(tier);
    }

    public async Task<ValidationResult> ValidateTierAsync(ProductTier tier)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(tier.ProductId);
        if (product == null)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = new List<string> { "Product not found" }
            };
        }

        var validationErrors = new List<string>();
        if (string.IsNullOrWhiteSpace(tier.Name))
        {
            validationErrors.Add("Tier name cannot be empty");
        }

        if (validationErrors.Count > 0)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = validationErrors
            };
        }   
        return new ValidationResult
        {
            IsValid = true,
            Errors = new List<string>()
        };

    }
}