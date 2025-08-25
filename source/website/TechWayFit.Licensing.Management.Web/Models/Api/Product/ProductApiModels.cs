using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Management.Web.Models.Api.Product;

/// <summary>
/// Request model for creating a new product
/// </summary>
[SwaggerSchema("Request model for creating a new enterprise product")]
public class CreateProductRequest
{
    /// <summary>
    /// Product name (required, max 100 characters)
    /// </summary>
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
    [SwaggerSchema("The name of the product")]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Product description (optional, max 500 characters)
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [SwaggerSchema("Detailed description of the product")]
    public string? Description { get; set; }
    
    /// <summary>
    /// Product version (required, max 20 characters)
    /// </summary>
    [Required(ErrorMessage = "Product version is required")]
    [StringLength(20, ErrorMessage = "Version cannot exceed 20 characters")]
    [SwaggerSchema("The version number of the product")]
    public string Version { get; set; } = string.Empty;
    
    /// <summary>
    /// Support email address (optional, must be valid email format)
    /// </summary>
    [EmailAddress(ErrorMessage = "Please provide a valid email address")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    [SwaggerSchema("Contact email for product support")]
    public string? SupportEmail { get; set; }
    
    /// <summary>
    /// Support phone number (optional)
    /// </summary>
    [Phone(ErrorMessage = "Please provide a valid phone number")]
    [StringLength(50, ErrorMessage = "Phone number cannot exceed 50 characters")]
    [SwaggerSchema("Contact phone for product support")]
    public string? SupportPhone { get; set; }
    
    /// <summary>
    /// Product release date (optional, defaults to current date)
    /// </summary>
    [SwaggerSchema("The date when the product was or will be released")]
    public DateTime? ReleaseDate { get; set; }
    
    /// <summary>
    /// Product status (defaults to Active)
    /// </summary>
    [SwaggerSchema("Current status of the product")]
    public ProductStatus Status { get; set; } = ProductStatus.Active;
    
    /// <summary>
    /// Initial features to create with the product (optional)
    /// </summary>
    [SwaggerSchema("List of features to create with the product")]
    public List<CreateProductFeatureRequest>? Features { get; set; }
    
    /// <summary>
    /// Initial pricing tiers to create with the product (optional)
    /// </summary>
    [SwaggerSchema("List of pricing tiers to create with the product")]
    public List<CreateProductTierRequest>? Tiers { get; set; }
}

/// <summary>
/// Request model for creating a product feature
/// </summary>
[SwaggerSchema("Request model for creating a new product feature")]
public class CreateProductFeatureRequest
{
    /// <summary>
    /// Feature name (required)
    /// </summary>
    [Required(ErrorMessage = "Feature name is required")]
    [SwaggerSchema("The name of the feature")]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Feature description (optional)
    /// </summary>
    [SwaggerSchema("Detailed description of the feature")]
    public string? Description { get; set; }
    
    /// <summary>
    /// Whether the feature is enabled by default
    /// </summary>
    [SwaggerSchema("Indicates if the feature is enabled by default")]
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Feature configuration settings (optional)
    /// </summary>
    [SwaggerSchema("Additional configuration settings for the feature")]
    public Dictionary<string, object>? Configuration { get; set; }
}

/// <summary>
/// Request model for creating a product tier
/// </summary>
[SwaggerSchema("Request model for creating a new product pricing tier")]
public class CreateProductTierRequest
{
    /// <summary>
    /// Tier name (required)
    /// </summary>
    [Required(ErrorMessage = "Tier name is required")]
    [SwaggerSchema("The name of the pricing tier")]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Tier description (optional)
    /// </summary>
    [SwaggerSchema("Detailed description of the pricing tier")]
    public string? Description { get; set; }
    
    /// <summary>
    /// Tier price (optional)
    /// </summary>
    [SwaggerSchema("The price for this tier")]
    public decimal? Price { get; set; }
    
    /// <summary>
    /// Maximum number of users for this tier (optional)
    /// </summary>
    [SwaggerSchema("Maximum number of users allowed in this tier")]
    public int? MaxUsers { get; set; }
    
    /// <summary>
    /// Feature IDs included in this tier (optional)
    /// </summary>
    [SwaggerSchema("List of feature IDs included in this pricing tier")]
    public List<Guid>? FeatureIds { get; set; }
}

public class UpdateProductRequest
{
    [StringLength(100)]
    public string? Name { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(20)]
    public string? Version { get; set; }
    
    [EmailAddress]
    [StringLength(255)]
    public string? SupportEmail { get; set; }
    
    [Phone]
    [StringLength(50)]
    public string? SupportPhone { get; set; }
    
    public DateTime? ReleaseDate { get; set; }
    
    public ProductStatus? Status { get; set; }
}

public class ProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Version { get; set; } = string.Empty;
    public string? SupportEmail { get; set; }
    public string? SupportPhone { get; set; }
    public DateTime ReleaseDate { get; set; }
    public ProductStatus Status { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public int LicenseCount { get; set; }
    public List<ProductFeatureResponse> Features { get; set; } = new();
    public List<ProductTierResponse> Tiers { get; set; } = new();
}

public class ProductFeatureResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    public Dictionary<string, object>? Configuration { get; set; }
}

public class ProductTierResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? MaxUsers { get; set; }
    public List<Guid> FeatureIds { get; set; } = new();
}

public class GetProductsRequest
{
    public string? Search { get; set; }
    public ProductStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetProductsResponse
{
    public List<ProductResponse> Products { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ProductVersionResponse
{
    public Guid Id { get; set; }
    public string Version { get; set; } = string.Empty;
    public string? ReleaseNotes { get; set; }
    public DateTime ReleaseDate { get; set; }
    public bool IsActive { get; set; }
}

public class CreateProductVersionRequest
{
    [Required]
    [StringLength(20)]
    public string Version { get; set; } = string.Empty;
    
    public string? ReleaseNotes { get; set; }
    
    public DateTime? ReleaseDate { get; set; }
    
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Request model for deactivating a product
/// </summary>
[SwaggerSchema("Request model for deactivating a product")]
public class DeactivateProductRequest
{
    /// <summary>
    /// Optional reason for deactivation
    /// </summary>
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    [SwaggerSchema("The reason for deactivating the product")]
    public string? Reason { get; set; }
}

/// <summary>
/// Request model for updating product status
/// </summary>
[SwaggerSchema("Request model for updating product status")]
public class UpdateProductStatusRequest
{
    /// <summary>
    /// New product status
    /// </summary>
    [Required(ErrorMessage = "Status is required")]
    [SwaggerSchema("The new status for the product")]
    public ProductStatus Status { get; set; }
    
    /// <summary>
    /// Optional reason for status change
    /// </summary>
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    [SwaggerSchema("The reason for changing the product status")]
    public string? Reason { get; set; }
}

/// <summary>
/// Request model for decommissioning a product
/// </summary>
[SwaggerSchema("Request model for decommissioning a product")]
public class DecommissionProductRequest
{
    /// <summary>
    /// Date when the product should be decommissioned
    /// </summary>
    [Required(ErrorMessage = "Decommission date is required")]
    [SwaggerSchema("The date when the product will be decommissioned")]
    public DateTime DecommissionDate { get; set; }
    
    /// <summary>
    /// Optional reason for decommissioning
    /// </summary>
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    [SwaggerSchema("The reason for decommissioning the product")]
    public string? Reason { get; set; }
}
