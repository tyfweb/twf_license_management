using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Products;
using TechWayFit.Licensing.Management.Infrastructure.Models.Search;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Models.Audit;
using TechWayFit.Licensing.Management.Core.Helpers;
using System.Text.Json;

namespace TechWayFit.Licensing.Management.Services.Implementations.Product;

/// <summary>
/// Implementation of the Enterprise Product service
/// </summary>
public class EnterpriseProductService : IEnterpriseProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IKeyManagementService _keyManagementService;
    private readonly IAuditService _auditService;
    private readonly ILogger<EnterpriseProductService> _logger;

    public EnterpriseProductService(
        IUnitOfWork unitOfWork,
        IKeyManagementService keyManagementService,
        IAuditService auditService,
        ILogger<EnterpriseProductService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _keyManagementService = keyManagementService ?? throw new ArgumentNullException(nameof(keyManagementService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
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

            // Save to repository
            var createdEntity = await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();
            // Ensure the product has at least few features
            
            var defaultFeature = new ProductFeature
            {
                ProductId = createdEntity.Id,
                Name = "Default Feature",
                Description = "Default feature for new products"
            };
            var createdFeature = await _unitOfWork.ProductFeatures.AddAsync(defaultFeature);
            // Ensure the product has at least one tier
            if (!createdEntity.Tiers.Any())
            {
                var defaultTier = ProductTier.Default;
                defaultTier.ProductId = createdEntity.Id; // Ensure the tier is linked to the product
                createdEntity.Tiers = new List<ProductTier> { defaultTier };
                var defaultTierEntity = await _unitOfWork.ProductTiers.AddAsync(defaultTier);

                await _unitOfWork.ProductFeatureTierMappings.AddAsync(new ProductFeatureTierMapping
                {
                    ProductFeatureId = createdFeature.FeatureId,
                    ProductTierId = defaultTierEntity.TierId,
                    IsEnabled = true,
                    DisplayOrder = 1,
                    Configuration = null
                });
            }
            // Ensure the product has at least one version
            if (!createdEntity.Versions.Any())
            {
                var defaultVersion = new ProductVersion
                {
                    ProductId = createdEntity.Id,
                    Version = SemanticVersion.Default,
                    ChangeLog = "Initial release",
                    ReleaseDate = DateTime.UtcNow, 
                    Name = "Initial Version"
                };
                createdEntity.Versions = [defaultVersion];
                await _unitOfWork.ProductVersions.AddAsync(defaultVersion);
            }
               await _unitOfWork.SaveChangesAsync();

            // Automatically generate cryptographic keys for the new product
            try
            {
                _logger.LogInformation("Generating cryptographic keys for product: {ProductId}", createdEntity.Id);
                await _keyManagementService.GenerateKeyPairForProductAsync(createdEntity.Id);
                _logger.LogInformation("Successfully generated cryptographic keys for product: {ProductId}", createdEntity.Id);
            }
            catch (Exception keyGenEx)
            {
                _logger.LogError(keyGenEx, "Failed to generate cryptographic keys for product: {ProductId}", createdEntity.Id);
                // Note: We don't throw here to avoid rolling back the product creation
                // The keys can be generated later if needed
            }

            // Map back to model
            var result = createdEntity;

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
            var updatedData = product;

            // Update properties manually to preserve audit fields
            existingEntity.Name = updatedData.Name;
            existingEntity.Description = updatedData.Description;
            existingEntity.ReleaseDate = updatedData.ReleaseDate;
            existingEntity.SupportEmail = updatedData.SupportEmail;
            existingEntity.SupportPhone = updatedData.SupportPhone;
            existingEntity.DecommissionDate = updatedData.DecommissionDate;
            existingEntity.Status = updatedData.Status;
 

            // Update in repository
            var updatedEntity = await _unitOfWork.Products.UpdateAsync(existingEntity.Id, existingEntity);
            await _unitOfWork.SaveChangesAsync();

            // Map back to model
            var result = updatedEntity;

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
            return entity;
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

            var searchRequest = new SearchRequest<EnterpriseProduct>
            {
                Filters = new Dictionary<string, object>
                {
                    { "Name", productName.ToUpper() }
                }
            };

            var searchResult = await _unitOfWork.Products.SearchAsync(searchRequest);
            var entity = searchResult.Results.FirstOrDefault();

            return entity;
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
            var searchRequest = new SearchRequest<EnterpriseProduct>
            {
                Query = searchTerm,
                Page = pageNumber,
                PageSize = pageSize,
                Filters = new Dictionary<string, object>()
            };

            // Apply status filter
            if (status.HasValue)
            {
                var statusString = status.Value.ToString();
                searchRequest.Filters.Add("Status", statusString);
            }

            // Apply search term filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                // searchRequest.Filters.Add(p =>
                //     p.Name.ToLower().Contains(term) ||
                //     p.Description.ToLower().Contains(term));
            }

            var searchResult = await _unitOfWork.Products.SearchAsync(searchRequest);
            return searchResult.Results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting enterprise products");
            throw;
        }
    }
    public async Task<bool> ProductExistsAsync(Guid productId, bool includeDeleted = false)
    {
        if (Guid.Empty.Equals(productId))
            throw new ArgumentException("ProductId cannot be null or empty", nameof(productId));
        _logger.LogInformation("Checking if product exists: {ProductId}", productId);

        var exists = await _unitOfWork.Products.ExistsAsync(productId, includeDeleted);
        _logger.LogInformation("Product exists: {ProductId} - {Exists}", productId, exists);
        return exists;
    }
    
    #region TODO: Missing Interface Methods - Require Implementation

    public async Task<bool> ActivateProductAsync(Guid productId, string activatedBy)
    {
        Ensure.NotDefault(productId, nameof(productId));
        Ensure.NotEmpty(activatedBy, nameof(activatedBy));

        _logger.LogInformation("Activating product {ProductId} by {ActivatedBy}", productId, activatedBy);

        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found for activation", productId);
                return false;
            }

            if (product.Status == ProductStatus.Active)
            {
                _logger.LogInformation("Product {ProductId} is already active", productId);
                return true;
            }

            // Update product status
            var previousStatus = product.Status;
            product.Status = ProductStatus.Active;
            product.Audit.UpdatedBy = activatedBy;
            product.Audit.UpdatedOn = DateTime.UtcNow;

            await _unitOfWork.Products.UpdateAsync(productId, product);

            // Create audit entry
            var auditEntry = new AuditEntry
            {
                EntryId = Guid.NewGuid(),
                EntityType = nameof(EnterpriseProduct),
                EntityId = productId.ToString(),
                ActionType = "Activate",
                UserName = activatedBy,
                Timestamp = DateTime.UtcNow,
                Reason = "Product activation",
                Metadata = new Dictionary<string, string>
                {
                    ["ProductId"] = product.ProductId.ToString(),
                    ["ProductName"] = product.Name,
                    ["PreviousStatus"] = previousStatus.ToString(),
                    ["NewStatus"] = ProductStatus.Active.ToString()
                }
            };

            await _auditService.LogAuditEntryAsync(auditEntry);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Product {ProductId} activated successfully", productId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating product {ProductId}", productId);
            return false;
        }
    }

    public async Task<bool> DeactivateProductAsync(Guid productId, string deactivatedBy, string? reason = null)
    {
        Ensure.NotDefault(productId, nameof(productId));
        Ensure.NotEmpty(deactivatedBy, nameof(deactivatedBy));

        _logger.LogInformation("Deactivating product {ProductId} by {DeactivatedBy}", productId, deactivatedBy);

        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found for deactivation", productId);
                return false;
            }

            if (product.Status == ProductStatus.Inactive)
            {
                _logger.LogInformation("Product {ProductId} is already inactive", productId);
                return true;
            }

            // Update product status
            var previousStatus = product.Status;
            product.Status = ProductStatus.Inactive;
            product.Audit.UpdatedBy = deactivatedBy;
            product.Audit.UpdatedOn = DateTime.UtcNow;

            await _unitOfWork.Products.UpdateAsync(productId, product);

            // Create audit entry
            var auditEntry = new AuditEntry
            {
                EntryId = Guid.NewGuid(),
                EntityType = nameof(EnterpriseProduct),
                EntityId = productId.ToString(),
                ActionType = "Deactivate",
                UserName = deactivatedBy,
                Timestamp = DateTime.UtcNow,
                Reason = reason ?? "Product deactivation",
                Metadata = new Dictionary<string, string>
                {
                    ["ProductId"] = product.ProductId.ToString(),
                    ["ProductName"] = product.Name,
                    ["PreviousStatus"] = previousStatus.ToString(),
                    ["NewStatus"] = ProductStatus.Inactive.ToString(),
                    ["Reason"] = reason ?? "No reason provided"
                }
            };

            await _auditService.LogAuditEntryAsync(auditEntry);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Product {ProductId} deactivated successfully", productId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating product {ProductId}", productId);
            return false;
        }
    }

    public async Task<bool> DeleteProductAsync(Guid productId, string deletedBy)
    {
        Ensure.NotDefault(productId, nameof(productId));
        Ensure.NotEmpty(deletedBy, nameof(deletedBy));

        _logger.LogInformation("Deleting product {ProductId} by {DeletedBy}", productId, deletedBy);

        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found for deletion", productId);
                return false;
            }

            // Check if product has active licenses before deletion
            // TODO: Add license dependency check when implementing license-product relationship
            
            // Mark as deleted (soft delete)
            product.Audit.IsDeleted = true;
            product.Audit.DeletedBy = deletedBy;
            product.Audit.DeletedOn = DateTime.UtcNow;
            product.Audit.IsActive = false;

            await _unitOfWork.Products.UpdateAsync(productId, product);

            // Create audit entry
            var auditEntry = new AuditEntry
            {
                EntryId = Guid.NewGuid(),
                EntityType = nameof(EnterpriseProduct),
                EntityId = productId.ToString(),
                ActionType = "Delete",
                UserName = deletedBy,
                Timestamp = DateTime.UtcNow,
                Reason = "Product deletion (soft delete)",
                Metadata = new Dictionary<string, string>
                {
                    ["ProductId"] = product.ProductId.ToString(),
                    ["ProductName"] = product.Name,
                    ["ProductStatus"] = product.Status.ToString(),
                    ["DeletionType"] = "SoftDelete"
                }
            };

            await _auditService.LogAuditEntryAsync(auditEntry);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Product {ProductId} deleted successfully (soft delete)", productId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", productId);
            return false;
        }
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
        try
        {
            _logger.LogInformation("Retrieving all active products");
            
            var activeProducts = await _unitOfWork.Products.GetActiveProductsAsync();
            
            _logger.LogInformation("Retrieved {Count} active products", activeProducts.Count());
            return activeProducts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active products");
            throw;
        }
    }

    public async Task<int> GetProductCountAsync(ProductStatus? status = null, string? searchTerm = null)
    {
        try
        {
            _logger.LogInformation("Retrieving product count with status: {Status}, searchTerm: {SearchTerm}", 
                status, searchTerm);

            var searchRequest = new SearchRequest<EnterpriseProduct>
            {
                Page = 1,
                PageSize = int.MaxValue, // Get all for counting
                Filters = new Dictionary<string, object>()
            };

            // Add status filter if provided
            if (status.HasValue)
            {
                searchRequest.Filters.Add("Status", status.Value.ToString());
            }

            // Add search term filter if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchRequest.Query = searchTerm;
            }

            // Add filter to exclude deleted products
            searchRequest.Filters.Add("Audit.IsDeleted", false);

            var searchResult = await _unitOfWork.Products.SearchAsync(searchRequest);
            var count = searchResult.TotalCount;

            _logger.LogInformation("Found {Count} products matching criteria", count);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product count");
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetProductCodesAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all product codes");

            var allProducts = await _unitOfWork.Products.GetAllAsync(CancellationToken.None);
            var productCodes = allProducts
                .Where(p => !p.Audit.IsDeleted) // Exclude deleted products
                .Select(p => p.ProductId.ToString()) // Using ProductId as code since there's no separate ProductCode property
                .Distinct()
                .OrderBy(code => code)
                .ToList();

            _logger.LogInformation("Retrieved {Count} product codes", productCodes.Count);
            return productCodes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product codes");
            throw;
        }
    }

    public async Task<bool> IsProductCodeUniqueAsync(string productCode, Guid? excludeProductId = null)
    {
        try
        {
            Ensure.NotEmpty(productCode, nameof(productCode));

            _logger.LogInformation("Checking uniqueness of product code: {ProductCode}", productCode);

            // Note: Since EnterpriseProduct doesn't have a ProductCode property,
            // we'll treat the product name as the code for uniqueness checking
            // In a real implementation, you might want to add a ProductCode property to the model
            
            var searchRequest = new SearchRequest<EnterpriseProduct>
            {
                Query = productCode,
                Filters = new Dictionary<string, object>
                {
                    ["Audit.IsDeleted"] = false
                }
            };

            var searchResult = await _unitOfWork.Products.SearchAsync(searchRequest);
            var matchingProducts = searchResult.Results.Where(p => 
                string.Equals(p.Name, productCode, StringComparison.OrdinalIgnoreCase));

            if (excludeProductId.HasValue)
            {
                matchingProducts = matchingProducts.Where(p => p.ProductId != excludeProductId.Value);
            }

            var isUnique = !matchingProducts.Any();

            _logger.LogInformation("Product code {ProductCode} uniqueness check result: {IsUnique}", 
                productCode, isUnique);

            return isUnique;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking product code uniqueness for: {ProductCode}", productCode);
            throw;
        }
    }

    public async Task<bool> IsProductNameUniqueAsync(string productName, Guid? excludeProductId = null)
    {
        try
        {
            Ensure.NotEmpty(productName, nameof(productName));

            _logger.LogInformation("Checking uniqueness of product name: {ProductName}", productName);

            // Use the repository's IsNameUniqueAsync method if available
            var isUnique = await _unitOfWork.Products.IsNameUniqueAsync(productName, excludeProductId);

            _logger.LogInformation("Product name {ProductName} uniqueness check result: {IsUnique}", 
                productName, isUnique);

            return isUnique;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking product name uniqueness for: {ProductName}", productName);
            throw;
        }
    }

    public async Task<bool> UpdateProductStatusAsync(Guid productId, ProductStatus status, string updatedBy)
    {
        Ensure.NotDefault(productId, nameof(productId));
        Ensure.NotEmpty(updatedBy, nameof(updatedBy));

        _logger.LogInformation("Updating product {ProductId} status to {Status} by {UpdatedBy}", 
            productId, status, updatedBy);

        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found for status update", productId);
                return false;
            }

            if (product.Status == status)
            {
                _logger.LogInformation("Product {ProductId} already has status {Status}", productId, status);
                return true;
            }

            // Update product status
            var previousStatus = product.Status;
            product.Status = status;
            product.Audit.UpdatedBy = updatedBy;
            product.Audit.UpdatedOn = DateTime.UtcNow;

            await _unitOfWork.Products.UpdateAsync(productId, product);

            // Create audit entry
            var auditEntry = new AuditEntry
            {
                EntryId = Guid.NewGuid(),
                EntityType = nameof(EnterpriseProduct),
                EntityId = productId.ToString(),
                ActionType = "StatusUpdate",
                UserName = updatedBy,
                Timestamp = DateTime.UtcNow,
                Reason = $"Product status updated to {status}",
                Metadata = new Dictionary<string, string>
                {
                    ["ProductId"] = product.ProductId.ToString(),
                    ["ProductName"] = product.Name,
                    ["PreviousStatus"] = previousStatus.ToString(),
                    ["NewStatus"] = status.ToString()
                }
            };

            await _auditService.LogAuditEntryAsync(auditEntry);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Product {ProductId} status updated successfully from {PreviousStatus} to {NewStatus}", 
                productId, previousStatus, status);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId} status to {Status}", productId, status);
            return false;
        }
    }

    public async Task<bool> DecommissionProductAsync(Guid productId, DateTime decommissionDate, string decommissionedBy)
    {
        Ensure.NotDefault(productId, nameof(productId));
        Ensure.NotDefault(decommissionDate, nameof(decommissionDate));
        Ensure.NotEmpty(decommissionedBy, nameof(decommissionedBy));

        _logger.LogInformation("Decommissioning product {ProductId} on {DecommissionDate} by {DecommissionedBy}", 
            productId, decommissionDate, decommissionedBy);

        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found for decommissioning", productId);
                return false;
            }

            if (product.Status == ProductStatus.Decommissioned)
            {
                _logger.LogInformation("Product {ProductId} is already decommissioned", productId);
                return true;
            }

            // Validate decommission date
            if (decommissionDate < DateTime.UtcNow.Date)
            {
                throw new ArgumentException("Decommission date cannot be in the past", nameof(decommissionDate));
            }

            // Update product status and decommission date
            var previousStatus = product.Status;
            product.Status = ProductStatus.Decommissioned;
            product.DecommissionDate = decommissionDate;
            product.Audit.UpdatedBy = decommissionedBy;
            product.Audit.UpdatedOn = DateTime.UtcNow;

            await _unitOfWork.Products.UpdateAsync(productId, product);

            // Create audit entry
            var auditEntry = new AuditEntry
            {
                EntryId = Guid.NewGuid(),
                EntityType = nameof(EnterpriseProduct),
                EntityId = productId.ToString(),
                ActionType = "Decommission",
                UserName = decommissionedBy,
                Timestamp = DateTime.UtcNow,
                Reason = "Product decommissioning",
                Metadata = new Dictionary<string, string>
                {
                    ["ProductId"] = product.ProductId.ToString(),
                    ["ProductName"] = product.Name,
                    ["PreviousStatus"] = previousStatus.ToString(),
                    ["NewStatus"] = ProductStatus.Decommissioned.ToString(),
                    ["DecommissionDate"] = decommissionDate.ToString("yyyy-MM-dd"),
                    ["DaysUntilDecommission"] = Math.Max(0, (decommissionDate - DateTime.UtcNow.Date).Days).ToString()
                }
            };

            await _auditService.LogAuditEntryAsync(auditEntry);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Product {ProductId} decommissioned successfully with date {DecommissionDate}", 
                productId, decommissionDate);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decommissioning product {ProductId}", productId);
            return false;
        }
    }

    public async Task<IEnumerable<EnterpriseProduct>> GetDeprecatedProductsAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all deprecated products");

            var searchRequest = new SearchRequest<EnterpriseProduct>
            {
                Filters = new Dictionary<string, object>
                {
                    ["Status"] = ProductStatus.Deprecated.ToString(),
                    ["Audit.IsDeleted"] = false
                }
            };

            var searchResult = await _unitOfWork.Products.SearchAsync(searchRequest);
            var deprecatedProducts = searchResult.Results;

            _logger.LogInformation("Retrieved {Count} deprecated products", deprecatedProducts.Count());
            return deprecatedProducts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deprecated products");
            throw;
        }
    }

    public async Task<IEnumerable<EnterpriseProduct>> GetProductsNearingDecommissionAsync(int daysAhead = 30)
    {
        try
        {
            _logger.LogInformation("Retrieving products nearing decommission within {DaysAhead} days", daysAhead);

            var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);
            
            var allProducts = await _unitOfWork.Products.GetAllAsync(CancellationToken.None);
            var nearingDecommission = allProducts
                .Where(p => !p.Audit.IsDeleted && 
                           p.DecommissionDate.HasValue && 
                           p.DecommissionDate.Value <= cutoffDate &&
                           p.DecommissionDate.Value >= DateTime.UtcNow)
                .OrderBy(p => p.DecommissionDate)
                .ToList();

            _logger.LogInformation("Found {Count} products nearing decommission within {DaysAhead} days", 
                nearingDecommission.Count, daysAhead);
            
            return nearingDecommission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products nearing decommission");
            throw;
        }
    }
    #endregion
    #region Product Version Management
    public async Task<ProductVersion> AddProductVersionAsync(Guid productId, ProductVersion version, string createdBy)
    {
        Ensure.NotNull(version, nameof(version)).NotDefault(version.ProductId, nameof(version.ProductId));
        Ensure.NotNull(version.Version, nameof(version.Version));
        Ensure.NotDefault(version.ReleaseDate, nameof(version.ReleaseDate)).NotInPast();
        Ensure.NotEmpty(version.ChangeLog, nameof(version.ChangeLog));

        if (!await ProductExistsAsync(productId))
            throw new InvalidOperationException($"Product with ID {productId} does not exist");
        _logger.LogInformation("Adding product version {Version} to product {ProductId}", version.Version, productId);
        var productEntity = await _unitOfWork.ProductVersions.AddAsync( version);
        await _unitOfWork.SaveChangesAsync(); 

        return productEntity;
    }

    public async Task<ProductVersion> UpdateProductVersionAsync(Guid productId, ProductVersion version, string updatedBy)
    {
        Ensure.NotNull(version, nameof(version)).NotDefault(version.ProductId, nameof(version.ProductId));
        Ensure.NotNull(version.Version, nameof(version.Version));
        Ensure.NotDefault(version.ReleaseDate, nameof(version.ReleaseDate)).NotInPast();
        Ensure.NotEmpty(version.ChangeLog, nameof(version.ChangeLog));

        if (!await ProductExistsAsync(productId))
            throw new InvalidOperationException($"Product with ID {productId} does not exist");
        _logger.LogInformation("Updating product version {Version} for product {ProductId}", version.Version, productId);

        var existingEntity = await _unitOfWork.ProductVersions.GetByIdAsync(version.VersionId);
        if (existingEntity == null || existingEntity.ProductId != productId)
            throw new InvalidOperationException($"Product version with ID {version.VersionId} does not exist for product {productId}");

        existingEntity.Version = version.Version;
        existingEntity.Name = version.Name;
        existingEntity.ReleaseDate = version.ReleaseDate;
        existingEntity.ChangeLog = version.ChangeLog; 

        var updatedEntity = await _unitOfWork.ProductVersions.UpdateAsync(existingEntity.VersionId, existingEntity);
        await _unitOfWork.SaveChangesAsync();

        return updatedEntity;
    }

    public async Task<bool> DeleteProductVersionAsync(Guid productId, ProductVersion version, string deletedBy)
    {
        Ensure.NotNull(version, nameof(version)).NotDefault(version.ProductId, nameof(version.ProductId));
        Ensure.NotNull(version.Version, nameof(version.Version));

        if (!await ProductExistsAsync(productId))
            throw new InvalidOperationException($"Product with ID {productId} does not exist");
        _logger.LogInformation("Deleting product version {Version} for product {ProductId}", version.Version, productId);

        var existingEntity = await _unitOfWork.ProductVersions.GetByIdAsync(version.VersionId);
        if (existingEntity == null || existingEntity.ProductId != productId)
            throw new InvalidOperationException($"Product version with ID {version.VersionId} does not exist for product {productId}");

        await _unitOfWork.ProductVersions.DeleteAsync(existingEntity.VersionId);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<ProductVersion>> GetProductVersionsAsync(Guid productId)
    {
        Ensure.NotDefault(productId, nameof(productId));

        if (!await ProductExistsAsync(productId))
            throw new InvalidOperationException($"Product with ID {productId} does not exist");

        _logger.LogInformation("Retrieving product versions for product {ProductId}", productId);
        var productVersions = await _unitOfWork.ProductVersions.GetByProductIdAsync(productId);
        return productVersions;
    }

    #endregion

   
    
    
}
