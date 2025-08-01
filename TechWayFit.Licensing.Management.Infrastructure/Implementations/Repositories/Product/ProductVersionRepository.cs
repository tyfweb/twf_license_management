using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;
using TechWayFit.Licensing.Management.Infrastructure.Data.Context;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;

namespace TechWayFit.Licensing.Management.Infrastructure.Data.Repositories.Product;

/// <summary>
/// Product version repository implementation
/// </summary>
public class ProductVersionRepository : BaseRepository<ProductVersionEntity>, IProductVersionRepository
{
    public ProductVersionRepository(LicensingDbContext context) : base(context)
    {
    }
 
}
