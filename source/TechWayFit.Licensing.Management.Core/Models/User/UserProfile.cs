using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Core.Models.User;

/// <summary>
/// Core model for user profile
/// </summary>
public class UserProfile : BaseAuditModel
{
    public Guid UserId { 
        get => Id; 
        set => Id = value; 
    }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Department { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedDate { get; set; }
    public bool IsAdmin { get; set; }

    // Aliases for View compatibility
    public DateTime CreatedDate { 
        get => CreatedOn; 
        set => CreatedOn = value; 
    }
    
    public Guid UserProfileId { 
        get => UserId; 
        set => UserId = value; 
    }

    // Navigation
    public List<UserRole> Roles { get; set; } = new();
}
