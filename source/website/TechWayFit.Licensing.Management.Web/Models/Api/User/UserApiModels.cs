using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Web.Models.Api.User;

public class CreateUserRequest
{
    [Required]
    [StringLength(100)]
    public string UserName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Phone]
    public string? Phone { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public List<string>? Roles { get; set; }
    
    public Dictionary<string, string>? Metadata { get; set; }
}

public class UpdateUserRequest
{
    [StringLength(100)]
    public string? UserName { get; set; }
    
    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }
    
    [StringLength(100)]
    public string? FirstName { get; set; }
    
    [StringLength(100)]
    public string? LastName { get; set; }
    
    [Phone]
    public string? Phone { get; set; }
    
    public bool? IsActive { get; set; }
    
    public List<string>? Roles { get; set; }
    
    public Dictionary<string, string>? Metadata { get; set; }
}

public class UserResponse
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public List<string> Roles { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class GetUsersRequest
{
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public string? Role { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetUsersResponse
{
    public List<UserResponse> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string NewPassword { get; set; } = string.Empty;
    
    [Required]
    [Compare("NewPassword")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class SetPasswordRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class UserActivityResponse
{
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
