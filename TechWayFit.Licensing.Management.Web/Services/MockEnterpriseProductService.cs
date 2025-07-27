using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Management.Web.Services;

/// <summary>
/// Temporary mock implementation of IEnterpriseProductService for testing service layer integration
/// This will be replaced by the full service implementation once the repository infrastructure is set up
/// </summary>
public class MockEnterpriseProductService : IEnterpriseProductService
{
    private static readonly List<EnterpriseProduct> _products = new()
    {
        new EnterpriseProduct
        {
            ProductId = "techway-api-gateway",
            Name = "TechWay API Gateway",
            Description = "Enterprise API Gateway with advanced routing and security features",
            Version = "1.0.0",
            Status = ProductStatus.Active,
            ReleaseDate = DateTime.UtcNow.AddDays(-30)
        },
        new EnterpriseProduct
        {
            ProductId = "techway-web-app",
            Name = "TechWay Web Application",
            Description = "Enterprise web application platform",
            Version = "2.1.0",
            Status = ProductStatus.Active,
            ReleaseDate = DateTime.UtcNow.AddDays(-60)
        },
        new EnterpriseProduct
        {
            ProductId = "techway-alerts",
            Name = "TechWay Enterprise Alerts",
            Description = "Advanced alerting and notification system",
            Version = "1.5.0",
            Status = ProductStatus.Deprecated,
            ReleaseDate = DateTime.UtcNow.AddDays(-120)
        }
    };

    public async Task<EnterpriseProduct> CreateProductAsync(EnterpriseProduct product, string createdBy)
    {
        await Task.Delay(10); // Simulate async operation
        
        // Generate new product ID if not provided
        if (string.IsNullOrEmpty(product.ProductId))
        {
            product.ProductId = $"product-{Guid.NewGuid():N}";
        }

        // Set default values
        product.ReleaseDate = DateTime.UtcNow;
        
        _products.Add(product);
        return product;
    }

    public async Task<bool> DeleteProductAsync(string productId, string deletedBy)
    {
        await Task.Delay(10); // Simulate async operation
        
        var product = _products.FirstOrDefault(p => p.ProductId == productId);
        if (product != null)
        {
            _products.Remove(product);
            return true;
        }
        return false;
    }

    public async Task<EnterpriseProduct?> GetProductByIdAsync(string productId)
    {
        await Task.Delay(10); // Simulate async operation
        return _products.FirstOrDefault(p => p.ProductId == productId);
    }

    public async Task<EnterpriseProduct?> GetProductByNameAsync(string productName)
    {
        await Task.Delay(10); // Simulate async operation
        return _products.FirstOrDefault(p => p.Name.Equals(productName, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IEnumerable<EnterpriseProduct>> GetProductsAsync()
    {
        await Task.Delay(10); // Simulate async operation
        return _products.ToList();
    }

    public async Task<IEnumerable<EnterpriseProduct>> GetProductsAsync(ProductStatus? status = null, string? searchTerm = null, int pageNumber = 1, int pageSize = 50)
    {
        await Task.Delay(10); // Simulate async operation
        
        var query = _products.AsQueryable();
        
        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }
        
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                    p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }
        
        var skip = (pageNumber - 1) * pageSize;
        return query.Skip(skip).Take(pageSize).ToList();
    }

    public async Task<int> GetProductCountAsync(ProductStatus? status = null, string? searchTerm = null)
    {
        await Task.Delay(10); // Simulate async operation
        
        var query = _products.AsQueryable();
        
        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }
        
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                    p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }
        
        return query.Count();
    }

    public async Task<bool> UpdateProductStatusAsync(string productId, ProductStatus status, string updatedBy)
    {
        await Task.Delay(10); // Simulate async operation
        
        var product = _products.FirstOrDefault(p => p.ProductId == productId);
        if (product != null)
        {
            product.Status = status;
            return true;
        }
        return false;
    }

    public async Task<bool> DecommissionProductAsync(string productId, DateTime decommissionDate, string decommissionedBy)
    {
        await Task.Delay(10); // Simulate async operation
        
        var product = _products.FirstOrDefault(p => p.ProductId == productId);
        if (product != null)
        {
            product.Status = ProductStatus.Decommissioned;
            return true;
        }
        return false;
    }

    public async Task<bool> ProductExistsAsync(string productId)
    {
        await Task.Delay(10); // Simulate async operation
        return _products.Any(p => p.ProductId == productId);
    }

    public async Task<IEnumerable<EnterpriseProduct>> GetActiveProductsAsync()
    {
        await Task.Delay(10); // Simulate async operation
        return _products.Where(p => p.Status == ProductStatus.Active).ToList();
    }

    public async Task<IEnumerable<EnterpriseProduct>> GetDeprecatedProductsAsync()
    {
        await Task.Delay(10); // Simulate async operation
        return _products.Where(p => p.Status == ProductStatus.Deprecated).ToList();
    }

    public async Task<IEnumerable<EnterpriseProduct>> GetProductsNearingDecommissionAsync(int daysFromNow = 30)
    {
        await Task.Delay(10); // Simulate async operation
        
        // Since we don't have actual decommission dates, return empty list
        // In a real implementation, this would check for products with decommission dates within the specified range
        return new List<EnterpriseProduct>();
    }

    public async Task<EnterpriseProduct> UpdateProductAsync(EnterpriseProduct product, string updatedBy)
    {
        await Task.Delay(10); // Simulate async operation
        
        var existingProduct = _products.FirstOrDefault(p => p.ProductId == product.ProductId);
        if (existingProduct != null)
        {
            // Update properties
            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Version = product.Version;
            existingProduct.Status = product.Status;
            existingProduct.ReleaseDate = product.ReleaseDate;
            
            return existingProduct;
        }
        
        throw new InvalidOperationException($"Product with ID {product.ProductId} not found");
    }

    public async Task<ValidationResult> ValidateProductAsync(EnterpriseProduct product)
    {
        await Task.Delay(10); // Simulate async operation
        
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(product.Name))
            errors.Add("Product name is required");
            
        if (string.IsNullOrWhiteSpace(product.Description))
            errors.Add("Product description is required");
            
        if (string.IsNullOrWhiteSpace(product.Version))
            errors.Add("Product version is required");
        
        // Check for duplicate names (excluding current product if updating)
        var duplicateName = _products.Any(p => p.ProductId != product.ProductId && 
                                              p.Name.Equals(product.Name, StringComparison.OrdinalIgnoreCase));
        if (duplicateName)
            errors.Add("A product with this name already exists");
        
        return new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors
        };
    }
}
