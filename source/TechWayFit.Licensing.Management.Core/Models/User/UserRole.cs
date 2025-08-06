using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Core.Models.User;

/// <summary>
/// Core model for user role
/// </summary>
public class UserRole
{
    public Guid RoleId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Audit information for the user role
    /// </summary>
    public AuditInfo Audit { get; set; } = new();

    public string RoleName { get; set; } = string.Empty;
    public string? RoleDescription { get; set; }
    public bool IsAdmin { get; set; }

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
}
