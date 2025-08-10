using TechWayFit.Licensing.Management.Core.Models.User;

namespace TechWayFit.Licensing.Management.Web.ViewModels.User;

/// <summary>
/// View model for user list/index page
/// </summary>
public class UserListViewModel
{
    public List<UserProfile> Users { get; set; } = new();
    public string? SearchTerm { get; set; }
    public string? DepartmentFilter { get; set; }
    public string? RoleFilter { get; set; }
    public string? StatusFilter { get; set; }
    public bool? IsLockedFilter { get; set; }
    public bool? IsAdminFilter { get; set; }
    
    // Pagination
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalUsers { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalUsers / PageSize);
    
    // Statistics
    public int ActiveUsers { get; set; }
    public int LockedUsers { get; set; }
    public int AdminUsers { get; set; }
    
    // Available filters
    public List<string> AvailableDepartments { get; set; } = new();
    public List<UserRole> AvailableRoles { get; set; } = new();
}

/// <summary>
/// View model for user details page
/// </summary>
public class UserDetailsViewModel
{
    public UserProfile User { get; set; } = new();
    public List<UserRole> UserRoles { get; set; } = new();
    public List<UserRoleMapping> RoleMappings { get; set; } = new();
    
    // Activity information
    public DateTime? LastLoginDate { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedDate { get; set; }
}

/// <summary>
/// View model for change password
/// </summary>
public class ChangePasswordViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Current password is required")]
    [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "New password is required")]
    [System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Please confirm your new password")]
    [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
    [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "New password and confirmation password do not match")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
