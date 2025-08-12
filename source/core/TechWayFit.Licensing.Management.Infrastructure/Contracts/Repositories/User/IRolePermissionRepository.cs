using TechWayFit.Licensing.Management.Core.Models.User;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.User;

/// <summary>
/// Repository interface for RolePermission entities
/// </summary>
public interface IRolePermissionRepository : IDataRepository<RolePermission>
{
    /// <summary>
    /// Gets role permissions for a specific role
    /// </summary>
    Task<IEnumerable<RolePermission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific permission for a role and module
    /// </summary>
    Task<RolePermission?> GetByRoleAndModuleAsync(Guid roleId, SystemModule module, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates or creates permissions for a role
    /// </summary>
    Task<IEnumerable<RolePermission>> UpdateRolePermissionsAsync(Guid roleId, Dictionary<SystemModule, PermissionLevel> permissions, string updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all permissions for a role
    /// </summary>
    Task RemoveAllRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a role has specific permission on a module
    /// </summary>
    Task<bool> HasPermissionAsync(Guid roleId, SystemModule module, PermissionLevel requiredLevel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets effective permissions for a user across all their roles
    /// </summary>
    Task<Dictionary<SystemModule, PermissionLevel>> GetUserEffectivePermissionsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets permissions for multiple roles
    /// </summary>
    Task<Dictionary<Guid, Dictionary<SystemModule, PermissionLevel>>> GetPermissionsForRolesAsync(IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default);
}
