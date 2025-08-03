using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Management.Core.Models.User;

namespace TechWayFit.Licensing.Management.Web.ViewModels.User;

/// <summary>
/// View model for editing an existing user
/// </summary>
public class EditUserViewModel
{
    public string UserId { get; set; } = string.Empty;

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

    public bool IsLocked { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsActive { get; set; } = true;

    [Required(ErrorMessage = "Please select at least one role")]
    public List<Guid> SelectedRoleIds { get; set; } = new();

    // Available roles for selection
    public List<UserRole> AvailableRoles { get; set; } = new();

    // Current user roles
    public List<UserRole> CurrentRoles { get; set; } = new();

    // Audit information
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedOn { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public int FailedLoginAttempts { get; set; }

    // Aliases for View compatibility
    public DateTime CreatedDate { 
        get => CreatedOn; 
        set => CreatedOn = value; 
    }
}
