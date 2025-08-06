using TechWayFit.Licensing.Management.Core.Models.User;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.User;

/// <summary>
/// Entity representing a user profile in the system
/// </summary>
public class UserProfileEntity : AuditEntity
{

    /// <summary>
    /// Unique username for authentication
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password for authentication
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Salt used for password hashing
    /// </summary>
    public string PasswordSalt { get; set; } = string.Empty;

    /// <summary>
    /// Full display name of the user
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the user
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Department or organizational unit
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// Indicates if the user account is locked (suspended)
    /// </summary>
    public bool IsLocked { get; set; } = false;

    /// <summary>
    /// Indicates if the user has administrative privileges
    /// </summary>
    public bool IsAdmin { get; set; } = false;

    /// <summary>
    /// Date when the user last logged in
    /// </summary>
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    /// Number of failed login attempts
    /// </summary>
    public int FailedLoginAttempts { get; set; } = 0;

    /// <summary>
    /// Date when the account was locked
    /// </summary>
    public DateTime? LockedDate { get; set; }

    /// <summary>
    /// Navigation property for user role mappings
    /// </summary>
    public virtual ICollection<UserRoleMappingEntity> UserRoles { get; set; } = new List<UserRoleMappingEntity>();
}
