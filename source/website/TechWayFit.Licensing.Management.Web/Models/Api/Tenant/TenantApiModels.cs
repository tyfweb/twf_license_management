using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Web.Models.Api.Tenant;

public class CreateTenantRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Subdomain { get; set; } = string.Empty;
    
    [EmailAddress]
    public string? ContactEmail { get; set; }
    
    [Phone]
    public string? ContactPhone { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public Dictionary<string, string>? Settings { get; set; }
    
    public Dictionary<string, string>? Metadata { get; set; }
}

public class UpdateTenantRequest
{
    [StringLength(100)]
    public string? Name { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(50)]
    public string? Subdomain { get; set; }
    
    [EmailAddress]
    public string? ContactEmail { get; set; }
    
    [Phone]
    public string? ContactPhone { get; set; }
    
    public bool? IsActive { get; set; }
    
    public Dictionary<string, string>? Settings { get; set; }
    
    public Dictionary<string, string>? Metadata { get; set; }
}

public class TenantResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Subdomain { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public int UserCount { get; set; }
    public int ConsumerCount { get; set; }
    public int LicenseCount { get; set; }
    public Dictionary<string, string> Settings { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class GetTenantsRequest
{
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetTenantsResponse
{
    public List<TenantResponse> Tenants { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class TenantStatsResponse
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalConsumers { get; set; }
    public int ActiveConsumers { get; set; }
    public int TotalLicenses { get; set; }
    public int ActiveLicenses { get; set; }
    public int TotalProducts { get; set; }
    public DateTime LastActivity { get; set; }
    public Dictionary<string, int> LicensesByStatus { get; set; } = new();
    public Dictionary<string, int> ConsumersByStatus { get; set; } = new();
}

public class TenantSettingRequest
{
    [Required]
    public string Key { get; set; } = string.Empty;
    
    [Required]
    public string Value { get; set; } = string.Empty;
}

public class TenantSettingResponse
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime? ModifiedDate { get; set; }
}
