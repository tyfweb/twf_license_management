using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Tenant;

/// <summary>
/// View model for creating a new tenant
/// </summary>
public class CreateTenantViewModel
{
    /// <summary>
    /// The name of the tenant organization
    /// </summary>
    [Required(ErrorMessage = "Tenant name is required")]
    [StringLength(100, ErrorMessage = "Tenant name cannot exceed 100 characters")]
    [Display(Name = "Tenant Name")]
    public string TenantName { get; set; } = string.Empty;

    /// <summary>
    /// Unique code for the tenant
    /// </summary>
    [StringLength(20, ErrorMessage = "Tenant code cannot exceed 20 characters")]
    [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Tenant code can only contain uppercase letters and numbers")]
    [Display(Name = "Tenant Code")]
    public string? TenantCode { get; set; }

    /// <summary>
    /// Description of the tenant
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    /// <summary>
    /// Website URL of the tenant
    /// </summary>
    [Url(ErrorMessage = "Please enter a valid URL")]
    [StringLength(255, ErrorMessage = "Website URL cannot exceed 255 characters")]
    [Display(Name = "Website")]
    public string? Website { get; set; }
}

/// <summary>
/// View model for displaying tenant information
/// </summary>
public class TenantViewModel
{
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string? TenantCode { get; set; }
    public string? Description { get; set; }
    public string? Website { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
    public int UserCount { get; set; }
    public int LicenseCount { get; set; }
}

/// <summary>
/// View model for tenant list page
/// </summary>
public class TenantListViewModel
{
    public List<TenantViewModel> Tenants { get; set; } = new();
    public int TotalTenants => Tenants.Count;
    public int ActiveTenants => Tenants.Count(t => t.IsActive);
}
