using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Management.Core.Models.User;

namespace TechWayFit.Licensing.Management.Web.ViewModels.User;

/// <summary>
/// View model for creating a new user
/// </summary>
public class CreateUserViewModel
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Username can only contain letters, numbers, underscore, dot, and hyphen")]
    public string UserName { get; set; } = string.Empty;

    // Alias for View compatibility
    public string Username { 
        get => UserName; 
        set => UserName = value; 
    }

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
    public string FullName { get; set; } = string.Empty;

    // Split name properties for View compatibility
    public string FirstName { 
        get => FullName.Split(' ').FirstOrDefault() ?? string.Empty; 
        set => UpdateFullName(value, LastName); 
    }
    
    public string LastName { 
        get => string.Join(" ", FullName.Split(' ').Skip(1)); 
        set => UpdateFullName(FirstName, value); 
    }

    private void UpdateFullName(string firstName, string lastName)
    {
        FullName = $"{firstName} {lastName}".Trim();
    }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string Email { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
    public string? Department { get; set; }

    // Job title alias for Department
    public string? JobTitle { 
        get => Department; 
        set => Department = value; 
    }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Password and confirmation password do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select at least one role")]
    public List<Guid> SelectedRoleIds { get; set; } = new();

    public bool IsAdmin { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public bool RequirePasswordChange { get; set; } = true;

    // Available roles for selection
    public List<UserRole> AvailableRoles { get; set; } = new();
}
