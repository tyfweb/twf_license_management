using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;
using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.Product;

/// <summary>
/// PostgreSQL implementation of Product repository
/// </summary>
public class PostgreSqlProductRepository : PostgreSqlBaseRepository<ProductEntity>, IProductRepository
{
    public PostgreSqlProductRepository(PostgreSqlPostgreSqlLicensingDbContext context) : base(context)
    {
    }
    public async Task<ProductEntity?> GetWithDetailsAsync(Guid productId)
    {
        return await _dbSet.Include(p => p.Versions)
                         .Include(p => p.Tiers)
                         .FirstOrDefaultAsync(p => p.Id == productId);
    }
    public async Task<IEnumerable<ProductEntity>> GetActiveProductsAsync()
    {
        return await _dbSet.Where(p => p.Status == ProductStatus.Active.ToString())
                         .OrderBy(p => p.Name)
                         .ToListAsync();
    }
    public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null)
    {
        var query = _dbSet.Where(p => p.Name == name);
        if (excludeId != null)
            query = query.Where(p => p.Id != excludeId);
        return !await query.AnyAsync();
    }

    protected override IQueryable<ProductEntity> SearchQuery(IQueryable<ProductEntity> query, string searchQuery)
    {
        return base.SearchQuery(query, searchQuery)
                   .Where(p => p.Name.Contains(searchQuery) ||
                               p.Description.Contains(searchQuery));
    }
}
