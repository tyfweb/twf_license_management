using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.User;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.User;
using TechWayFit.Licensing.Management.Core.Models.User;
using TechWayFit.Licensing.Management.Core.Contracts;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories.User;

/// <summary>
/// Role permission repository implementation
/// </summary>
public class EfCoreRolePermissionRepository : BaseRepository<RolePermission, RolePermissionEntity>, IRolePermissionRepository
{
    public EfCoreRolePermissionRepository(EfCoreLicensingDbContext context, IUserContext userContext) : base(context, userContext)
    {
    }

    public async Task<IEnumerable<RolePermission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet
            .Include(rp => rp.Role)
            .Where(rp => rp.RoleId == roleId && rp.IsActive)
            .OrderBy(rp => rp.SystemModule)
            .ToListAsync(cancellationToken);
        return result.Select(rp => rp.Map());
    }

    public async Task<RolePermission?> GetByRoleAndModuleAsync(Guid roleId, SystemModule module, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet
            .Include(rp => rp.Role)
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && 
                                     rp.SystemModule == (int)module && 
                                     rp.IsActive, cancellationToken);
        return result?.Map();
    }

    public async Task<IEnumerable<RolePermission>> UpdateRolePermissionsAsync(Guid roleId, Dictionary<SystemModule, PermissionLevel> permissions, string updatedBy, CancellationToken cancellationToken = default)
    {
        // Get ALL existing permissions for the role (both active and inactive) to avoid unique constraint violations
        var existingPermissions = await _dbSet
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(cancellationToken);

        var updatedPermissions = new List<RolePermissionEntity>();

        foreach (var permission in permissions)
        {
            // Find existing permission (active OR inactive) for this module
            var existing = existingPermissions.FirstOrDefault(ep => ep.SystemModule == (int)permission.Key);
            
            if (existing != null)
            {
                // Update existing permission (whether active or inactive)
                if (permission.Value == PermissionLevel.None)
                {
                    // Remove permission by setting IsActive to false
                    existing.IsActive = false;
                    existing.UpdatedBy = updatedBy;
                    existing.UpdatedOn = DateTime.UtcNow;
                }
                else
                {
                    // Update permission level and ensure it's active
                    existing.PermissionLevel = (int)permission.Value;
                    existing.IsActive = true; // Reactivate if it was inactive
                    existing.UpdatedBy = updatedBy;
                    existing.UpdatedOn = DateTime.UtcNow;
                    updatedPermissions.Add(existing);
                }
            }
            else if (permission.Value != PermissionLevel.None)
            {
                // Create new permission only if no existing record found
                var newPermission = new RolePermissionEntity
                {
                    Id = Guid.NewGuid(),
                    TenantId = _userContext.TenantId ?? Guid.Empty,
                    RoleId = roleId,
                    SystemModule = (int)permission.Key,
                    PermissionLevel = (int)permission.Value,
                    IsActive = true,
                    CreatedBy = updatedBy,
                    CreatedOn = DateTime.UtcNow
                };
                
                _dbSet.Add(newPermission);
                updatedPermissions.Add(newPermission);
            }
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Log the specific error and rethrow with more context
            throw new InvalidOperationException($"Failed to update role permissions for RoleId: {roleId}. " +
                $"This might be due to a unique constraint violation. Error: {ex.Message}", ex);
        }
        
        return updatedPermissions.Select(up => up.Map());
    }

    public async Task RemoveAllRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var permissions = await _dbSet
            .Where(rp => rp.RoleId == roleId && rp.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var permission in permissions)
        {
            permission.IsActive = false;
            permission.UpdatedOn = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Cleanup method to remove duplicate inactive permissions that might cause unique constraint issues
    /// This is a maintenance method to fix any data inconsistencies
    /// </summary>
    public async Task CleanupDuplicatePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        // Find all permissions for the role grouped by module
        var allPermissions = await _dbSet
            .Where(rp => rp.RoleId == roleId)
            .OrderBy(rp => rp.SystemModule)
            .ThenByDescending(rp => rp.IsActive) // Active records first
            .ThenByDescending(rp => rp.UpdatedOn ?? rp.CreatedOn) // Most recent first
            .ToListAsync(cancellationToken);

        var permissionsToDelete = new List<RolePermissionEntity>();

        // Group by module and keep only the most recent record for each module
        var groupedPermissions = allPermissions.GroupBy(p => p.SystemModule);
        
        foreach (var group in groupedPermissions)
        {
            var modulePermissions = group.ToList();
            if (modulePermissions.Count > 1)
            {
                // Keep the first one (most recent active, or most recent if all inactive)
                // Delete the rest
                permissionsToDelete.AddRange(modulePermissions.Skip(1));
            }
        }

        if (permissionsToDelete.Any())
        {
            _dbSet.RemoveRange(permissionsToDelete);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> HasPermissionAsync(Guid roleId, SystemModule module, PermissionLevel requiredLevel, CancellationToken cancellationToken = default)
    {
        var permission = await _dbSet
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && 
                                     rp.SystemModule == (int)module && 
                                     rp.IsActive, cancellationToken);

        if (permission == null) return false;

        var currentLevel = (PermissionLevel)permission.PermissionLevel;
        return currentLevel >= requiredLevel;
    }

    public async Task<Dictionary<SystemModule, PermissionLevel>> GetUserEffectivePermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var permissions = await (from urm in _context.UserRoleMappings
                                join rp in _dbSet on urm.RoleId equals rp.RoleId
                                where urm.UserId == userId && urm.IsActive && rp.IsActive
                                select new { rp.SystemModule, rp.PermissionLevel })
                                .ToListAsync(cancellationToken);

        var effectivePermissions = new Dictionary<SystemModule, PermissionLevel>();

        // Group by module and take the highest permission level
        var groupedPermissions = permissions
            .GroupBy(p => (SystemModule)p.SystemModule)
            .Select(g => new
            {
                Module = g.Key,
                MaxLevel = (PermissionLevel)g.Max(p => p.PermissionLevel)
            });

        foreach (var permission in groupedPermissions)
        {
            effectivePermissions[permission.Module] = permission.MaxLevel;
        }

        return effectivePermissions;
    }

    public async Task<Dictionary<Guid, Dictionary<SystemModule, PermissionLevel>>> GetPermissionsForRolesAsync(IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        var permissions = await _dbSet
            .Where(rp => roleIds.Contains(rp.RoleId) && rp.IsActive)
            .OrderBy(rp => rp.RoleId)
            .ThenBy(rp => rp.SystemModule)
            .ToListAsync(cancellationToken);

        var result = new Dictionary<Guid, Dictionary<SystemModule, PermissionLevel>>();

        foreach (var roleId in roleIds)
        {
            result[roleId] = new Dictionary<SystemModule, PermissionLevel>();
        }

        foreach (var permission in permissions)
        {
            var module = (SystemModule)permission.SystemModule;
            var level = (PermissionLevel)permission.PermissionLevel;
            result[permission.RoleId][module] = level;
        }

        return result;
    }    protected override IQueryable<RolePermissionEntity> SearchQuery(IQueryable<RolePermissionEntity> query, string searchQuery)
    {
        return base.SearchQuery(query, searchQuery)
                   .Where(rp => rp.Role.RoleName.Contains(searchQuery));
    }

    protected override IQueryable<RolePermissionEntity> ApplyIncludes(IQueryable<RolePermissionEntity> query)
    {
        return query.Include(rp => rp.Role);
    }
}
