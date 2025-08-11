using TechWayFit.Licensing.Management.Core.Models.User;

namespace TechWayFit.Licensing.Management.Core.Contracts.Services;

/// <summary>
/// Service interface for user management operations
/// </summary>
public interface IUserService
{
    #region User Management

    /// <summary>
    /// Get all users with optional filtering
    /// </summary>
    Task<IEnumerable<UserProfile>> GetAllUsersAsync(
        string? searchTerm = null,
        string? departmentFilter = null,
        string? roleFilter = null,
        bool? isLockedFilter = null,
        bool? isAdminFilter = null,
        int page = 1,
        int pageSize = 20);

    /// <summary>
    /// Get total count of users with filters
    /// </summary>
    Task<int> GetUserCountAsync(
        string? searchTerm = null,
        string? departmentFilter = null,
        string? roleFilter = null,
        bool? isLockedFilter = null,
        bool? isAdminFilter = null);

    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<UserProfile?> GetUserByIdAsync(Guid userId);

    /// <summary>
    /// Get user by username
    /// </summary>
    Task<UserProfile?> GetUserByUsernameAsync(string username);

    /// <summary>
    /// Get user by email
    /// </summary>
    Task<UserProfile?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Create a new user
    /// </summary>
    Task<(bool Success, string Message, UserProfile? User)> CreateUserAsync(
        string username,
        string password,
        string fullName,
        string email,
        string? department,
        bool isAdmin,
        List<Guid> roleIds,
        string createdBy);

    /// <summary>
    /// Update an existing user
    /// </summary>
    Task<(bool Success, string Message)> UpdateUserAsync(
        Guid userId,
        string username,
        string fullName,
        string email,
        string? department,
        bool isAdmin,
        List<Guid> roleIds,
        string updatedBy);

    /// <summary>
    /// Delete a user (soft delete)
    /// </summary>
    Task<(bool Success, string Message)> DeleteUserAsync(Guid userId, string deletedBy);

    /// <summary>
    /// Lock/unlock a user account
    /// </summary>
    Task<(bool Success, string Message)> SetUserLockStatusAsync(Guid userId, bool isLocked, string updatedBy);

    /// <summary>
    /// Change user password
    /// </summary>
    Task<(bool Success, string Message)> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, string updatedBy);

    /// <summary>
    /// Reset user password (admin only)
    /// </summary>
    Task<(bool Success, string Message, string? TempPassword)> ResetPasswordAsync(Guid userId, string resetBy);

    /// <summary>
    /// Authenticate user
    /// </summary>
    Task<(bool Success, UserProfile? User, string Message)> AuthenticateAsync(string username, string password);

    #endregion

    #region Role Management

    /// <summary>
    /// Get all roles
    /// </summary>
    Task<IEnumerable<UserRole>> GetAllRolesAsync();

    /// <summary>
    /// Get roles by tenant ID
    /// </summary>
    Task<IEnumerable<UserRole>> GetRolesByTenantAsync(Guid tenantId);

    /// <summary>
    /// Get role by ID
    /// </summary>
    Task<UserRole?> GetRoleByIdAsync(Guid roleId);

    /// <summary>
    /// Get role by name
    /// </summary>
    Task<UserRole?> GetRoleByNameAsync(string roleName);

    /// <summary>
    /// Create a new role
    /// </summary>
    Task<(bool Success, string Message, UserRole? Role)> CreateRoleAsync(
        UserRole role);

    /// <summary>
    /// Update an existing role
    /// </summary>
    Task<(bool Success, string Message)> UpdateRoleAsync(
        Guid roleId,
        UserRole role);

    /// <summary>
    /// Delete a role
    /// </summary>
    Task<(bool Success, string Message)> DeleteRoleAsync(Guid roleId, string deletedBy);

    #endregion

    #region User Role Assignments

    /// <summary>
    /// Get user roles
    /// </summary>
    Task<IEnumerable<UserRole>> GetUserRolesAsync(Guid userId);

    /// <summary>
    /// Assign role to user
    /// </summary>
    Task<(bool Success, string Message)> AssignRoleToUserAsync(
        Guid userId,
        Guid roleId,
        DateTime? expiryDate,
        string assignedBy);

    /// <summary>
    /// Remove role from user
    /// </summary>
    Task<(bool Success, string Message)> RemoveRoleFromUserAsync(Guid userId, Guid roleId, string removedBy);

    /// <summary>
    /// Update user roles (replace all existing roles)
    /// </summary>
    Task<(bool Success, string Message)> UpdateUserRolesAsync(
        Guid userId,
        List<Guid> roleIds,
        string updatedBy);

    #endregion

    #region Utility Methods

    /// <summary>
    /// Get available departments
    /// </summary>
    Task<IEnumerable<string>> GetAvailableDepartmentsAsync();

    /// <summary>
    /// Check if username is available
    /// </summary>
    Task<bool> IsUsernameAvailableAsync(string username, Guid? excludeUserId = null);

    /// <summary>
    /// Check if email is available
    /// </summary>
    Task<bool> IsEmailAvailableAsync(string email, Guid? excludeUserId = null);

    /// <summary>
    /// Validate password strength
    /// </summary>
    (bool IsValid, string Message) ValidatePassword(string password);

    #endregion
}
