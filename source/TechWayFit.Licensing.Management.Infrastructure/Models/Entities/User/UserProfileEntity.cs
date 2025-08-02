using TechWayFit.Licensing.Management.Core.Models.User;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.User;

/// <summary>
/// Entity representing a user profile in the system
/// </summary>
public class UserProfileEntity : BaseAuditEntity
{
    /// <summary>
    /// Unique identifier for the user
    /// </summary>
    public Guid UserId { get; set; } = Guid.NewGuid();

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
    /// Indicates if the user has been soft deleted
    /// </summary>
    public bool IsDeleted { get; set; } = false;

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

    public static UserProfileEntity FromModel(UserProfile model)
    {
        return new UserProfileEntity
        {
            UserId = model.UserId,
            UserName = model.UserName,
            FullName = model.FullName,
            Email = model.Email,
            Department = model.Department,
            IsLocked = model.IsLocked,
            IsDeleted = model.IsDeleted,
            IsAdmin = model.IsAdmin,
            LastLoginDate = model.LastLoginDate,
            FailedLoginAttempts = model.FailedLoginAttempts,
            LockedDate = model.LockedDate
        };
    }
    public UserProfile ToModel()
    {
        return new UserProfile
        {
            UserId = UserId,
            UserName = UserName,
            FullName = FullName,
            Email = Email,
            Department = Department,
            IsLocked = IsLocked,
            IsDeleted = IsDeleted,
            IsAdmin = IsAdmin,
            LastLoginDate = LastLoginDate,
            FailedLoginAttempts = FailedLoginAttempts,
            LockedDate = LockedDate,
            Roles = UserRoles.Select(urm => urm.Role).Select(role => new UserRole
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                RoleDescription = role.RoleDescription,
                IsAdmin = role.IsAdmin,
                CreatedOn = role.CreatedOn,
                CreatedBy = role.CreatedBy,
                UpdatedOn = role.UpdatedOn,
                UpdatedBy = role.UpdatedBy,
                IsActive = role.IsActive
            }).ToList()
        };
    }
}
