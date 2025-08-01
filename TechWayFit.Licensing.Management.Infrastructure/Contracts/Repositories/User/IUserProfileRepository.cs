using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.User;

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.User;

/// <summary>
/// Repository interface for UserProfile entities
/// </summary>
public interface IUserProfileRepository : IBaseRepository<UserProfileEntity>
{
    /// <summary>
    /// Gets a user by username
    /// </summary>
    Task<UserProfileEntity?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by email
    /// </summary>
    Task<UserProfileEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets users with their roles
    /// </summary>
    Task<IEnumerable<UserProfileEntity>> GetUsersWithRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets users by role
    /// </summary>
    Task<IEnumerable<UserProfileEntity>> GetUsersByRoleAsync(string roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches users with pagination and filters
    /// </summary>
    Task<(IEnumerable<UserProfileEntity> Users, int TotalCount)> SearchUsersAsync(
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
}
