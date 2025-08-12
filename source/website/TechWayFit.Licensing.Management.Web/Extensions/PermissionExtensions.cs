using System.Security.Claims;
using TechWayFit.Licensing.Management.Core.Models.User;
using TechWayFit.Licensing.Management.Core.Contracts.Services;

namespace TechWayFit.Licensing.Management.Web.Extensions;

/// <summary>
/// Extension methods for permission checking
/// </summary>
public static class PermissionExtensions
{
    /// <summary>
    /// Checks if the current user has the required permission
    /// </summary>
    public static async Task<bool> HasPermissionAsync(this ClaimsPrincipal user, IPermissionService permissionService, SystemModule module, PermissionLevel requiredLevel = PermissionLevel.ReadOnly)
    {
        if (!user.Identity?.IsAuthenticated ?? true)
            return false;

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return false;

        try
        {
            return await permissionService.UserHasPermissionAsync(userId, module, requiredLevel);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Synchronous version of permission check (use sparingly in views)
    /// </summary>
    public static bool HasPermission(this ClaimsPrincipal user, IPermissionService permissionService, SystemModule module, PermissionLevel requiredLevel = PermissionLevel.ReadOnly)
    {
        return HasPermissionAsync(user, permissionService, module, requiredLevel).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets all permissions for the current user
    /// </summary>
    public static async Task<Dictionary<SystemModule, PermissionLevel>> GetUserPermissionsAsync(this ClaimsPrincipal user, IPermissionService permissionService)
    {
        if (!user.Identity?.IsAuthenticated ?? true)
            return new Dictionary<SystemModule, PermissionLevel>();

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return new Dictionary<SystemModule, PermissionLevel>();

        try
        {
            return await permissionService.GetUserPermissionsAsync(userId);
        }
        catch
        {
            return new Dictionary<SystemModule, PermissionLevel>();
        }
    }

    /// <summary>
    /// Checks if user can read a specific module
    /// </summary>
    public static async Task<bool> CanReadAsync(this ClaimsPrincipal user, IPermissionService permissionService, SystemModule module)
    {
        return await user.HasPermissionAsync(permissionService, module, PermissionLevel.ReadOnly);
    }

    /// <summary>
    /// Checks if user can write to a specific module
    /// </summary>
    public static async Task<bool> CanWriteAsync(this ClaimsPrincipal user, IPermissionService permissionService, SystemModule module)
    {
        return await user.HasPermissionAsync(permissionService, module, PermissionLevel.ReadWrite);
    }

    /// <summary>
    /// Checks if user can approve in a specific module
    /// </summary>
    public static async Task<bool> CanApproveAsync(this ClaimsPrincipal user, IPermissionService permissionService, SystemModule module)
    {
        return await user.HasPermissionAsync(permissionService, module, PermissionLevel.Approver);
    }
}
