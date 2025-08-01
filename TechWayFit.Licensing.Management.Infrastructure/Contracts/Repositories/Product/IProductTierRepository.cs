using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;

/// <summary>
/// Repository interface for ProductTier entities
/// </summary>
public interface IProductTierRepository : IBaseRepository<ProductTierEntity>
{
    Task<IEnumerable<ProductTierEntity>> GetByProductIdAsync(string productId);
    Task<ProductTierEntity?> GetWithFeaturesAsync(string tierId);
    
}
