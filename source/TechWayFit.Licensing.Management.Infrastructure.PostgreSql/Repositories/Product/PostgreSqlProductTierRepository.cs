using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.Product;

/// <summary>
/// Product tier repository implementation
/// </summary>
public class PostgreSqlProductTierRepository : PostgreSqlBaseRepository<ProductTierEntity>, IProductTierRepository
{
    public PostgreSqlProductTierRepository(PostgreSqlPostgreSqlLicensingDbContext context) : base(context)
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
