namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.User;

/// <summary>
/// Entity representing a user role in the system
/// </summary>
public class UserRoleEntity : BaseDbEntity
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
}
