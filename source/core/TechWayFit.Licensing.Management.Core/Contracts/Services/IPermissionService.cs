using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.User;

namespace TechWayFit.Licensing.Management.Core.Contracts.Services;

/// <summary>
/// Service interface for managing role permissions
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Gets effective permissions for a user across all their roles
    /// </summary>
    Task<Dictionary<SystemModule, PermissionLevel>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has specific permission on a module
    /// </summary>
    Task<bool> UserHasPermissionAsync(Guid userId, SystemModule module, PermissionLevel requiredLevel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates permissions for a role
    /// </summary>
    Task<IEnumerable<RolePermission>> UpdateRolePermissionsAsync(Guid roleId, Dictionary<SystemModule, PermissionLevel> permissions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets permissions for a specific role
    /// </summary>
    Task<Dictionary<SystemModule, PermissionLevel>> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets permissions for multiple roles
    /// </summary>
    Task<Dictionary<Guid, Dictionary<SystemModule, PermissionLevel>>> GetRolePermissionsBatchAsync(IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a permission level is sufficient for the required level
    /// </summary>
    bool IsPermissionSufficient(PermissionLevel currentLevel, PermissionLevel requiredLevel);

    /// <summary>
    /// Gets the highest permission level from a collection of levels
    /// </summary>
    PermissionLevel GetHighestPermissionLevel(IEnumerable<PermissionLevel> levels);

    /// <summary>
    /// Validates permission configuration for a role
    /// </summary>
    Task<ValidationResult> ValidateRolePermissionsAsync(Guid roleId, Dictionary<SystemModule, PermissionLevel> permissions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Maintenance method to cleanup duplicate permissions that might cause constraint violations
    /// Call this if you encounter unique constraint errors when updating role permissions
    /// </summary>
    Task CleanupDuplicatePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default);
}
