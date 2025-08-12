using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Management.Core.Models.User;

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
    public Guid TenantId { get; set; }    /// <summary>
    /// Available tenants for selection
    /// </summary>
    public List<SelectListItem> AvailableTenants { get; set; } = new();

    /// <summary>
    /// Role permissions for different modules
    /// </summary>
    public Dictionary<SystemModule, PermissionLevel> Permissions { get; set; } = new();
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

/// <summary>
/// View model for editing an existing role
/// </summary>
public class EditRoleViewModel
{
    /// <summary>
    /// The role identifier
    /// </summary>
    public Guid RoleId { get; set; }

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

    /// <summary>
    /// Original tenant name (for display)
    /// </summary>
    public string? TenantName { get; set; }

    /// <summary>
    /// Whether the role is active
    /// </summary>
    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;    /// <summary>
    /// Audit information
    /// </summary>
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedOn { get; set; }
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Role permissions for different modules
    /// </summary>
    public Dictionary<SystemModule, PermissionLevel> Permissions { get; set; } = new();
}

/// <summary>
/// View model for role details page
/// </summary>
public class RoleDetailsViewModel
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? RoleDescription { get; set; }
    public string? TenantName { get; set; }
    public Guid TenantId { get; set; }
    public bool IsSystemRole { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedOn { get; set; }
    public string? UpdatedBy { get; set; }    public int UsersCount { get; set; }
    public List<UserSummary> AssignedUsers { get; set; } = new();

    /// <summary>
    /// Role permissions for different modules
    /// </summary>
    public Dictionary<SystemModule, PermissionLevel> Permissions { get; set; } = new();
}

/// <summary>
/// Summary information for users assigned to a role
/// </summary>
public class UserSummary
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime LastLoginDate { get; set; }
}

/// <summary>
/// View model for module permission selection
/// </summary>
public class ModulePermissionViewModel
{
    public SystemModule Module { get; set; }
    public string ModuleName => ModuleInfo.ModuleNames.GetValueOrDefault(Module, Module.ToString());
    public string ModuleDescription => ModuleInfo.ModuleDescriptions.GetValueOrDefault(Module, "");
    public string ModuleIcon => ModuleInfo.ModuleIcons.GetValueOrDefault(Module, "fas fa-cog");
    public PermissionLevel SelectedLevel { get; set; } = PermissionLevel.None;
    public List<PermissionOption> AvailablePermissions { get; set; } = new();
}

/// <summary>
/// View model for permission option
/// </summary>
public class PermissionOption
{
    public PermissionLevel Level { get; set; }
    public string Name => ModuleInfo.PermissionNames.GetValueOrDefault(Level, Level.ToString());
    public string Description => ModuleInfo.PermissionDescriptions.GetValueOrDefault(Level, "");
    public string Color => ModuleInfo.PermissionColors.GetValueOrDefault(Level, "text-muted");
    public string BadgeColor => ModuleInfo.PermissionBadgeColors.GetValueOrDefault(Level, "bg-secondary");
    public bool IsSelected { get; set; }
}

/// <summary>
/// View model for bulk permission assignment
/// </summary>
public class BulkPermissionViewModel
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public Dictionary<SystemModule, PermissionLevel> Permissions { get; set; } = new();
}

/// <summary>
/// View model for permission summary in role cards
/// </summary>
public class PermissionSummaryViewModel
{
    public int TotalModules { get; set; }
    public int ReadOnlyCount { get; set; }
    public int ReadWriteCount { get; set; }
    public int ApproverCount { get; set; }
    public int NoAccessCount { get; set; }
    
    public string GetSummaryText()
    {
        var parts = new List<string>();
        if (ApproverCount > 0) parts.Add($"{ApproverCount} Approver");
        if (ReadWriteCount > 0) parts.Add($"{ReadWriteCount} ReadWrite");
        if (ReadOnlyCount > 0) parts.Add($"{ReadOnlyCount} ReadOnly");
        if (NoAccessCount > 0) parts.Add($"{NoAccessCount} Restricted");
        
        return parts.Any() ? string.Join(", ", parts) : "No permissions set";
    }
}
