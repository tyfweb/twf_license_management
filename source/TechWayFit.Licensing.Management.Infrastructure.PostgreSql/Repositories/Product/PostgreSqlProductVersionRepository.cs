using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.Products;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Contracts;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.Product;

/// <summary>
/// Product version repository implementation
/// </summary>
public class PostgreSqlProductVersionRepository : BaseRepository<ProductVersion,ProductVersionEntity>, IProductVersionRepository
{
    public PostgreSqlProductVersionRepository(PostgreSqlPostgreSqlLicensingDbContext context,IUserContext userContext) : base(context,userContext)
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
