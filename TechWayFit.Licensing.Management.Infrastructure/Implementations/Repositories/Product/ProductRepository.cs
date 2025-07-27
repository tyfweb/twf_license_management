using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Infrastructure.Contracts.Repositories.Product;
using TechWayFit.Licensing.Infrastructure.Data.Context;
using TechWayFit.Licensing.Infrastructure.Models.Entities.Products;
using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Infrastructure.Data.Repositories.Product;

/// <summary>
/// Product repository implementation
/// </summary>
public class ProductRepository : BaseRepository<ProductEntity>, IProductRepository
{
    public ProductRepository(LicensingDbContext context) : base(context)
    {
    }
    public async Task<ProductEntity?> GetWithDetailsAsync(string productId)
    {
        return await _dbSet.Include(p => p.Versions)
                         .Include(p => p.Tiers)
                         .FirstOrDefaultAsync(p => p.ProductId == productId);
    }
    public async Task<IEnumerable<ProductEntity>> GetActiveProductsAsync()
    {
        return await _dbSet.Where(p => p.Status == ProductStatus.Active.ToString())
                         .OrderBy(p => p.Name)
                         .ToListAsync();
    }
    public async Task<bool> IsNameUniqueAsync(string name, string? excludeId = null)
    {
        var query = _dbSet.Where(p => p.Name == name);
        if (excludeId != null)
            query = query.Where(p => p.ProductId != excludeId);
        return !await query.AnyAsync();
    }

    protected override IQueryable<ProductEntity> SearchQuery(IQueryable<ProductEntity> query, string searchQuery)
    {
        return base.SearchQuery(query, searchQuery)
                   .Where(p => p.Name.Contains(searchQuery) ||
                               p.Description.Contains(searchQuery));
    }
}
