using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;
using TechWayFit.Licensing.Management.Infrastructure.Data.Context;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;

namespace TechWayFit.Licensing.Management.Infrastructure.Data.Repositories.Product;

/// <summary>
/// Product tier repository implementation
/// </summary>
public class ProductTierRepository : BaseRepository<ProductTierEntity>, IProductTierRepository
{
    public ProductTierRepository(LicensingDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ProductTierEntity>> GetByProductIdAsync(string productId)
    {
        return await _dbSet.Where(t => t.ProductId == productId && t.IsActive)
                     .OrderBy(t => t.DisplayOrder)
                     .ToListAsync();
    }

    public Task<ProductTierEntity?> GetWithFeaturesAsync(string tierId)
    {
        return _dbSet.Include(t => t.Features)
                     .FirstOrDefaultAsync(t => t.TierId == tierId && t.IsActive);
    }
}
