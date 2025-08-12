using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.User;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.User;

/// <summary>
/// Entity representing role permissions in the system
/// </summary>
[Table("role_permissions")]
public class RolePermissionEntity : BaseEntity, IEntityMapper<RolePermission, RolePermissionEntity>
{
    /// <summary>
    /// Foreign key to UserRole
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// System module that this permission applies to
    /// </summary>
    public int SystemModule { get; set; }    /// <summary>
    /// Permission level for the module
    /// </summary>
    public int PermissionLevel { get; set; }

    /// <summary>
    /// Navigation property to UserRole
    /// </summary>
    public virtual UserRoleEntity Role { get; set; } = null!;

    #region IEntityMapper Implementation
    public RolePermissionEntity Map(RolePermission model)
    {
        if (model == null) return null!;

        Id = model.PermissionId;
        TenantId = model.TenantId;        RoleId = model.RoleId;
        SystemModule = (int)model.Module;
        PermissionLevel = (int)model.Level;
        IsActive = model.Audit.IsActive;
        CreatedBy = model.Audit.CreatedBy;
        CreatedOn = model.Audit.CreatedOn;
        UpdatedBy = model.Audit.UpdatedBy;
        UpdatedOn = model.Audit.UpdatedOn;

        return this;
    }

    /// <summary>
    /// Converts RolePermissionEntity to RolePermission core model
    /// </summary>
    public RolePermission Map()
    {
        return new RolePermission
        {
            PermissionId = this.Id,
            TenantId = this.TenantId,            RoleId = this.RoleId,
            Module = (SystemModule)this.SystemModule,
            Level = (PermissionLevel)this.PermissionLevel,
            Audit = new AuditInfo
            {
                IsActive = this.IsActive,
                IsDeleted = this.IsDeleted,
                CreatedBy = this.CreatedBy,
                CreatedOn = this.CreatedOn,
                UpdatedBy = this.UpdatedBy,
                UpdatedOn = this.UpdatedOn,
                DeletedBy = this.DeletedBy,
                DeletedOn = this.DeletedOn,
                RowVersion = this.RowVersion
            }
        };
    }
    #endregion
}
