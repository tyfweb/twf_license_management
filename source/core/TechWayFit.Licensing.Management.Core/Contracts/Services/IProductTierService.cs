using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Management.Core.Contracts.Services;

/// <summary>
/// Service interface for managing product tiers
/// </summary>
public interface IProductTierService
{
    /// <summary>
    /// Creates a new product tier
    /// </summary>
    /// <param name="tier">Tier to create</param>
    /// <param name="createdBy">User creating the tier</param>
    /// <returns>Created tier</returns>
    Task<ProductTier> CreateTierAsync(ProductTier tier, string createdBy);

    /// <summary>
    /// Updates an existing product tier
    /// </summary>
    /// <param name="tier">Tier to update</param>
    /// <param name="updatedBy">User updating the tier</param>
    /// <returns>Updated tier</returns>
    Task<ProductTier> UpdateTierAsync(ProductTier tier, string updatedBy);

    /// <summary>
    /// Gets a tier by ID
    /// </summary>
    /// <param name="tierId">Tier ID</param>
    /// <returns>Tier or null if not found</returns>
    Task<ProductTier?> GetTierByIdAsync(Guid tierId);

    /// <summary>
    /// Gets tiers for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>List of tiers for the product</returns>
    Task<IEnumerable<ProductTier>> GetTiersByProductAsync(Guid productId);

    /// <summary>
    /// Gets a tier by product and tier name
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="tierName">Tier name</param>
    /// <returns>Tier or null if not found</returns>
    Task<ProductTier?> GetTierByNameAsync(Guid productId, string tierName);

    /// <summary>
    /// Deletes a tier
    /// </summary>
    /// <param name="tierId">Tier ID</param>
    /// <param name="deletedBy">User deleting the tier</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteTierAsync(Guid tierId, string deletedBy);

    /// <summary>
    /// Checks if a tier exists
    /// </summary>
    /// <param name="tierId">Tier ID</param>
    /// <returns>True if exists</returns>
    Task<bool> TierExistsAsync(Guid tierId);

    /// <summary>
    /// Checks if a tier name exists for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="tierName">Tier name</param>
    /// <param name="excludeTierId">Tier ID to exclude from check (for updates)</param>
    /// <returns>True if name exists</returns>
    Task<bool> TierNameExistsAsync(Guid productId, string tierName, Guid? excludeTierId = null);

    /// <summary>
    /// Validates tier data
    /// </summary>
    /// <param name="tier">Tier to validate</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> ValidateTierAsync(ProductTier tier);
}
