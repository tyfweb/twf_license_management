using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Role;

/// <summary>
/// View model for creating a new role
/// </summary>
public class CreateRoleViewModel
{
    /// <summary>
    /// The name of the role
    /// </summary>
    [Required(ErrorMessage = "Role name is required")]
    [StringLength(100, ErrorMessage = "Role name cannot exceed 100 characters")]
    [Display(Name = "Role Name")]
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// Description of the role
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [Display(Name = "Description")]
    public string? RoleDescription { get; set; }

    /// <summary>
    /// Whether this is an admin role
    /// </summary>
    [Display(Name = "Is Admin Role")]
    public bool IsAdminRole { get; set; }

    /// <summary>
    /// The tenant this role belongs to
    /// </summary>
    [Required(ErrorMessage = "Please select a tenant")]
    [Display(Name = "Tenant")]
    public Guid TenantId { get; set; }

    /// <summary>
    /// Available tenants for selection
    /// </summary>
    public List<SelectListItem> AvailableTenants { get; set; } = new();
}

/// <summary>
/// View model for displaying role information
/// </summary>
public class RoleViewModel
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? RoleDescription { get; set; }
    public string? TenantName { get; set; }
    public bool IsSystemRole { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
    public int UsersCount { get; set; }
}

/// <summary>
/// View model for role list page
/// </summary>
public class RoleListViewModel
{
    public List<RoleViewModel> Roles { get; set; } = new();
    public int TotalRoles => Roles.Count;
    public int ActiveRoles => Roles.Count(r => r.IsActive);
}
