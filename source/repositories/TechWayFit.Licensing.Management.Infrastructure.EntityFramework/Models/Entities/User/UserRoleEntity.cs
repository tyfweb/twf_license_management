using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.User;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.User;

/// <summary>
/// Entity representing a user role in the system
/// </summary>
[Table("user_roles")]
public class UserRoleEntity : BaseEntity, IEntityMapper<UserRole, UserRoleEntity>
{

    /// <summary>
    /// Name of the role
    /// </summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// Description of the role and its permissions
    /// </summary>
    public string? RoleDescription { get; set; }

    /// <summary>
    /// Indicates if this role has administrative privileges
    /// </summary>
    public bool IsAdmin { get; set; } = false;    /// <summary>
    /// Navigation property for user role mappings
    /// </summary>
    public virtual ICollection<UserRoleMappingEntity> UserRoles { get; set; } = new List<UserRoleMappingEntity>();

    /// <summary>
    /// Navigation property for role permissions
    /// </summary>
    public virtual ICollection<RolePermissionEntity> RolePermissions { get; set; } = new List<RolePermissionEntity>();

    #region IEntityMapper Implementation
    public UserRoleEntity Map(UserRole model)
    {
        if (model == null) return null!;

        Id = model.RoleId;
        TenantId = model.TenantId;
        RoleName = model.RoleName;
        RoleDescription = model.RoleDescription;
        IsAdmin = model.IsAdmin;
        IsActive = model.Audit.IsActive;
        CreatedBy = model.Audit.CreatedBy;
        CreatedOn = model.Audit.CreatedOn;
        UpdatedBy = model.Audit.UpdatedBy;
        UpdatedOn = model.Audit.UpdatedOn;
        return this;
    }

    /// <summary>
    /// Converts UserRoleEntity to UserRole core model
    /// </summary>
    public UserRole Map()
    {

        var role = new UserRole
        {
            RoleId = this.Id,
            TenantId = this.TenantId,
            RoleName = this.RoleName,
            RoleDescription = this.RoleDescription,
            IsAdmin = this.IsAdmin
        };
        role.Audit = MapAuditInfo();
        return role;
    }
    #endregion
}
