using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.User;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories;

using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.User;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.User;

/// <summary>
/// User role repository implementation
/// </summary>
public class PostgreSqlUserRoleRepository : PostgreSqlBaseRepository<UserRoleEntity>, IUserRoleRepository
{
    public PostgreSqlUserRoleRepository(PostgreSqlPostgreSqlLicensingDbContext context) : base(context)
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
            query = query.Where(r => r.Id != excludeRoleId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    protected override IQueryable<UserRoleEntity> SearchQuery(IQueryable<UserRoleEntity> query, string searchQuery)
    {
        return base.SearchQuery(query, searchQuery)
                   .Where(r => r.RoleName.Contains(searchQuery) ||
                              (r.RoleDescription != null && r.RoleDescription.Contains(searchQuery)));
    }

    public async Task<IEnumerable<UserRoleEntity>> GetActiveRolesByIdsAsync(List<Guid> roleIds)
    {
        return await _dbSet
            .Where(r => roleIds.Contains(r.Id) && r.IsActive)
            .OrderBy(r => r.RoleName)
            .ToListAsync();
    }
}
