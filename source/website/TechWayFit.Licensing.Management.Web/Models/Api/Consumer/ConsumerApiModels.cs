using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Management.Core.Models.Consumer;

namespace TechWayFit.Licensing.Management.Web.Models.Api.Consumer;

public class CreateConsumerRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? Company { get; set; }
    
    [Phone]
    public string? Phone { get; set; }
    
    public string? Address { get; set; }
    
    public string? City { get; set; }
    
    public string? Country { get; set; }
    
    public string? PostalCode { get; set; }
    
    public ConsumerStatus Status { get; set; } = ConsumerStatus.Active;
    
    public Dictionary<string, string>? Metadata { get; set; }
}

public class UpdateConsumerRequest
{
    [StringLength(100)]
    public string? Name { get; set; }
    
    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }
    
    [StringLength(100)]
    public string? Company { get; set; }
    
    [Phone]
    public string? Phone { get; set; }
    
    public string? Address { get; set; }
    
    public string? City { get; set; }
    
    public string? Country { get; set; }
    
    public string? PostalCode { get; set; }
    
    public ConsumerStatus? Status { get; set; }
    
    public Dictionary<string, string>? Metadata { get; set; }
}

public class ConsumerResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public ConsumerStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public int LicenseCount { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class GetConsumersRequest
{
    public string? SearchTerm { get; set; }
    public ConsumerStatus? Status { get; set; }
    public bool? IsActive { get; set; }
    public string? Company { get; set; }
    public string? Country { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetConsumersResponse
{
    public List<ConsumerResponse> Consumers { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ConsumerLicenseResponse
{
    public Guid Id { get; set; }
    public string LicenseKey { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductVersion { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TierName { get; set; }
}

public class GetConsumerLicensesResponse
{
    public List<ConsumerLicenseResponse> Licenses { get; set; } = new();
    public int TotalCount { get; set; }
}
