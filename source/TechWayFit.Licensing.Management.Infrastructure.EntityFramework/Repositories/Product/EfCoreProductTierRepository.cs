using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Products;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Contracts;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories.Product;

/// <summary>
/// Product tier repository implementation
/// </summary>
public class EfCoreProductTierRepository :  BaseRepository<ProductTier,ProductTierEntity>, IProductTierRepository
{
    public EfCoreProductTierRepository(EfCoreLicensingDbContext context,IUserContext userContext) : base(context,userContext)
    {
    }

    public async Task<IEnumerable<ProductTier>> GetByProductIdAsync(Guid productId)
    {
        var result = await _dbSet.Where(t => t.ProductId == productId && t.IsActive)
                     .OrderBy(t => t.DisplayOrder)
                     .ToListAsync();
        return result.Select(t => t.Map());
    }

    public Task<ProductTier?> GetWithFeaturesAsync(Guid tierId)
    {
        var result = _dbSet.Include(t => t.Features)
                     .FirstOrDefaultAsync(t => t.Id == tierId && t.IsActive);
        return result.ContinueWith(t => t.Result?.Map());
    }
}
