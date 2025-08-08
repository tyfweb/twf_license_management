using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.Products;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Contracts;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.Product;

/// <summary>
/// PostgreSQL implementation of Product repository
/// </summary>
public class PostgreSqlProductRepository : BaseRepository<EnterpriseProduct,ProductEntity>, IProductRepository
{
    public PostgreSqlProductRepository(PostgreSqlPostgreSqlLicensingDbContext context,IUserContext userContext) : base(context,userContext)
    {
    }
    public async Task<EnterpriseProduct?> GetWithDetailsAsync(Guid productId)
    {
        var result = await _dbSet.Include(p => p.Versions)
                         .Include(p => p.Tiers)
                         .FirstOrDefaultAsync(p => p.Id == productId);
        return result?.Map();
    }
    public async Task<IEnumerable<EnterpriseProduct>> GetActiveProductsAsync()
    {
        var result = await _dbSet.Where(p => p.Status == ProductStatus.Active.ToString())
                         .OrderBy(p => p.Name)
                         .ToListAsync();
        return result.Select(p => p.Map());
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
