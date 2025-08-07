using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Core.Models.User;

/// <summary>
/// Core model for user profile
/// </summary>
public class UserProfile
{
    public Guid UserId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Audit information for the user profile
    /// </summary>
    public AuditInfo Audit { get; set; } = new();

    /// <summary>
    /// Workflow information for the user profile
    /// </summary>
    public WorkflowInfo Workflow { get; set; } = new();
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Department { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedDate { get; set; }
    public bool IsAdmin { get; set; }

    public string Password{ get; set; } = string.Empty;

    // Navigation
    public List<UserRole> Roles { get; set; } = new();
}
