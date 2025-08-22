using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Products;
using TechWayFit.Licensing.Management.Core.Contracts;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories.Product;

/// <summary>
/// Entity Framework implementation of ProductKeys repository
/// </summary>
public class ProductKeysRepository : BaseRepository<ProductKeys, ProductKeysEntity>, IProductKeysRepository
{
    public ProductKeysRepository(EfCoreLicensingDbContext context, IUserContext userContext) : base(context, userContext)
    {
    }

    /// <summary>
    /// Gets the active key pair for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Active ProductKeys entity or null if not found</returns>
    public async Task<ProductKeys?> GetActiveKeysByProductIdAsync(Guid productId)
    {
        var entity = await _dbSet.Where(pk => pk.ProductId == productId && pk.IsActive)
            .OrderByDescending(pk => pk.CreatedOn)
            .FirstOrDefaultAsync();
        
        return entity?.Map();
    }

    /// <summary>
    /// Gets all key pairs for a specific product (including archived ones)
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>List of ProductKeys entities</returns>
    public async Task<IEnumerable<ProductKeys>> GetKeysByProductIdAsync(Guid productId)
    {
        var entities = await _dbSet.Where(pk => pk.ProductId == productId)
            .OrderByDescending(pk => pk.CreatedOn)
            .ToListAsync();
        
        return entities.Select(e => e.Map());
    }

    /// <summary>
    /// Archives existing active keys and sets new keys as active
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="newKeys">New ProductKeys to set as active</param>
    public async Task RotateKeysAsync(Guid productId, ProductKeys newKeys)
    {
        // Deactivate all existing keys for this product
        var existingKeys = await _dbSet.Where(pk => pk.ProductId == productId && pk.IsActive)
            .ToListAsync();

        foreach (var key in existingKeys)
        {
            key.IsActive = false;
            key.DeletedOn = DateTime.UtcNow;
        }

        // Add new active keys
        var newEntity = new ProductKeysEntity().Map(newKeys);
        newEntity.IsActive = true;
        newEntity.CreatedOn = DateTime.UtcNow;
        await _dbSet.AddAsync(newEntity);
    }

    /// <summary>
    /// Checks if a product has any active keys
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>True if active keys exist</returns>
    public async Task<bool> HasActiveKeysAsync(Guid productId)
    {
        return await _dbSet.AnyAsync(pk => pk.ProductId == productId && pk.IsActive);
    }

    /// <summary>
    /// Deactivates all keys for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    public async Task DeactivateAllKeysAsync(Guid productId)
    {
        var activeKeys = await _dbSet.Where(pk => pk.ProductId == productId && pk.IsActive)
            .ToListAsync();

        foreach (var key in activeKeys)
        {
            key.IsActive = false;
            key.DeletedOn = DateTime.UtcNow;
        }
    }
}
