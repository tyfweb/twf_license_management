using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Management.Core.Models.Enums;
using TechWayFit.Licensing.Management.Core.Models.License;

namespace TechWayFit.Licensing.Management.Web.ViewModels.License;

/// <summary>
/// ViewModel for ProductKey management within License context
/// </summary>
public class LicenseProductKeysViewModel
{
    public ProductLicense License { get; set; } = new();
    public List<ProductKeyItemViewModel> ProductKeys { get; set; } = new();
    public ProductKeyStatsViewModel Stats { get; set; } = new();
    public ProductKeyFilterViewModel Filter { get; set; } = new();
    public PaginationViewModel Pagination { get; set; } = new();
}

/// <summary>
/// Individual ProductKey item for display
/// </summary>
public class ProductKeyItemViewModel
{
    public Guid Id { get; set; }
    public string ProductKey { get; set; } = string.Empty;
    public string ClientIdentifier { get; set; } = string.Empty;
    public ProductActivationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ActivationDate { get; set; }
    public DateTime? ActivationEndDate { get; set; }
    public string? ActivationSignature { get; set; }
    public string? Description { get; set; }
    public string? MachineId { get; set; }
    public string? MachineName { get; set; }
    public string? IpAddress { get; set; }
    public DateTime? LastHeartbeat { get; set; }
    
    // Helper properties for UI
    public string StatusCssClass => GetStatusCssClass();
    public string StatusIcon => GetStatusIcon();
    public bool CanRevoke => Status == ProductActivationStatus.PendingActivation;
    public bool CanDeactivate => Status == ProductActivationStatus.Active;
    public bool IsActive => Status == ProductActivationStatus.Active;
    public bool IsExpired => ActivationEndDate.HasValue && ActivationEndDate.Value < DateTime.UtcNow;
    public int DaysUntilExpiry => ActivationEndDate.HasValue ? 
        (int)(ActivationEndDate.Value - DateTime.UtcNow).TotalDays : 0;

    private string GetStatusCssClass()
    {
        return Status switch
        {
            ProductActivationStatus.PendingActivation => "badge-warning",
            ProductActivationStatus.Active => "badge-success",
            ProductActivationStatus.Inactive => "badge-secondary",
            ProductActivationStatus.Expired => "badge-danger",
            ProductActivationStatus.Revoked => "badge-dark",
            ProductActivationStatus.Suspended => "badge-warning",
            _ => "badge-secondary"
        };
    }

    private string GetStatusIcon()
    {
        return Status switch
        {
            ProductActivationStatus.PendingActivation => "fas fa-clock",
            ProductActivationStatus.Active => "fas fa-check-circle",
            ProductActivationStatus.Inactive => "fas fa-pause-circle",
            ProductActivationStatus.Expired => "fas fa-times-circle",
            ProductActivationStatus.Revoked => "fas fa-ban",
            ProductActivationStatus.Suspended => "fas fa-pause",
            _ => "fas fa-question-circle"
        };
    }
}

/// <summary>
/// ProductKey statistics for dashboard
/// </summary>
public class ProductKeyStatsViewModel
{
    public int TotalKeys { get; set; }
    public int PendingKeys { get; set; }
    public int ActiveKeys { get; set; }
    public int InactiveKeys { get; set; }
    public int ExpiredKeys { get; set; }
    public int RevokedKeys { get; set; }
    public int SuspendedKeys { get; set; }
    
    public Dictionary<ProductActivationStatus, int> StatusBreakdown { get; set; } = new();
    
    // Helper properties
    public double ActivePercentage => TotalKeys > 0 ? (double)ActiveKeys / TotalKeys * 100 : 0;
    public double PendingPercentage => TotalKeys > 0 ? (double)PendingKeys / TotalKeys * 100 : 0;
    public bool HasKeys => TotalKeys > 0;
    public bool HasActiveKeys => ActiveKeys > 0;
}

/// <summary>
/// Filter options for ProductKey listing
/// </summary>
public class ProductKeyFilterViewModel
{
    public ProductActivationStatus? Status { get; set; }
    public string? ClientIdentifier { get; set; }
    public string? SearchTerm { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public DateTime? ActivatedAfter { get; set; }
    public DateTime? ActivatedBefore { get; set; }
    public bool? IsExpired { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortDirection { get; set; } = "desc";
}

/// <summary>
/// ViewModel for creating a new ProductKey
/// </summary>
public class CreateProductKeyViewModel
{
    [Required]
    public Guid ProductLicenseId { get; set; }
    
    [Required]
    [StringLength(100)]
    [Display(Name = "Client Identifier")]
    public string ClientIdentifier { get; set; } = string.Empty;
    
    [StringLength(500)]
    [Display(Name = "Description")]
    public string? Description { get; set; }
    
    [Display(Name = "Additional Metadata")]
    public Dictionary<string, string> Metadata { get; set; } = new();
    
    // Context information for display
    public ProductLicense? License { get; set; }
    public string? ProductName { get; set; }
    public string? ConsumerName { get; set; }
}

/// <summary>
/// ViewModel for ProductKey details
/// </summary>
public class ProductKeyDetailsViewModel
{
    public ProductKeyItemViewModel ProductKey { get; set; } = new();
    public ProductLicense License { get; set; } = new();
    public List<ProductKeyAuditViewModel> AuditHistory { get; set; } = new();
    public ProductKeyActivationDetailsViewModel? ActivationDetails { get; set; }
    
    // Permissions
    public bool CanRevoke { get; set; }
    public bool CanDeactivate { get; set; }
    public bool CanReactivate { get; set; }
}

/// <summary>
/// Activation details for a ProductKey
/// </summary>
public class ProductKeyActivationDetailsViewModel
{
    public string? MachineId { get; set; }
    public string? MachineName { get; set; }
    public string? MachineFingerprint { get; set; }
    public string? IpAddress { get; set; }
    public DateTime? ActivationDate { get; set; }
    public DateTime? ActivationEndDate { get; set; }
    public DateTime? LastHeartbeat { get; set; }
    public string? ActivationSignature { get; set; }
    public Dictionary<string, string> ActivationData { get; set; } = new();
    public Dictionary<string, string> ClientInfo { get; set; } = new();
}

/// <summary>
/// Audit entry for ProductKey operations
/// </summary>
public class ProductKeyAuditViewModel
{
    public DateTime Timestamp { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? PerformedBy { get; set; }
    public string? Reason { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
    public string ActionIcon => GetActionIcon();
    public string ActionCssClass => GetActionCssClass();

    private string GetActionIcon()
    {
        return Action.ToLower() switch
        {
            "created" => "fas fa-plus",
            "activated" => "fas fa-check",
            "deactivated" => "fas fa-pause",
            "revoked" => "fas fa-ban",
            "renewed" => "fas fa-sync",
            _ => "fas fa-info"
        };
    }

    private string GetActionCssClass()
    {
        return Action.ToLower() switch
        {
            "created" => "text-info",
            "activated" => "text-success",
            "deactivated" => "text-warning",
            "revoked" => "text-danger",
            "renewed" => "text-primary",
            _ => "text-muted"
        };
    }
}
