using TechWayFit.Licensing.Infrastructure.Contracts.Repositories.Common;
using TechWayFit.Licensing.Infrastructure.Models.Entities.Products;

namespace TechWayFit.Licensing.Infrastructure.Contracts.Repositories.Product;

/// <summary>
/// Repository interface for ProductTier entities
/// </summary>
public interface IProductTierRepository : IBaseRepository<ProductTierEntity>
{
    Task<IEnumerable<ProductTierEntity>> GetByProductIdAsync(string productId);
    Task<ProductTierEntity?> GetWithFeaturesAsync(string tierId);
    
}
