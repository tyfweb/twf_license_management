using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TechWayFit.Licensing.Management.Core.Models.User;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using System.Security.Claims;

namespace TechWayFit.Licensing.Management.Web.Attributes;

/// <summary>
/// Authorization attribute for module-based permissions
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class ModulePermissionAttribute : Attribute, IAuthorizationFilter
{
    public SystemModule Module { get; }
    public PermissionLevel RequiredLevel { get; }

    public ModulePermissionAttribute(SystemModule module, PermissionLevel requiredLevel = PermissionLevel.ReadOnly)
    {
        Module = module;
        RequiredLevel = requiredLevel;
    }    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Check if user is authenticated
        if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Get permission service to check permissions
        var permissionService = context.HttpContext.RequestServices.GetService<IPermissionService>();
        if (permissionService == null)
        {
            context.Result = new ForbidResult();
            return;
        }

        try
        {
            // Get current user ID from claims
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                context.Result = new ForbidResult();
                return;
            }

            // Check if user has required permission
            var hasPermission = permissionService.UserHasPermissionAsync(userId, Module, RequiredLevel).Result;
            if (!hasPermission)
            {
                // Check if it's an AJAX request
                if (context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    context.Result = new JsonResult(new { success = false, message = "Insufficient permissions" })
                    {
                        StatusCode = 403
                    };
                }
                else
                {
                    context.Result = new ForbidResult();
                }
                return;
            }
        }
        catch (Exception)
        {
            // Log error in production
            context.Result = new ForbidResult();
            return;
        }
    }
}

/// <summary>
/// Authorization attribute for read-only access
/// </summary>
public class ReadOnlyAccessAttribute : ModulePermissionAttribute
{
    public ReadOnlyAccessAttribute(SystemModule module) : base(module, PermissionLevel.ReadOnly)
    {
    }
}

/// <summary>
/// Authorization attribute for read-write access
/// </summary>
public class ReadWriteAccessAttribute : ModulePermissionAttribute
{
    public ReadWriteAccessAttribute(SystemModule module) : base(module, PermissionLevel.ReadWrite)
    {
    }
}

/// <summary>
/// Authorization attribute for approver access
/// </summary>
public class ApproverAccessAttribute : ModulePermissionAttribute
{
    public ApproverAccessAttribute(SystemModule module) : base(module, PermissionLevel.Approver)
    {
    }
}

/// <summary>
/// Helper class for checking permissions in views and controllers
/// </summary>
public static class PermissionHelper
{
    /// <summary>
    /// Check if current user has the required permission level for a module
    /// </summary>
    public static bool HasPermission(this ClaimsPrincipal user, SystemModule module, PermissionLevel requiredLevel)
    {
        // TODO: Implement actual permission checking
        // For now, return true for authenticated users
        return user.Identity?.IsAuthenticated ?? false;
    }

    /// <summary>
    /// Get user's permission level for a specific module
    /// </summary>
    public static PermissionLevel GetPermissionLevel(this ClaimsPrincipal user, SystemModule module)
    {
        // TODO: Implement actual permission retrieval
        // For now, return ReadWrite for authenticated users
        if (user.Identity?.IsAuthenticated ?? false)
        {
            return PermissionLevel.ReadWrite;
        }
        return PermissionLevel.None;
    }

    /// <summary>
    /// Check if user can read from a module
    /// </summary>
    public static bool CanRead(this ClaimsPrincipal user, SystemModule module)
    {
        return user.HasPermission(module, PermissionLevel.ReadOnly);
    }

    /// <summary>
    /// Check if user can write to a module
    /// </summary>
    public static bool CanWrite(this ClaimsPrincipal user, SystemModule module)
    {
        return user.HasPermission(module, PermissionLevel.ReadWrite);
    }

    /// <summary>
    /// Check if user can approve in a module
    /// </summary>
    public static bool CanApprove(this ClaimsPrincipal user, SystemModule module)
    {
        return user.HasPermission(module, PermissionLevel.Approver);
    }
}
