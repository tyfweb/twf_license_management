using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Infrastructure.Contracts.Repositories.User;
using TechWayFit.Licensing.Infrastructure.Data.Context;
using TechWayFit.Licensing.Infrastructure.Data.Repositories;
using TechWayFit.Licensing.Infrastructure.Models.Entities.User;

namespace TechWayFit.Licensing.Management.Infrastructure.Implementations.Repositories.User;

/// <summary>
/// User role repository implementation
/// </summary>
public class UserRoleRepository : BaseRepository<UserRoleEntity>, IUserRoleRepository
{
    public UserRoleRepository(LicensingDbContext context) : base(context)
    {
    }

    public async Task<UserRoleEntity?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r => r.RoleName == roleName && r.IsActive, cancellationToken);
    }

    public async Task<IEnumerable<UserRoleEntity>> GetActiveRolesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.IsActive)
            .OrderBy(r => r.RoleName)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> RoleNameExistsAsync(string roleName, Guid? excludeRoleId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(r => r.RoleName == roleName && r.IsActive);
        
        if (excludeRoleId.HasValue)
        {
            query = query.Where(r => r.RoleId != excludeRoleId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    protected override IQueryable<UserRoleEntity> SearchQuery(IQueryable<UserRoleEntity> query, string searchQuery)
    {
        return base.SearchQuery(query, searchQuery)
                   .Where(r => r.RoleName.Contains(searchQuery) ||
                              (r.RoleDescription != null && r.RoleDescription.Contains(searchQuery)));
    }
}
