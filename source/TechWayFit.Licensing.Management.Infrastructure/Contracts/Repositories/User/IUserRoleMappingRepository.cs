using TechWayFit.Licensing.Management.Core.Models.User;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common; 

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.User;

/// <summary>
/// Repository interface for UserRoleMapping entities
/// </summary>
public interface IUserRoleMappingRepository : IDataRepository<UserRoleMapping>
{
    /// <summary>
    /// Gets user role mappings for a specific user
    /// </summary>
    Task<IEnumerable<UserRoleMapping>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user role mappings for a specific role
    /// </summary>
    Task<IEnumerable<UserRoleMapping>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific user-role mapping
    /// </summary>
    Task<UserRoleMapping?> GetByUserAndRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has a specific role
    /// </summary>
    Task<bool> UserHasRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all roles from a user
    /// </summary>
    Task RemoveAllUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a specific role from a user
    /// </summary>
    Task RemoveUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a role to a user if not already assigned
    /// </summary>
    Task<UserRoleMapping> AssignRoleToUserAsync(Guid userId, Guid roleId, string assignedBy, CancellationToken cancellationToken = default);
}
