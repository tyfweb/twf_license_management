namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.User;

/// <summary>
/// Entity representing the mapping between users and roles
/// </summary>
public class UserRoleMappingEntity : BaseDbEntity
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
}

