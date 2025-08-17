using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Management.Core.Models.Enums;

namespace TechWayFit.Licensing.Management.Web.Models.Api.ProductKey;

/// <summary>
/// Request model for creating a product key registration
/// </summary>
public class CreateProductKeyRequest
{
    [Required]
    public Guid ProductLicenseId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string ClientIdentifier { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Request model for activating a product key
/// </summary>
public class ActivateProductKeyRequest
{
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    [StringLength(19)]
    public string ProductKey { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string ClientIdentifier { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? ActivationReason { get; set; }
    
    public Dictionary<string, string> ClientInfo { get; set; } = new();
}

/// <summary>
/// Request model for retrieving activation details by signature
/// </summary>
public class GetActivationBySignatureRequest
{
    [Required]
    [StringLength(500)]
    public string ActivationSignature { get; set; } = string.Empty;
}

/// <summary>
/// Response model for product key registration
/// </summary>
public class ProductKeyRegistrationResponse
{
    public bool Success { get; set; }
    public string? ProductKey { get; set; }
    public ProductActivationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Response model for product key activation
/// </summary>
public class ProductActivationResponse
{
    public bool Success { get; set; }
    public string? ActivationSignature { get; set; }
    public ProductActivationStatus Status { get; set; }
    public DateTime? ActivationDate { get; set; }
    public DateTime? ActivationEndDate { get; set; }
    public string? ProductKey { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Response model for activation details
/// </summary>
public class ProductActivationDetailsResponse
{
    public Guid Id { get; set; }
    public string ProductKey { get; set; } = string.Empty;
    public ProductActivationStatus Status { get; set; }
    public string ClientIdentifier { get; set; } = string.Empty;
    public DateTime? ActivationDate { get; set; }
    public DateTime? ActivationEndDate { get; set; }
    public string? ActivationSignature { get; set; }
    public string? ActivationReason { get; set; }
    public Dictionary<string, string> ClientInfo { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Related ProductLicense Information
    public Guid ProductLicenseId { get; set; }
    public string? LicenseCode { get; set; }
    public LicenseStatus LicenseStatus { get; set; }
    public DateTime LicenseValidFrom { get; set; }
    public DateTime LicenseValidTo { get; set; }
}

/// <summary>
/// Request model for listing product keys with filtering
/// </summary>
public class GetProductKeysRequest
{
    public ProductActivationStatus? Status { get; set; }
    public string? ClientIdentifier { get; set; }
    public string? SearchTerm { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Response model for paginated product keys
/// </summary>
public class GetProductKeysResponse
{
    public List<ProductActivationDetailsResponse> ProductKeys { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>
/// Summary response for product key statistics
/// </summary>
public class ProductKeyStatsResponse
{
    public int TotalKeys { get; set; }
    public int PendingActivationKeys { get; set; }
    public int ActiveKeys { get; set; }
    public int InactiveKeys { get; set; }
    public int ExpiredKeys { get; set; }
    public int RevokedKeys { get; set; }
    public Dictionary<ProductActivationStatus, int> StatusBreakdown { get; set; } = new();
}
