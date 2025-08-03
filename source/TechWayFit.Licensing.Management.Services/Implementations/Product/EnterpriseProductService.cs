using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;
using TechWayFit.Licensing.Management.Infrastructure.Models.Search;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Management.Services.Implementations.Product;

/// <summary>
/// Implementation of the Enterprise Product service
/// </summary>
public class EnterpriseProductService : IEnterpriseProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EnterpriseProductService> _logger;

    public EnterpriseProductService(
        IUnitOfWork unitOfWork,
        ILogger<EnterpriseProductService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new enterprise product
    /// </summary>
    public async Task<EnterpriseProduct> CreateProductAsync(EnterpriseProduct product, string createdBy)
    {
        _logger.LogInformation("Creating enterprise product: {ProductName}", product.Name);

        // Input validation
        if (product == null)
            throw new ArgumentNullException(nameof(product));
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("CreatedBy cannot be null or empty", nameof(createdBy));

        // Business validation
        var validationResult = await ValidateProductAsync(product);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors);
            throw new InvalidOperationException($"Product validation failed: {errors}");
        }

        try
        {
            // Check if product with same name already exists
            var existingProduct = await GetProductByNameAsync(product.Name);
            if (existingProduct != null)
            {
                throw new InvalidOperationException($"Product with name '{product.Name}' already exists");
            }

            // Map to entity and set audit fields
            var productEntity = ProductEntity.FromModel(product);
            productEntity.CreatedBy = createdBy;
            productEntity.CreatedOn = DateTime.UtcNow;
            productEntity.UpdatedBy = createdBy;
            productEntity.UpdatedOn = DateTime.UtcNow;

            // Save to repository
            var createdEntity = await _unitOfWork.Products.AddAsync(productEntity);
            await _unitOfWork.SaveChangesAsync();
            
            // Map back to model
            var result = createdEntity.ToModel();
            
            _logger.LogInformation("Successfully created enterprise product with ID: {ProductId}", result.ProductId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating enterprise product: {ProductName}", product.Name);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing enterprise product
    /// </summary>
    public async Task<EnterpriseProduct> UpdateProductAsync(EnterpriseProduct product, string updatedBy)
    {
        _logger.LogInformation("Updating enterprise product: {ProductId}", product.ProductId);

        // Input validation
        if (product == null)
            throw new ArgumentNullException(nameof(product));
        if (Guid.Empty.Equals(product.ProductId))
            throw new ArgumentException("ProductId cannot be null or empty", nameof(product));
        if (string.IsNullOrWhiteSpace(updatedBy))
            throw new ArgumentException("UpdatedBy cannot be null or empty", nameof(updatedBy));

        try
        {
            // Get existing product
            var existingEntity = await _unitOfWork.Products.GetByIdAsync(product.ProductId);
            if (existingEntity == null)
            {
                throw new InvalidOperationException($"Product with ID {product.ProductId} not found");
            }

            // Business validation
            var validationResult = await ValidateProductAsync(product);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors);
                throw new InvalidOperationException($"Product validation failed: {errors}");
            }

            // Update properties
            var updatedData = ProductEntity.FromModel(product);
            
            // Update properties manually to preserve audit fields
            existingEntity.Name = updatedData.Name;
            existingEntity.Description = updatedData.Description;
            existingEntity.ReleaseDate = updatedData.ReleaseDate;
            existingEntity.SupportEmail = updatedData.SupportEmail;
            existingEntity.SupportPhone = updatedData.SupportPhone;
            existingEntity.DecommissionDate = updatedData.DecommissionDate;
            existingEntity.Status = updatedData.Status;
            
            existingEntity.UpdatedBy = updatedBy;
            existingEntity.UpdatedOn = DateTime.UtcNow;

            // Update in repository
            var updatedEntity = await _unitOfWork.Products.UpdateAsync(existingEntity);
            await _unitOfWork.SaveChangesAsync();
            
            // Map back to model
            var result = updatedEntity.ToModel();
            
            _logger.LogInformation("Successfully updated enterprise product: {ProductId}", product.ProductId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating enterprise product: {ProductId}", product.ProductId);
            throw;
        }
    }

    /// <summary>
    /// Gets a product by ID
    /// </summary>
    public async Task<EnterpriseProduct?> GetProductByIdAsync(Guid productId)
    {
        if (Guid.Empty.Equals(productId))
            throw new ArgumentException("ProductId cannot be null or empty", nameof(productId));

        try
        {
            var entity = await _unitOfWork.Products.GetByIdAsync(productId);
            return entity?.ToModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting enterprise product by ID: {ProductId}", productId);
            throw;
        }
    }

    /// <summary>
    /// Gets a product by name
    /// </summary>
    public async Task<EnterpriseProduct?> GetProductByNameAsync(string productName)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("ProductName cannot be null or empty", nameof(productName));

        try
        {
            // TODO: Implement GetByNameAsync in repository
            _logger.LogWarning("GetProductByNameAsync using search - GetByNameAsync repository method missing");
            
            var searchRequest = new SearchRequest<ProductEntity>
            {
                Filters = new List<Expression<Func<ProductEntity, bool>>>
                {
                    p => p.Name.Equals(productName, StringComparison.OrdinalIgnoreCase)
                }
            };
            
            var searchResult = await _unitOfWork.Products.SearchAsync(searchRequest);
            var entity = searchResult.Results.FirstOrDefault();
            
            return entity?.ToModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting enterprise product by name: {ProductName}", productName);
            throw;
        }
    }

    /// <summary>
    /// Gets all products with optional filtering
    /// </summary>
    public async Task<IEnumerable<EnterpriseProduct>> GetProductsAsync(ProductStatus? status = null, string? searchTerm = null, int pageNumber = 1, int pageSize = 50)
    {
        try
        {
            var searchRequest = new SearchRequest<ProductEntity>
            {
                Filters = new List<Expression<Func<ProductEntity, bool>>>()
            };

            // Apply status filter
            if (status.HasValue)
            {
                var statusString = status.Value.ToString();
                searchRequest.Filters.Add(p => p.Status == statusString);
            }

            // Apply search term filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                searchRequest.Filters.Add(p => 
                    p.Name.ToLower().Contains(term) ||
                    p.Description.ToLower().Contains(term));
            }

            var searchResult = await _unitOfWork.Products.SearchAsync(searchRequest);
            return searchResult.Results.Select(e => e.ToModel());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting enterprise products");
            throw;
        }
    }

    #region TODO: Missing Interface Methods - Require Implementation

    public async Task<bool> ActivateProductAsync(Guid productId, string activatedBy)
    {
        // TODO: Implement
        _logger.LogWarning("ActivateProductAsync not implemented");
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> DeactivateProductAsync(Guid productId, string deactivatedBy, string? reason = null)
    {
        // TODO: Implement
        _logger.LogWarning("DeactivateProductAsync not implemented");
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> DeleteProductAsync(Guid productId, string deletedBy)
    {
        // TODO: Implement
        _logger.LogWarning("DeleteProductAsync not implemented");
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> ProductExistsAsync(Guid productId)
    {
        // TODO: Implement
        _logger.LogWarning("ProductExistsAsync not implemented");
        await Task.CompletedTask;
        return false;
    }

    public async Task<ValidationResult> ValidateProductAsync(EnterpriseProduct product)
    {
        var errors = new List<string>();

        // Basic validation
        if (string.IsNullOrWhiteSpace(product.Name))
            errors.Add("Product name is required");

        if (string.IsNullOrWhiteSpace(product.Description))
            errors.Add("Product description is required");

        // TODO: Add more business validation rules

        await Task.CompletedTask;
        return errors.Any() ? ValidationResult.Failure(errors.ToArray()) : ValidationResult.Success();
    }

    public async Task<IEnumerable<EnterpriseProduct>> GetActiveProductsAsync()
    {
        // TODO: Implement
        _logger.LogWarning("GetActiveProductsAsync not implemented");
        await Task.CompletedTask;
        return Enumerable.Empty<EnterpriseProduct>();
    }

    public async Task<int> GetProductCountAsync(ProductStatus? status = null, string? searchTerm = null)
    {
        // TODO: Implement
        _logger.LogWarning("GetProductCountAsync not implemented");
        await Task.CompletedTask;
        return 0;
    }

    public async Task<IEnumerable<string>> GetProductCodesAsync()
    {
        // TODO: Implement
        _logger.LogWarning("GetProductCodesAsync not implemented");
        await Task.CompletedTask;
        return Enumerable.Empty<string>();
    }

    public async Task<bool> IsProductCodeUniqueAsync(string productCode, Guid? excludeProductId = null)
    {
        // TODO: Implement
        _logger.LogWarning("IsProductCodeUniqueAsync not implemented");
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> IsProductNameUniqueAsync(string productName, Guid? excludeProductId = null)
    {
        // TODO: Implement
        _logger.LogWarning("IsProductNameUniqueAsync not implemented");
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> UpdateProductStatusAsync(Guid productId, ProductStatus status, string updatedBy)
    {
        // TODO: Implement
        _logger.LogWarning("UpdateProductStatusAsync not implemented");
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> DecommissionProductAsync(Guid productId, DateTime decommissionDate, string decommissionedBy)
    {
        // TODO: Implement
        _logger.LogWarning("DecommissionProductAsync not implemented");
        await Task.CompletedTask;
        return false;
    }

    public async Task<IEnumerable<EnterpriseProduct>> GetDeprecatedProductsAsync()
    {
        // TODO: Implement
        _logger.LogWarning("GetDeprecatedProductsAsync not implemented");
        await Task.CompletedTask;
        return Enumerable.Empty<EnterpriseProduct>();
    }

    public async Task<IEnumerable<EnterpriseProduct>> GetProductsNearingDecommissionAsync(int daysAhead = 30)
    {
        // TODO: Implement
        _logger.LogWarning("GetProductsNearingDecommissionAsync not implemented");
        await Task.CompletedTask;
        return Enumerable.Empty<EnterpriseProduct>();
    }

    #endregion
}
