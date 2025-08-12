using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Core.Models.User;

/// <summary>
/// Enumeration for system modules/sections
/// </summary>
public enum SystemModule
{
    Products = 1,
    Consumers = 2,
    Licenses = 3,
    Users = 4,
    Roles = 5,
    Tenants = 6,
    Approvals = 7,
    Reports = 8,
    Audit = 9,
    System = 10
}

/// <summary>
/// Enumeration for permission levels
/// </summary>
public enum PermissionLevel
{
    None = 0,
    ReadOnly = 1,
    ReadWrite = 2,
    Approver = 3
}

/// <summary>
/// Model for role permissions mapping
/// </summary>
public class RolePermission
{
    public Guid PermissionId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Tenant identifier for multi-tenant isolation
    /// </summary>
    public Guid TenantId { get; set; } = Guid.Empty;
    
    /// <summary>
    /// Role this permission belongs to
    /// </summary>
    public Guid RoleId { get; set; }
    
    /// <summary>
    /// System module/section
    /// </summary>
    public SystemModule Module { get; set; }
    
    /// <summary>
    /// Permission level for this module
    /// </summary>
    public PermissionLevel Level { get; set; }
    
    /// <summary>
    /// Audit information
    /// </summary>
    public AuditInfo Audit { get; set; } = new();
    
    // Aliases for backward compatibility
    public DateTime CreatedOn { 
        get => Audit.CreatedOn; 
        set => Audit.CreatedOn = value; 
    }
    
    public string CreatedBy { 
        get => Audit.CreatedBy; 
        set => Audit.CreatedBy = value; 
    }
    
    public DateTime? UpdatedOn { 
        get => Audit.UpdatedOn; 
        set => Audit.UpdatedOn = value; 
    }
    
    public string? UpdatedBy { 
        get => Audit.UpdatedBy; 
        set => Audit.UpdatedBy = value; 
    }
    
    public bool IsActive { 
        get => Audit.IsActive; 
        set => Audit.IsActive = value; 
    }
    
    // Navigation properties
    public UserRole? Role { get; set; }
}

/// <summary>
/// Static helper class for module information
/// </summary>
public static class ModuleInfo
{
    public static readonly Dictionary<SystemModule, string> ModuleNames = new()
    {
        { SystemModule.Products, "Products" },
        { SystemModule.Consumers, "Consumers" },
        { SystemModule.Licenses, "Licenses" },
        { SystemModule.Users, "Users" },
        { SystemModule.Roles, "Roles" },
        { SystemModule.Tenants, "Tenants" },
        { SystemModule.Approvals, "Approvals" },
        { SystemModule.Reports, "Reports" },
        { SystemModule.Audit, "Audit" },
        { SystemModule.System, "System" }
    };
    
    public static readonly Dictionary<SystemModule, string> ModuleDescriptions = new()
    {
        { SystemModule.Products, "Manage enterprise products and their configurations" },
        { SystemModule.Consumers, "Manage consumer accounts and their details" },
        { SystemModule.Licenses, "Manage product licenses and their assignments" },
        { SystemModule.Users, "Manage user accounts and profiles" },
        { SystemModule.Roles, "Manage user roles and permissions" },
        { SystemModule.Tenants, "Manage tenant organizations" },
        { SystemModule.Approvals, "Manage approval workflows and processes" },
        { SystemModule.Reports, "Access system reports and analytics" },
        { SystemModule.Audit, "View audit logs and security information" },
        { SystemModule.System, "System administration and configuration" }
    };
    
    public static readonly Dictionary<SystemModule, string> ModuleIcons = new()
    {
        { SystemModule.Products, "fas fa-box" },
        { SystemModule.Consumers, "fas fa-users" },
        { SystemModule.Licenses, "fas fa-key" },
        { SystemModule.Users, "fas fa-user" },
        { SystemModule.Roles, "fas fa-user-shield" },
        { SystemModule.Tenants, "fas fa-building" },
        { SystemModule.Approvals, "fas fa-check-circle" },
        { SystemModule.Reports, "fas fa-chart-bar" },
        { SystemModule.Audit, "fas fa-shield-alt" },
        { SystemModule.System, "fas fa-cogs" }
    };
    
    public static readonly Dictionary<PermissionLevel, string> PermissionNames = new()
    {
        { PermissionLevel.None, "No Access" },
        { PermissionLevel.ReadOnly, "Read Only" },
        { PermissionLevel.ReadWrite, "Read/Write" },
        { PermissionLevel.Approver, "Approver" }
    };
    
    public static readonly Dictionary<PermissionLevel, string> PermissionDescriptions = new()
    {
        { PermissionLevel.None, "No access to this module" },
        { PermissionLevel.ReadOnly, "Can view data but cannot create, edit, or delete" },
        { PermissionLevel.ReadWrite, "Can view, create, edit, and delete data" },
        { PermissionLevel.Approver, "Can approve submissions and perform all read/write operations" }
    };
    
    public static readonly Dictionary<PermissionLevel, string> PermissionColors = new()
    {
        { PermissionLevel.None, "text-muted" },
        { PermissionLevel.ReadOnly, "text-info" },
        { PermissionLevel.ReadWrite, "text-warning" },
        { PermissionLevel.Approver, "text-success" }
    };
    
    public static readonly Dictionary<PermissionLevel, string> PermissionBadgeColors = new()
    {
        { PermissionLevel.None, "bg-secondary" },
        { PermissionLevel.ReadOnly, "bg-info" },
        { PermissionLevel.ReadWrite, "bg-warning" },
        { PermissionLevel.Approver, "bg-success" }
    };
}
