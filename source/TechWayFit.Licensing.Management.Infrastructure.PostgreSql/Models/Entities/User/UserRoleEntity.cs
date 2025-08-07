using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.User;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.User;

/// <summary>
/// Entity representing a user role in the system
/// </summary>
public class UserRoleEntity : AuditEntity, IEntityMapper<UserRole, UserRoleEntity>
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
    public bool IsAdmin { get; set; } = false;

    /// <summary>
    /// Navigation property for user role mappings
    /// </summary>
    public virtual ICollection<UserRoleMappingEntity> UserRoles { get; set; } = new List<UserRoleMappingEntity>();

    #region IEntityMapper Implementation
    public UserRoleEntity Map(UserRole model)
    {
        if (model == null) return null!;

        return new UserRoleEntity
        {
            Id = model.RoleId,
            RoleName = model.RoleName,
            RoleDescription = model.RoleDescription,
            IsAdmin = model.IsAdmin,
            IsActive = model.Audit.IsActive,
            CreatedBy = model.Audit.CreatedBy,
            CreatedOn = model.Audit.CreatedOn,
            UpdatedBy = model.Audit.UpdatedBy,
            UpdatedOn = model.Audit.UpdatedOn
        };
    }

    /// <summary>
    /// Converts UserRoleEntity to UserRole core model
    /// </summary>
    public UserRole Map()
    {

        return new UserRole
        {
            RoleId = this.Id,
            RoleName = this.RoleName,
            RoleDescription = this.RoleDescription,
            IsAdmin = this.IsAdmin,
            Audit = new AuditInfo
            {
                IsActive = this.IsActive,
                CreatedBy = this.CreatedBy,
                CreatedOn = this.CreatedOn,
                UpdatedBy = this.UpdatedBy,
                UpdatedOn = this.UpdatedOn
            }
        };
    }
    #endregion
}
