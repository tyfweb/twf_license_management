namespace TechWayFit.Licensing.Management.Core.Models.Configuration;

/// <summary>
/// Configuration model for admin dashboard settings
/// </summary>
public class AdminDashboardConfiguration
{
    /// <summary>
    /// Whether the admin dashboard is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Maximum number of licenses to display per page
    /// </summary>
    public int LicensesPerPage { get; set; } = 50;

    /// <summary>
    /// Whether to enable license bulk operations
    /// </summary>
    public bool EnableBulkOperations { get; set; } = true;

    /// <summary>
    /// Whether to enable advanced license filtering
    /// </summary>
    public bool EnableAdvancedFiltering { get; set; } = true;

    /// <summary>
    /// Whether to enable license audit trail
    /// </summary>
    public bool EnableAuditTrail { get; set; } = true;

    /// <summary>
    /// Whether to enable license export functionality
    /// </summary>
    public bool EnableExport { get; set; } = true;

    /// <summary>
    /// Whether to enable user management features
    /// </summary>
    public bool EnableUserManagement { get; set; } = true;

    /// <summary>
    /// Security settings for admin dashboard
    /// </summary>
    public AdminSecuritySettings Security { get; set; } = new();
}

/// <summary>
/// Security settings for admin dashboard
/// </summary>
public class AdminSecuritySettings
{
    /// <summary>
    /// Whether to require two-factor authentication for admin operations
    /// </summary>
    public bool RequireTwoFactorAuth { get; set; } = false;

    /// <summary>
    /// Session timeout in minutes
    /// </summary>
    public int SessionTimeoutMinutes { get; set; } = 30;

    /// <summary>
    /// Whether to log all admin operations
    /// </summary>
    public bool LogAdminOperations { get; set; } = true;

    /// <summary>
    /// Maximum number of failed login attempts before lockout
    /// </summary>
    public int MaxFailedLoginAttempts { get; set; } = 5;

    /// <summary>
    /// Account lockout duration in minutes
    /// </summary>
    public int LockoutDurationMinutes { get; set; } = 15;
}
