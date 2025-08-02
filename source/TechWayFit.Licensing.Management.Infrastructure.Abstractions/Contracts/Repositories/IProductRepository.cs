using TechWayFit.Licensing.Management.Infrastructure.Abstractions.Contracts;

namespace TechWayFit.Licensing.Management.Infrastructure.Abstractions.Contracts.Repositories;

/// <summary>
/// Repository interface for Product entities
/// </summary>
public interface IProductRepository : IBaseRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Get product by its unique identifier with related data
    /// </summary>
    Task<TEntity?> GetWithDetailsAsync(string productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active products
    /// </summary>
    Task<IEnumerable<TEntity>> GetActiveProductsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a product name is unique
    /// </summary>
    Task<bool> IsNameUniqueAsync(string name, string? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get products by status
    /// </summary>
    Task<IEnumerable<TEntity>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search products by name or description
    /// </summary>
    Task<IEnumerable<TEntity>> SearchAsync(string searchQuery, CancellationToken cancellationToken = default);
}
