using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.User;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Core.Contracts;
using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Services.User;

/// <summary>
/// Service implementation for managing role permissions
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public PermissionService(
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }    public async Task<Dictionary<SystemModule, PermissionLevel>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.RolePermissions.GetUserEffectivePermissionsAsync(userId, cancellationToken);
    }

    public async Task<bool> UserHasPermissionAsync(Guid userId, SystemModule module, PermissionLevel requiredLevel, CancellationToken cancellationToken = default)
    {
        var userPermissions = await GetUserPermissionsAsync(userId, cancellationToken);
        
        if (!userPermissions.TryGetValue(module, out var currentLevel))
        {
            return false;
        }

        return IsPermissionSufficient(currentLevel, requiredLevel);
    }    public async Task<IEnumerable<RolePermission>> UpdateRolePermissionsAsync(Guid roleId, Dictionary<SystemModule, PermissionLevel> permissions, CancellationToken cancellationToken = default)
    {
        var currentUser = _userContext.UserName ?? "System";
        
        // Validate permissions before updating
        var validationResult = await ValidateRolePermissionsAsync(roleId, permissions, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException($"Invalid permissions: {string.Join(", ", validationResult.Errors)}");
        }

        return await _unitOfWork.RolePermissions.UpdateRolePermissionsAsync(roleId, permissions, currentUser, cancellationToken);
    }

    public async Task<Dictionary<SystemModule, PermissionLevel>> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var permissions = await _unitOfWork.RolePermissions.GetByRoleIdAsync(roleId, cancellationToken);
          var result = new Dictionary<SystemModule, PermissionLevel>();
        foreach (var permission in permissions)
        {
            result[permission.Module] = permission.Level;
        }

        return result;
    }

    public async Task<Dictionary<Guid, Dictionary<SystemModule, PermissionLevel>>> GetRolePermissionsBatchAsync(IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.RolePermissions.GetPermissionsForRolesAsync(roleIds, cancellationToken);
    }

    public bool IsPermissionSufficient(PermissionLevel currentLevel, PermissionLevel requiredLevel)
    {
        // Permission hierarchy: None < ReadOnly < ReadWrite < Approver
        return (int)currentLevel >= (int)requiredLevel;
    }

    public PermissionLevel GetHighestPermissionLevel(IEnumerable<PermissionLevel> levels)
    {
        if (!levels.Any())
            return PermissionLevel.None;

        return levels.Max();
    }

    public async Task<ValidationResult> ValidateRolePermissionsAsync(Guid roleId, Dictionary<SystemModule, PermissionLevel> permissions, CancellationToken cancellationToken = default)
    {
        var result = new ValidationResult { IsValid = true };

        // Validate each permission
        foreach (var permission in permissions)
        {
            // Check if module is valid
            if (!Enum.IsDefined(typeof(SystemModule), permission.Key))
            {
                result.Errors.Add($"Invalid system module: {permission.Key}");
                result.IsValid = false;
            }

            // Check if permission level is valid
            if (!Enum.IsDefined(typeof(PermissionLevel), permission.Value))
            {
                result.Errors.Add($"Invalid permission level: {permission.Value} for module {permission.Key}");
                result.IsValid = false;
            }

            // Business rules validation
            await ValidateBusinessRules(roleId, permission.Key, permission.Value, result, cancellationToken);
        }

        return result;
    }

    private async Task ValidateBusinessRules(Guid roleId, SystemModule module, PermissionLevel level, ValidationResult result, CancellationToken cancellationToken)
    {
        // Add business rule validations here
        
        // Example: System module might require special permissions
        if (module == SystemModule.System && level > PermissionLevel.ReadOnly)
        {
            // Check if role is admin role or has special privileges
            // This is a placeholder for business logic
            result.Errors.Add($"Granting {level} access to {module} module requires administrative privileges");
        }

        // Example: Audit module should typically be read-only
        if (module == SystemModule.Audit && level > PermissionLevel.ReadOnly)
        {
            result.Errors.Add($"Granting {level} access to {module} module should be carefully considered");
        }

        // Add more business rules as needed
        await Task.CompletedTask; // Placeholder for async business rules
    }

    /// <summary>
    /// Maintenance method to cleanup duplicate permissions that might cause constraint violations
    /// Call this if you encounter unique constraint errors when updating role permissions
    /// </summary>
    public async Task CleanupDuplicatePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.RolePermissions.CleanupDuplicatePermissionsAsync(roleId, cancellationToken);
    }
}
