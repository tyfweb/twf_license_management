using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common; 

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;

/// <summary>
/// Repository interface for ProductVersion entities
/// </summary>
public interface IProductVersionRepository : IDataRepository<ProductVersion>
{
    Task<IEnumerable<ProductVersion>> GetByProductIdAsync(Guid productId);
}
