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

        return tier;
    }
    public Task<bool> DeleteTierAsync(Guid tierId, string deletedBy)
    {
        if (tierId == Guid.Empty) throw new ArgumentException("Tier ID cannot be empty", nameof(tierId));
        if (string.IsNullOrWhiteSpace(deletedBy)) throw new ArgumentException("Deleted by cannot be null or empty", nameof(deletedBy));

        // Check if tier exists
        var tierExists = TierExistsAsync(tierId).Result;
        if (!tierExists)
        {
            _logger.LogWarning("Attempted to delete non-existing tier with ID: {TierId}", tierId);
            return Task.FromResult(false);
        }

        // Soft delete the tier
        return _unitOfWork.ProductTiers.DeleteAsync(tierId);
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

    public Task<ProductTier?> GetTierByNameAsync(Guid productId, string tierName)
    {
        throw new NotImplementedException();
    }
 

    public Task<IEnumerable<ProductTier>> GetTiersByProductAsync(Guid productId)
    {
        if (productId == Guid.Empty) throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        // Fetch tiers by product ID
        var tiers = _unitOfWork.ProductTiers.GetByProductIdAsync(productId);
        return tiers;
    }

    public Task<bool> TierExistsAsync(Guid tierId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> TierNameExistsAsync(Guid productId, string tierName, Guid? excludeTierId = null)
    {
        throw new NotImplementedException();
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