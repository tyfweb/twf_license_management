using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Core.Models.User;

/// <summary>
/// Core model for user role mapping
/// </summary>
public class UserRoleMapping
{
    public Guid MappingId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tenant identifier for multi-tenant isolation
    /// </summary>
    public Guid TenantId { get; set; } = Guid.Empty;

    /// <summary>
    /// Audit information for the user role mapping
    /// </summary>
    public AuditInfo Audit { get; set; } = new();

    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

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

    // Navigation
    public UserProfile? User { get; set; }
    public UserRole? Role { get; set; }
}
