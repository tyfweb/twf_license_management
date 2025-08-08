using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.User;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.User;

/// <summary>
/// Entity representing the mapping between users and roles
/// </summary>
public class UserRoleMappingEntity : BaseEntity, IEntityMapper<UserRoleMapping, UserRoleMappingEntity>
{
    /// <summary>
    /// Foreign key to UserProfile
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Foreign key to UserRole
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// Date when the role was assigned
    /// </summary>
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional expiry date for the role assignment
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Navigation property to UserProfile
    /// </summary>
    public virtual UserProfileEntity User { get; set; } = null!;

    /// <summary>
    /// Navigation property to UserRole
    /// </summary>
    public virtual UserRoleEntity Role { get; set; } = null!;

    #region IEntityMapper Implementation
       public UserRoleMappingEntity Map( UserRoleMapping model)
    {
        if (model == null) return null!;

        return new UserRoleMappingEntity
        {
            Id = model.MappingId,
            TenantId = model.TenantId,
            UserId = model.UserId,
            RoleId = model.RoleId,
            AssignedDate = model.AssignedDate,
            ExpiryDate = model.ExpiryDate,
            IsActive = model.Audit.IsActive,
            CreatedBy = model.Audit.CreatedBy,
            CreatedOn = model.Audit.CreatedOn,
            UpdatedBy = model.Audit.UpdatedBy,
            UpdatedOn = model.Audit.UpdatedOn
        };
    }

    /// <summary>
    /// Converts UserRoleMappingEntity to UserRoleMapping core model
    /// </summary>
    public UserRoleMapping Map()
    { 

        return new UserRoleMapping
        {
            MappingId = this.Id,
            TenantId = this.TenantId,
            UserId = this.UserId,
            RoleId = this.RoleId,
            AssignedDate = this.AssignedDate,
            ExpiryDate = this.ExpiryDate,
            Audit = new AuditInfo
            {
                IsActive = this.IsActive,
                CreatedBy = this.CreatedBy,
                CreatedOn = this.CreatedOn,
                UpdatedBy = this.UpdatedBy,
                UpdatedOn = this.UpdatedOn
            },
            User = this.User?.Map(),
            Role = this.Role?.Map()
        };
    }
    #endregion
}

