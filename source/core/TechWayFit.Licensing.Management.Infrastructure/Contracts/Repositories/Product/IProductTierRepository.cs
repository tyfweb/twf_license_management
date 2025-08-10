using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common; 

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;

/// <summary>
/// Repository interface for ProductTier entities
/// </summary>
public interface IProductTierRepository : IDataRepository<ProductTier>
{
    Task<IEnumerable<ProductTier>> GetByProductIdAsync(Guid productId);
    Task<ProductTier?> GetWithFeaturesAsync(Guid tierId);

}
