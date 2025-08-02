namespace TechWayFit.Licensing.Management.Core.Models.User;

/// <summary>
/// Core model for user role mapping
/// </summary>
public class UserRoleMapping
{
    public Guid MappingId { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedOn { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public UserProfile? User { get; set; }
    public UserRole? Role { get; set; }
}
