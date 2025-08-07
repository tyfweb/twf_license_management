using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Products;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Examples;

/// <summary>
/// Example showing how to work with the multi-tenant DbContext
/// This example demonstrates proper usage patterns for tenant-aware operations
/// </summary>
public class TenantAwareRepositoryExample
{
    private readonly EfCoreLicensingDbContext _context;

    public TenantAwareRepositoryExample(EfCoreLicensingDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Example: Normal query - automatically filtered by tenant
    /// </summary>
    public async Task<List<ProductEntity>> GetActiveProductsAsync()
    {
        // This query will automatically include: WHERE TenantId = @currentTenantId
        return await _context.Products
            .Where(p => p.IsActive)
            .ToListAsync();
    }

    /// <summary>
    /// Example: Complex query with relationships - all automatically filtered
    /// </summary>
    public async Task<List<ProductEntity>> GetProductsWithLicensesAsync()
    {
        // Both Products and ProductLicenses will be filtered by tenant
        return await _context.Products
            .Include(p => p.Licenses)
            .Where(p => p.IsActive && p.Licenses.Any())
            .ToListAsync();
    }

    /// <summary>
    /// Example: Administrative operation - bypassing tenant filter
    /// Use ONLY for system-level operations
    /// </summary>
    public async Task<int> GetTotalProductCountAcrossAllTenantsAsync()
    {
        // WARNING: This bypasses tenant filtering - use with extreme caution
        return await _context.WithoutTenantFilterAsync(async () =>
        {
            return await _context.Products.CountAsync();
        });
    }

    /// <summary>
    /// Example: Creating new entity - tenant ID automatically set
    /// </summary>
    public async Task<ProductEntity> CreateProductAsync(string name, string description)
    {
        var product = new ProductEntity
        {
            Name = name,
            Description = description,
            IsActive = true
            // TenantId will be automatically set during SaveChanges()
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        
        return product;
    }

    /// <summary>
    /// Example: Tenant-specific search with pagination
    /// </summary>
    public async Task<(List<ProductEntity> Products, int TotalCount)> SearchProductsAsync(
        string searchTerm, 
        int page = 1, 
        int pageSize = 20)
    {
        var query = _context.Products.AsQueryable();
        
        // All queries are automatically filtered by tenant
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(p => p.Name.Contains(searchTerm) || 
                                   p.Description.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync();
        var products = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (products, totalCount);
    }

    /// <summary>
    /// Example: Bulk operation within tenant scope
    /// </summary>
    public async Task<int> DeactivateExpiredLicensesAsync()
    {
        // This will only affect licenses within the current tenant
        return await _context.ProductLicenses
            .Where(l => l.ValidTo < DateTime.UtcNow && l.Status == "Active")
            .ExecuteUpdateAsync(l => l.SetProperty(x => x.Status, "Expired"));
    }

    /// <summary>
    /// Example: Cross-tenant admin report (requires special privileges)
    /// </summary>
    public async Task<Dictionary<Guid, int>> GetLicenseCountByTenantAsync()
    {
        // Only for system administrators
        return await _context.WithoutTenantFilterAsync(async () =>
        {
            return await _context.ProductLicenses
                .GroupBy(l => l.TenantId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        });
    }
}

/// <summary>
/// Example of proper service layer implementation with tenant awareness
/// </summary>
public class TenantAwareProductService
{
    private readonly EfCoreLicensingDbContext _context;
    private readonly ILogger<TenantAwareProductService> _logger;

    public TenantAwareProductService(
        EfCoreLicensingDbContext context,
        ILogger<TenantAwareProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Example: Service method that ensures tenant isolation
    /// </summary>
    public async Task<ProductEntity?> GetProductByIdAsync(Guid productId)
    {
        try
        {
            // Automatically filtered by tenant - returns null if product belongs to different tenant
            var product = await _context.Products
                .Include(p => p.Versions)
                .Include(p => p.Tiers)
                .FirstOrDefaultAsync(p => p.Id == productId);

            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", productId);
            return null;
        }
    }

  
}

