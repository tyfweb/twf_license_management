using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common; 

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;

/// <summary>
/// Repository interface for Product entities
/// </summary>
public interface IProductRepository : IDataRepository<EnterpriseProduct>
{
    /// <summary>
    /// Get product by its unique identifier
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    Task<EnterpriseProduct?> GetWithDetailsAsync(Guid productId);

    /// <summary>
    /// Get all active products
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<EnterpriseProduct>> GetActiveProductsAsync();

    /// <summary>
    /// Check if a product name is unique
    /// </summary>
    /// <param name="name"></param>
    /// <param name="excludeId"></param>
    /// <returns></returns>
    Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null);
}
