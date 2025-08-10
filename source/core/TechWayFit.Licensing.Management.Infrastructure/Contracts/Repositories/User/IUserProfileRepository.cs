using TechWayFit.Licensing.Management.Core.Models.User;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.User;

/// <summary>
/// Repository interface for UserProfile entities
/// </summary>
public interface IUserProfileRepository : IDataRepository<UserProfile>
{
    /// <summary>
    /// Gets a user by username
    /// </summary>
    Task<UserProfile?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by email
    /// </summary>
    Task<UserProfile?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets users with their roles
    /// </summary>
    Task<IEnumerable<UserProfile>> GetUsersWithRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets users by role
    /// </summary>
    Task<IEnumerable<UserProfile>> GetUsersByRoleAsync(string roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches users with pagination and filters
    /// </summary>
    Task<(IEnumerable<UserProfile> Users, int TotalCount)> SearchUsersAsync(
        string? searchTerm = null,
        string? departmentFilter = null,
        string? roleFilter = null,
        bool? isLockedFilter = null,
        bool? isAdminFilter = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user statistics
    /// </summary>
    Task<(int TotalUsers, int ActiveUsers, int LockedUsers, int AdminUsers)> GetUserStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if username exists
    /// </summary>
    Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if email exists
    /// </summary>
    Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates user's last login date
    /// </summary>
    Task UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates failed login attempts
    /// </summary>
    Task UpdateFailedLoginAttemptsAsync(Guid userId, int attempts, CancellationToken cancellationToken = default);

    /// <summary>
    /// Locks a user account
    /// </summary>
    Task LockUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unlocks a user account
    /// </summary>
    Task UnlockUserAsync(Guid userId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Validates the user's password
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    Task<bool> ValidatePasswordAsync(string username, string password);
    /// <summary>
    /// Updates the user's password
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="newPassword"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> UpdatePasswordAsync(Guid userId, string newPassword, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets a user profile by ID
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<UserProfile?> GetByIdAsync(Guid userId);
    /// <summary>
    /// Gets all available departments from user profiles
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<string>> GetAvailableDepartmentsAsync();
}
