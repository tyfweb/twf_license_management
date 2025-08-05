using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.Product;

/// <summary>
/// Product version repository implementation
/// </summary>
public class PostgreSqlProductVersionRepository : PostgreSqlBaseRepository<ProductVersionEntity>, IProductVersionRepository
{
    public PostgreSqlProductVersionRepository(PostgreSqlPostgreSqlLicensingDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ProductVersionEntity>> GetByProductIdAsync(Guid productId)
    {
       return await _dbSet
            .Where(v => v.ProductId == productId)
            .ToListAsync();
    }
}
