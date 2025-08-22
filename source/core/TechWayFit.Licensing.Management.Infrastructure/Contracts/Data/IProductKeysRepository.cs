using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;

/// <summary>
/// Repository interface for ProductKeys entity
/// Handles database operations for product cryptographic keys
/// </summary>
public interface IProductKeysRepository : IDataRepository<ProductKeys>
{
    /// <summary>
    /// Gets the active key pair for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Active ProductKeys entity or null if not found</returns>
    Task<ProductKeys?> GetActiveKeysByProductIdAsync(Guid productId);

    /// <summary>
    /// Gets all key pairs for a specific product (including archived ones)
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>List of ProductKeys entities</returns>
    Task<IEnumerable<ProductKeys>> GetKeysByProductIdAsync(Guid productId);

    /// <summary>
    /// Archives existing active keys and sets new keys as active
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="newKeys">New ProductKeys to set as active</param>
    Task RotateKeysAsync(Guid productId, ProductKeys newKeys);

    /// <summary>
    /// Checks if a product has any active keys
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>True if active keys exist</returns>
    Task<bool> HasActiveKeysAsync(Guid productId);

    /// <summary>
    /// Deactivates all keys for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    Task DeactivateAllKeysAsync(Guid productId);
}
