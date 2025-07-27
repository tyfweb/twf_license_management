using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Infrastructure.Contracts.Repositories.Product;
using TechWayFit.Licensing.Infrastructure.Data.Context;
using TechWayFit.Licensing.Infrastructure.Models.Entities.Products;

namespace TechWayFit.Licensing.Infrastructure.Data.Repositories.Product;

/// <summary>
/// Product version repository implementation
/// </summary>
public class ProductVersionRepository : BaseRepository<ProductVersionEntity>, IProductVersionRepository
{
    public ProductVersionRepository(LicensingDbContext context) : base(context)
    {
    }
 
}
