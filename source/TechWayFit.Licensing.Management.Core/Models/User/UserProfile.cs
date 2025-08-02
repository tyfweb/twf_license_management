namespace TechWayFit.Licensing.Management.Core.Models.User;

/// <summary>
/// Core model for user profile
/// </summary>
public class UserProfile
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Department { get; set; }
    public bool IsLocked { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsAdmin { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedDate { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedOn { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsActive { get; set; } = true;

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
