using TechWayFit.Licensing.Management.Core.Models.User;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.User;

namespace TechWayFit.Licensing.Management.Infrastructure.Extensions.Mapping;

/// <summary>
/// Extension methods for mapping between UserRole core model and UserRoleEntity
/// </summary>
public static class UserRoleMappingExtensions
{
    /// <summary>
    /// Converts UserRole core model to UserRoleEntity
    /// </summary>
    public static UserRoleEntity ToEntity(this UserRole model)
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
    public static UserRole ToModel(this UserRoleEntity entity)
    {
        if (entity == null) return null!;

        return new UserRole
        {
            RoleId = entity.Id,
            RoleName = entity.RoleName,
            RoleDescription = entity.RoleDescription,
            IsAdmin = entity.IsAdmin,
            Audit = new AuditInfo
            {
                IsActive = entity.IsActive,
                CreatedBy = entity.CreatedBy,
                CreatedOn = entity.CreatedOn,
                UpdatedBy = entity.UpdatedBy,
                UpdatedOn = entity.UpdatedOn
            }
        };
    }

    /// <summary>
    /// Updates existing UserRoleEntity with values from UserRole core model
    /// </summary>
    public static void UpdateFromModel(this UserRoleEntity entity, UserRole model)
    {
        if (entity == null || model == null) return;

        entity.RoleName = model.RoleName;
        entity.RoleDescription = model.RoleDescription;
        entity.IsAdmin = model.IsAdmin;
        entity.IsActive = model.Audit.IsActive;
        entity.UpdatedBy = model.Audit.UpdatedBy;
        entity.UpdatedOn = model.Audit.UpdatedOn;
    }
}
