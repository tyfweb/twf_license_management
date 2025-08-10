using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Products;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Contracts;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories.Product;

/// <summary>
/// Product version repository implementation
/// </summary>
public class EfCoreProductVersionRepository : BaseRepository<ProductVersion,ProductVersionEntity>, IProductVersionRepository
{
    public EfCoreProductVersionRepository(EfCoreLicensingDbContext context,IUserContext userContext) : base(context,userContext)
    {
    }

    public async Task<IEnumerable<ProductVersion>> GetByProductIdAsync(Guid productId)
    {
        var result = await _dbSet
             .Where(v => v.ProductId == productId)
             .ToListAsync();
        return result.Select(v => v.Map());
    }
}
