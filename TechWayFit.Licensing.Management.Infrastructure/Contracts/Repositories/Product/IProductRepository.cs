using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;

/// <summary>
/// Repository interface for Product entities
/// </summary>
public interface IProductRepository : IBaseRepository<ProductEntity>
{
    /// <summary>
    /// Get product by its unique identifier
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    Task<ProductEntity?> GetWithDetailsAsync(string productId);

    /// <summary>
    /// Get all active products
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<ProductEntity>> GetActiveProductsAsync();

    /// <summary>
    /// Check if a product name is unique
    /// </summary>
    /// <param name="name"></param>
    /// <param name="excludeId"></param>
    /// <returns></returns>
    Task<bool> IsNameUniqueAsync(string name, string? excludeId = null);
}
