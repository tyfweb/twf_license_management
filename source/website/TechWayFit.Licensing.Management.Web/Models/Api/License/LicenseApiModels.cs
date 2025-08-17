using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Core.Models;

namespace TechWayFit.Licensing.Management.Web.Models.Api.License;

public class CreateLicenseRequest
{
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    public Guid ConsumerId { get; set; }
    
    public Guid? TierId { get; set; }
    
    public DateTime? ExpirationDate { get; set; }
    
    public int? MaxUsers { get; set; }
    
    public List<Guid>? FeatureIds { get; set; }
    
    public Dictionary<string, object>? CustomAttributes { get; set; }
}

public class LicenseResponse
{
    public Guid Id { get; set; }
    public string LicenseKey { get; set; } = string.Empty;
    public ProductInfo Product { get; set; } = new();
    public ConsumerInfo Consumer { get; set; } = new();
    public DateTime CreatedDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public LicenseStatus Status { get; set; }
    public int? MaxUsers { get; set; }
    public string? TierName { get; set; }
    public List<string> Features { get; set; } = new();
    
    public class ProductInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
    }
    
    public class ConsumerInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Company { get; set; }
    }
}

public class GetLicensesRequest
{
    public string? SearchTerm { get; set; }
    public LicenseStatus? Status { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? ConsumerId { get; set; }
    public DateTime? ExpirationFrom { get; set; }
    public DateTime? ExpirationTo { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetLicensesResponse
{
    public List<LicenseResponse> Licenses { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ValidateLicenseRequest
{
    [Required]
    public string LicenseKey { get; set; } = string.Empty;
    
    public string? ProductVersion { get; set; }
    
    public Dictionary<string, object>? Context { get; set; }
}

public class ValidateLicenseResponse
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public LicenseResponse? License { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class UpdateLicenseStatusRequest
{
    [Required]
    public LicenseStatus Status { get; set; }
    
    public string? Reason { get; set; }
}
