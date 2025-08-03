using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Management.Core.Contracts.Services;

/// <summary>
/// Service interface for managing enterprise products
/// </summary>
public interface IEnterpriseProductService
{
    /// <summary>
    /// Creates a new enterprise product
    /// </summary>
    /// <param name="product">Product to create</param>
    /// <param name="createdBy">User creating the product</param>
    /// <returns>Created product</returns>
    Task<EnterpriseProduct> CreateProductAsync(EnterpriseProduct product, string createdBy);

    /// <summary>
    /// Updates an existing enterprise product
    /// </summary>
    /// <param name="product">Product to update</param>
    /// <param name="updatedBy">User updating the product</param>
    /// <returns>Updated product</returns>
    Task<EnterpriseProduct> UpdateProductAsync(EnterpriseProduct product, string updatedBy);

    /// <summary>
    /// Gets a product by ID
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Product or null if not found</returns>
    Task<EnterpriseProduct?> GetProductByIdAsync(Guid productId);

    /// <summary>
    /// Gets a product by name
    /// </summary>
    /// <param name="productName">Product name</param>
    /// <returns>Product or null if not found</returns>
    Task<EnterpriseProduct?> GetProductByNameAsync(string productName);

    /// <summary>
    /// Gets all products with optional filtering
    /// </summary>
    /// <param name="status">Filter by product status</param>
    /// <param name="searchTerm">Search term for name or description</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of products</returns>
    Task<IEnumerable<EnterpriseProduct>> GetProductsAsync(
        ProductStatus? status = null,
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 50);

    /// <summary>
    /// Gets the total count of products with optional filtering
    /// </summary>
    /// <param name="status">Filter by product status</param>
    /// <param name="searchTerm">Search term for name or description</param>
    /// <returns>Total count</returns>
    Task<int> GetProductCountAsync(ProductStatus? status = null, string? searchTerm = null);

    /// <summary>
    /// Updates product status
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="status">New status</param>
    /// <param name="updatedBy">User updating the status</param>
    /// <returns>True if updated successfully</returns>
    Task<bool> UpdateProductStatusAsync(Guid productId, ProductStatus status, string updatedBy);

    /// <summary>
    /// Decommissions a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="decommissionDate">Decommission date</param>
    /// <param name="decommissionedBy">User decommissioning the product</param>
    /// <returns>True if decommissioned successfully</returns>
    Task<bool> DecommissionProductAsync(Guid productId, DateTime decommissionDate, string decommissionedBy);

    /// <summary>
    /// Deletes a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="deletedBy">User deleting the product</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteProductAsync(Guid productId, string deletedBy);

    /// <summary>
    /// Checks if a product exists
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>True if exists</returns>
    Task<bool> ProductExistsAsync(Guid productId);

    /// <summary>
    /// Gets active products only
    /// </summary>
    /// <returns>List of active products</returns>
    Task<IEnumerable<EnterpriseProduct>> GetActiveProductsAsync();

    /// <summary>
    /// Gets deprecated products
    /// </summary>
    /// <returns>List of deprecated products</returns>
    Task<IEnumerable<EnterpriseProduct>> GetDeprecatedProductsAsync();

    /// <summary>
    /// Gets products nearing decommission
    /// </summary>
    /// <param name="daysFromNow">Number of days from now to check</param>
    /// <returns>List of products nearing decommission</returns>
    Task<IEnumerable<EnterpriseProduct>> GetProductsNearingDecommissionAsync(int daysFromNow = 30);

    /// <summary>
    /// Validates product data
    /// </summary>
    /// <param name="product">Product to validate</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> ValidateProductAsync(EnterpriseProduct product);
}
