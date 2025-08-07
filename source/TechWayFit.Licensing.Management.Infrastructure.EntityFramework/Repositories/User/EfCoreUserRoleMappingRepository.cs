using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.User;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories;

using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.User;
using TechWayFit.Licensing.Management.Core.Models.User;
using TechWayFit.Licensing.Management.Core.Contracts;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories.User;

/// <summary>
/// User role mapping repository implementation
/// </summary>
public class EfCoreUserRoleMappingRepository : BaseRepository<UserRoleMapping,UserRoleMappingEntity>, IUserRoleMappingRepository
{
    public EfCoreUserRoleMappingRepository(EfCoreLicensingDbContext context,IUserContext userContext) : base(context,userContext)
    {
    }

    public async Task<IEnumerable<UserRoleMapping>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet
            .Include(urm => urm.Role)
            .Include(urm => urm.User)
            .Where(urm => urm.UserId == userId && urm.IsActive)
            .ToListAsync(cancellationToken);
        return result.Select(urm => urm.Map());
    }

    public async Task<IEnumerable<UserRoleMapping>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet
            .Include(urm => urm.Role)
            .Include(urm => urm.User)
            .Where(urm => urm.RoleId == roleId && urm.IsActive)
            .ToListAsync(cancellationToken);
        return result.Select(urm => urm.Map());
    }

    public async Task<UserRoleMapping?> GetByUserAndRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet
            .Include(urm => urm.Role)
            .Include(urm => urm.User)
            .FirstOrDefaultAsync(urm => urm.UserId == userId && urm.RoleId == roleId && urm.IsActive, cancellationToken);
        return result?.Map();
    }

    public async Task<bool> UserHasRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet
            .Include(urm => urm.Role)
            .AnyAsync(urm => urm.UserId == userId &&
                           urm.Role.RoleName == roleName &&
                           urm.IsActive &&
                           urm.Role.IsActive, cancellationToken);
        return result;
    }

    public async Task RemoveAllUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userRoles = await _dbSet
            .Where(urm => urm.UserId == userId && urm.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var userRole in userRoles)
        {
            userRole.IsActive = false;
            userRole.UpdatedOn = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var userRole = await _dbSet
            .FirstOrDefaultAsync(urm => urm.UserId == userId && urm.RoleId == roleId && urm.IsActive, cancellationToken);

        if (userRole != null)
        {
            userRole.IsActive = false;
            userRole.UpdatedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<UserRoleMapping> AssignRoleToUserAsync(Guid userId, Guid roleId, string assignedBy, CancellationToken cancellationToken = default)
    {
        // Check if mapping already exists
        var existingMapping = await _dbSet
            .FirstOrDefaultAsync(urm => urm.UserId == userId && urm.RoleId == roleId, cancellationToken);

        if (existingMapping != null)
        {
            // Reactivate if it was deactivated
            if (!existingMapping.IsActive)
            {
                existingMapping.IsActive = true;
                existingMapping.UpdatedOn = DateTime.UtcNow;
                existingMapping.UpdatedBy = assignedBy;
                await _context.SaveChangesAsync(cancellationToken);
            }
            return existingMapping.Map();
        }

        // Create new mapping
        var newMapping = new UserRoleMappingEntity
        {
            UserId = userId,
            RoleId = roleId,
            AssignedDate = DateTime.UtcNow,
            CreatedBy = assignedBy,
            CreatedOn = DateTime.UtcNow
        };

        _dbSet.Add(newMapping);
        await _context.SaveChangesAsync(cancellationToken);
        return newMapping.Map();
    }

    protected override IQueryable<UserRoleMappingEntity> SearchQuery(IQueryable<UserRoleMappingEntity> query, string searchQuery)
    {
        return base.SearchQuery(query, searchQuery)
                   .Where(urm => urm.User.FullName.Contains(searchQuery) ||
                                urm.User.Email.Contains(searchQuery) ||
                                urm.Role.RoleName.Contains(searchQuery));
    }
}
