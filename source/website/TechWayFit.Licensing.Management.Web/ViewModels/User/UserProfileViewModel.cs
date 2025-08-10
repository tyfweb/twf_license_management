using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Web.ViewModels.User
{
    /// <summary>
    /// ViewModel for UserProfile operations
    /// </summary>
    public class UserProfileViewModel
    {
        public Guid UserId { get; set; }
        public Guid TenantId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, ErrorMessage = "Username cannot exceed 100 characters")]
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(200, ErrorMessage = "Full name cannot exceed 200 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
        [Display(Name = "Department")]
        public string? Department { get; set; }

        [Display(Name = "Is Locked")]
        public bool IsLocked { get; set; }

        [Display(Name = "Last Login Date")]
        public DateTime? LastLoginDate { get; set; }

        [Display(Name = "Failed Login Attempts")]
        public int FailedLoginAttempts { get; set; }

        [Display(Name = "Locked Date")]
        public DateTime? LockedDate { get; set; }

        [Display(Name = "Is Administrator")]
        public bool IsAdmin { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created Date")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; } = string.Empty;

        [Display(Name = "Updated Date")]
        public DateTime? UpdatedOn { get; set; }

        [Display(Name = "Updated By")]
        public string? UpdatedBy { get; set; }

        // Password fields for creation/edit
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }

        // Roles
        public List<string> Roles { get; set; } = new();
        public List<string> AvailableRoles { get; set; } = new();

        // User ID for profile ID compatibility
        public Guid UserProfileId => UserId;
        public DateTime? CreatedDate => CreatedOn;
    }

    /// <summary>
    /// ViewModel for User details
    /// </summary>
    public class UserDetailViewModel : UserProfileViewModel
    {
        public DateTime? LastPasswordChange { get; set; }
        public int TotalLoginCount { get; set; }
        public List<UserLoginHistoryViewModel> LoginHistory { get; set; } = new();
        public List<UserActivityViewModel> RecentActivity { get; set; } = new();
    }

    /// <summary>
    /// ViewModel for user login history
    /// </summary>
    public class UserLoginHistoryViewModel
    {
        public DateTime LoginDate { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public bool WasSuccessful { get; set; }
    }

    /// <summary>
    /// ViewModel for user activity
    /// </summary>
    public class UserActivityViewModel
    {
        public DateTime ActivityDate { get; set; }
        public string ActivityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? EntityType { get; set; }
        public string? EntityId { get; set; }
    }
}
