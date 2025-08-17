using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Web.Models.Api;
using TechWayFit.Licensing.Management.Web.Models.Api.Product;
using TechWayFit.Licensing.Management.Web.Controllers;
using TechWayFit.Licensing.Core.Models;

namespace TechWayFit.Licensing.Management.Web.Controllers.Api;

/// <summary>
/// Product Management API
/// </summary>
/// <remarks>
/// Provides comprehensive REST API endpoints for managing enterprise products, features, and tiers.
/// All endpoints require authentication and proper authorization.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[SwaggerTag("Product Management - Manage enterprise products, features, and pricing tiers")]
public class ProductApiController : BaseController
{
    private readonly ILogger<ProductApiController> _logger;
    private readonly IEnterpriseProductService _productService;
    private readonly IProductFeatureService _featureService;
    private readonly IProductTierService _tierService;

    public ProductApiController(
        ILogger<ProductApiController> logger,
        IEnterpriseProductService productService,
        IProductFeatureService featureService,
        IProductTierService tierService)
    {
        _logger = logger;
        _productService = productService;
        _featureService = featureService;
        _tierService = tierService;
    }

    /// <summary>
    /// Get all products with optional filtering
    /// </summary>
    /// <param name="request">Filter criteria including search term, pagination, and status filtering</param>
    /// <returns>Paginated list of products with metadata</returns>
    /// <response code="200">Returns the list of products successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="401">Unauthorized access</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Retrieve all products",
        Description = "Gets a paginated list of all enterprise products with optional filtering by name, status, and other criteria.",
        OperationId = "GetProducts",
        Tags = new[] { "Products" }
    )]
    [SwaggerResponse(200, "Success", typeof(JsonResponse))]
    [SwaggerResponse(400, "Bad Request - Invalid parameters")]
    [SwaggerResponse(401, "Unauthorized - Authentication required")]
    [SwaggerResponse(500, "Internal Server Error")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> GetProducts([FromQuery] GetProductsRequest request)
    {
        try
        {
            _logger.LogInformation("Getting products with filters: Search={Search}, Status={Status}", 
                request.Search, request.Status);

            var products = await _productService.GetProductsAsync(
                status: request.Status,
                searchTerm: request.Search,
                pageNumber: request.Page,
                pageSize: request.PageSize);

            var productResponses = products.Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Version = p.Version,
                Status = p.Status,
                SupportEmail = p.SupportEmail,
                SupportPhone = p.SupportPhone,
                ReleaseDate = p.ReleaseDate,
                CreatedOn = p.Audit.CreatedOn,
                UpdatedOn = p.Audit.UpdatedOn,
                LicenseCount = 0, // TODO: Get actual license count
                Features = new List<ProductFeatureResponse>(),
                Tiers = new List<ProductTierResponse>()
            }).ToList();

            var totalCount = await _productService.GetProductCountAsync(request.Status, request.Search);

            var response = new GetProductsResponse
            {
                Products = productResponses,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };

            return Ok(JsonResponse.OK(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, JsonResponse.Error("Failed to retrieve products"));
        }
    }

    /// <summary>
    /// Get a specific product by ID
    /// </summary>
    /// <param name="id">The unique identifier of the product</param>
    /// <returns>Complete product details including features and tiers</returns>
    /// <response code="200">Returns the product details successfully</response>
    /// <response code="404">Product not found</response>
    /// <response code="401">Unauthorized access</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        Summary = "Get product by ID",
        Description = "Retrieves detailed information about a specific product including its features and pricing tiers.",
        OperationId = "GetProductById",
        Tags = new[] { "Products" }
    )]
    [SwaggerResponse(200, "Success", typeof(JsonResponse))]
    [SwaggerResponse(404, "Product not found")]
    [SwaggerResponse(401, "Unauthorized - Authentication required")]
    [SwaggerResponse(500, "Internal Server Error")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> GetProduct(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting product with ID: {ProductId}", id);

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(JsonResponse.Error($"Product with ID {id} not found"));
            }

            var features = await _featureService.GetFeaturesByproductIdAsync(id);
            var tiers = await _tierService.GetTiersByProductAsync(id);

            var response = new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Version = product.Version,
                Status = product.Status,
                SupportEmail = product.SupportEmail,
                SupportPhone = product.SupportPhone,
                ReleaseDate = product.ReleaseDate,
                CreatedOn = product.Audit.CreatedOn,
                UpdatedOn = product.Audit.UpdatedOn,
                LicenseCount = 0, // TODO: Get actual license count
                Features = features.Select(f => new ProductFeatureResponse
                {
                    Id = f.FeatureId,
                    Name = f.Name,
                    Description = f.Description,
                    IsEnabled = f.IsEnabled
                }).ToList(),
                Tiers = tiers.Select(t => new ProductTierResponse
                {
                    Id = t.TierId,
                    Name = t.Name,
                    Description = t.Description,
                    MaxUsers = t.MaxUsers,
                    FeatureIds = t.Features.Select(f => f.FeatureId).ToList()
                }).ToList()
            };

            return Ok(JsonResponse.OK(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return StatusCode(500, JsonResponse.Error("Failed to retrieve product"));
        }
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="request">Product creation data including name, description, version, and contact information</param>
    /// <returns>The newly created product with generated ID and audit information</returns>
    /// <response code="201">Product created successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="401">Unauthorized access</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create new product",
        Description = "Creates a new enterprise product with the provided information. The product will be assigned a unique ID and audit trail.",
        OperationId = "CreateProduct",
        Tags = new[] { "Products" }
    )]
    [SwaggerResponse(201, "Product created successfully", typeof(JsonResponse))]
    [SwaggerResponse(400, "Bad Request - Invalid or missing required fields")]
    [SwaggerResponse(401, "Unauthorized - Authentication required")]
    [SwaggerResponse(500, "Internal Server Error")]
    [ProducesResponseType(typeof(JsonResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> CreateProduct([FromBody] CreateProductRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(JsonResponse.Error("Invalid request data"));
        }

        try
        {
            _logger.LogInformation("Creating new product: {ProductName}", request.Name);

            var currentUser = CurrentUserName;

            var product = new EnterpriseProduct
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                Version = request.Version,
                Status = request.Status,
                SupportEmail = request.SupportEmail ?? string.Empty,
                SupportPhone = request.SupportPhone ?? string.Empty,
                ReleaseDate = request.ReleaseDate ?? DateTime.UtcNow
            };

            var createdProduct = await _productService.CreateProductAsync(product, currentUser);

            // Create features if provided
            if (request.Features?.Any() == true)
            {
                foreach (var featureRequest in request.Features)
                {
                    var feature = new ProductFeature
                    {
                        FeatureId = Guid.NewGuid(),
                        ProductId = createdProduct.Id,
                        Name = featureRequest.Name,
                        Description = featureRequest.Description ?? string.Empty,
                        IsEnabled = featureRequest.IsEnabled
                    };
                    await _featureService.CreateFeatureAsync(feature, currentUser);
                }
            }

            // Create tiers if provided
            if (request.Tiers?.Any() == true)
            {
                foreach (var tierRequest in request.Tiers)
                {
                    var tier = new TechWayFit.Licensing.Management.Core.Models.Product.ProductTier
                    {
                        TierId = Guid.NewGuid(),
                        ProductId = createdProduct.Id,
                        Name = tierRequest.Name,
                        Description = tierRequest.Description ?? string.Empty,
                        MaxUsers = tierRequest.MaxUsers ?? 1,
                        Features = new List<ProductFeature>()
                    };
                    await _tierService.CreateTierAsync(tier, currentUser);
                }
            }

            // Return the created product
            return await GetProduct(createdProduct.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, JsonResponse.Error("Failed to create product"));
        }
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <param name="id">The unique identifier of the product to update</param>
    /// <param name="request">Product update data with fields to be modified</param>
    /// <returns>The updated product information</returns>
    /// <response code="200">Product updated successfully</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="404">Product not found</response>
    /// <response code="401">Unauthorized access</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id:guid}")]
    [SwaggerOperation(
        Summary = "Update existing product",
        Description = "Updates an existing product with new information. Only provided fields will be updated.",
        OperationId = "UpdateProduct",
        Tags = new[] { "Products" }
    )]
    [SwaggerResponse(200, "Product updated successfully", typeof(JsonResponse))]
    [SwaggerResponse(400, "Bad Request - Invalid or missing required fields")]
    [SwaggerResponse(404, "Product not found")]
    [SwaggerResponse(401, "Unauthorized - Authentication required")]
    [SwaggerResponse(500, "Internal Server Error")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(JsonResponse.Error("Invalid request data"));
        }

        try
        {
            _logger.LogInformation("Updating product {ProductId}", id);

            var existingProduct = await _productService.GetProductByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound(JsonResponse.Error($"Product with ID {id} not found"));
            }

            var currentUser = CurrentUserName;

            // Update product properties
            if (!string.IsNullOrEmpty(request.Name))
                existingProduct.Name = request.Name;
            if (request.Description != null)
                existingProduct.Description = request.Description;
            if (!string.IsNullOrEmpty(request.Version))
                existingProduct.Version = request.Version;
            if (request.Status.HasValue)
                existingProduct.Status = request.Status.Value;
            if (request.SupportEmail != null)
                existingProduct.SupportEmail = request.SupportEmail;
            if (request.SupportPhone != null)
                existingProduct.SupportPhone = request.SupportPhone;
            if (request.ReleaseDate.HasValue)
                existingProduct.ReleaseDate = request.ReleaseDate.Value;

            await _productService.UpdateProductAsync(existingProduct, currentUser);

            // Return updated product
            return await GetProduct(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return StatusCode(500, JsonResponse.Error("Failed to update product"));
        }
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    /// <param name="id">The unique identifier of the product to delete</param>
    /// <returns>Success confirmation message</returns>
    /// <response code="200">Product deleted successfully</response>
    /// <response code="404">Product not found</response>
    /// <response code="401">Unauthorized access</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        Summary = "Delete product",
        Description = "Permanently deletes a product and all associated data. This action cannot be undone.",
        OperationId = "DeleteProduct",
        Tags = new[] { "Products" }
    )]
    [SwaggerResponse(200, "Product deleted successfully", typeof(JsonResponse))]
    [SwaggerResponse(404, "Product not found")]
    [SwaggerResponse(401, "Unauthorized - Authentication required")]
    [SwaggerResponse(500, "Internal Server Error")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> DeleteProduct(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting product {ProductId}", id);

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(JsonResponse.Error($"Product with ID {id} not found"));
            }

            var currentUser = CurrentUserName;
            var result = await _productService.DeleteProductAsync(id, currentUser);

            if (result)
            {
                return Ok(JsonResponse.OK("Product deleted successfully"));
            }
            else
            {
                return StatusCode(500, JsonResponse.Error("Failed to delete product"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            return StatusCode(500, JsonResponse.Error("Failed to delete product"));
        }
    }

    /// <summary>
    /// Get features for a specific product
    /// </summary>
    /// <param name="id">The unique identifier of the product</param>
    /// <returns>List of all features associated with the product</returns>
    /// <response code="200">Returns the list of product features</response>
    /// <response code="404">Product not found</response>
    /// <response code="401">Unauthorized access</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:guid}/features")]
    [SwaggerOperation(
        Summary = "Get product features",
        Description = "Retrieves all features associated with a specific product.",
        OperationId = "GetProductFeatures",
        Tags = new[] { "Product Features" }
    )]
    [SwaggerResponse(200, "Success", typeof(JsonResponse))]
    [SwaggerResponse(404, "Product not found")]
    [SwaggerResponse(401, "Unauthorized - Authentication required")]
    [SwaggerResponse(500, "Internal Server Error")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> GetProductFeatures(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting features for product {ProductId}", id);

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(JsonResponse.Error($"Product with ID {id} not found"));
            }

            var features = await _featureService.GetFeaturesByproductIdAsync(id);
            var response = features.Select(f => new ProductFeatureResponse
            {
                Id = f.FeatureId,
                Name = f.Name,
                Description = f.Description,
                IsEnabled = f.IsEnabled
            }).ToList();

            return Ok(JsonResponse.OK(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving features for product {ProductId}", id);
            return StatusCode(500, JsonResponse.Error("Failed to retrieve product features"));
        }
    }

    /// <summary>
    /// Create a new feature for a product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="request">Feature creation data</param>
    /// <returns>Created feature</returns>
    [HttpPost("{id:guid}/features")]
    [ProducesResponseType(typeof(JsonResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> CreateProductFeature(Guid id, [FromBody] CreateProductFeatureRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(JsonResponse.Error("Invalid request data"));
        }

        try
        {
            _logger.LogInformation("Creating feature for product {ProductId}", id);

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(JsonResponse.Error($"Product with ID {id} not found"));
            }

            var currentUser = CurrentUserName;
            var feature = new ProductFeature
            {
                FeatureId = Guid.NewGuid(),
                ProductId = id,
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                IsEnabled = request.IsEnabled
            };

            var createdFeature = await _featureService.CreateFeatureAsync(feature, currentUser);
            var response = new ProductFeatureResponse
            {
                Id = createdFeature.FeatureId,
                Name = createdFeature.Name,
                Description = createdFeature.Description,
                IsEnabled = createdFeature.IsEnabled
            };

            return CreatedAtAction(nameof(GetProductFeatures), new { id }, JsonResponse.OK(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating feature for product {ProductId}", id);
            return StatusCode(500, JsonResponse.Error("Failed to create product feature"));
        }
    }

    /// <summary>
    /// Get tiers for a specific product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>List of product tiers</returns>
    [HttpGet("{id:guid}/tiers")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> GetProductTiers(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting tiers for product {ProductId}", id);

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(JsonResponse.Error($"Product with ID {id} not found"));
            }

            var tiers = await _tierService.GetTiersByProductAsync(id);
            var response = tiers.Select(t => new ProductTierResponse
            {
                Id = t.TierId,
                Name = t.Name,
                Description = t.Description,
                MaxUsers = t.MaxUsers,
                FeatureIds = t.Features.Select(f => f.FeatureId).ToList()
            }).ToList();

            return Ok(JsonResponse.OK(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tiers for product {ProductId}", id);
            return StatusCode(500, JsonResponse.Error("Failed to retrieve product tiers"));
        }
    }

    /// <summary>
    /// Create a new tier for a product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="request">Tier creation data</param>
    /// <returns>Created tier</returns>
    [HttpPost("{id:guid}/tiers")]
    [ProducesResponseType(typeof(JsonResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> CreateProductTier(Guid id, [FromBody] CreateProductTierRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(JsonResponse.Error("Invalid request data"));
        }

        try
        {
            _logger.LogInformation("Creating tier for product {ProductId}", id);

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(JsonResponse.Error($"Product with ID {id} not found"));
            }

            var currentUser = CurrentUserName;
            var tier = new TechWayFit.Licensing.Management.Core.Models.Product.ProductTier
            {
                TierId = Guid.NewGuid(),
                ProductId = id,
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                MaxUsers = request.MaxUsers ?? 1,
                Features = new List<ProductFeature>()
            };

            var createdTier = await _tierService.CreateTierAsync(tier, currentUser);
            var response = new ProductTierResponse
            {
                Id = createdTier.TierId,
                Name = createdTier.Name,
                Description = createdTier.Description,
                MaxUsers = createdTier.MaxUsers,
                FeatureIds = createdTier.Features.Select(f => f.FeatureId).ToList()
            };

            return CreatedAtAction(nameof(GetProductTiers), new { id }, JsonResponse.OK(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tier for product {ProductId}", id);
            return StatusCode(500, JsonResponse.Error("Failed to create product tier"));
        }
    }
}
