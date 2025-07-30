namespace TechWayFit.Licensing.Management.Core.Models.User;

/// <summary>
/// Enumeration for predefined user roles
/// </summary>
public enum UserRoleType
{
    /// <summary>
    /// Read-only access to most views except System and Users
    /// </summary>
    User = 1,

    /// <summary>
    /// Can create and manage most parts except System and Users
    /// </summary>
    Manager = 2,

    /// <summary>
    /// Full access to the system
    /// </summary>
    Administrator = 3
}

/// <summary>
/// Static class containing predefined role information
/// </summary>
public static class PredefinedRoles
{
    public static readonly (string Name, string Description, bool IsAdmin) Administrator = 
        ("Administrator", "Full access to system including user management and system configuration", true);
        
    public static readonly (string Name, string Description, bool IsAdmin) Manager = 
        ("Manager", "Can create and manage licenses, consumers, products, notifications, and audits", false);
        
    public static readonly (string Name, string Description, bool IsAdmin) User = 
        ("User", "Read-only access to licenses, consumers, products, notifications, and audits", false);

    public static List<(string Name, string Description, bool IsAdmin)> GetAllRoles()
    {
        return new List<(string Name, string Description, bool IsAdmin)>
        {
            Administrator,
            Manager,
            User
        };
    }
}
