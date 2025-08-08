using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.User;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories;

using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.User;
using TechWayFit.Licensing.Management.Core.Models.User;
using TechWayFit.Licensing.Management.Core.Contracts;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.User;

/// <summary>
/// User role repository implementation
/// </summary>
public class PostgreSqlUserRoleRepository : BaseRepository<UserRole,UserRoleEntity>, IUserRoleRepository
{
    public PostgreSqlUserRoleRepository(PostgreSqlPostgreSqlLicensingDbContext context,IUserContext userContext) : base(context,userContext)
    {
    }

    public async Task<UserRole?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet
            .FirstOrDefaultAsync(r => r.RoleName == roleName && r.IsActive, cancellationToken);
        return result?.Map();
    }

    public async Task<IEnumerable<UserRole>> GetActiveRolesAsync(CancellationToken cancellationToken = default)
    {
        var result = await _dbSet
            .Where(r => r.IsActive)
            .OrderBy(r => r.RoleName)
            .ToListAsync(cancellationToken);
        return result.Select(r => r.Map());
    }

    public async Task<bool> RoleNameExistsAsync(string roleName, Guid? excludeRoleId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(r => r.RoleName == roleName && r.IsActive);

        if (excludeRoleId.HasValue)
        {
            query = query.Where(r => r.Id != excludeRoleId.Value);
        }

        var result = await query.AnyAsync(cancellationToken);
        return result;
    }

    protected override IQueryable<UserRoleEntity> SearchQuery(IQueryable<UserRoleEntity> query, string searchQuery)
    {
        return base.SearchQuery(query, searchQuery)
                   .Where(r => r.RoleName.Contains(searchQuery) ||
                              (r.RoleDescription != null && r.RoleDescription.Contains(searchQuery)));
    }

    public async Task<IEnumerable<UserRole>> GetActiveRolesByIdsAsync(List<Guid> roleIds)
    {
        var result = await _dbSet
            .Where(r => roleIds.Contains(r.Id) && r.IsActive)
            .OrderBy(r => r.RoleName)
            .ToListAsync();
        return result.Select(r => r.Map());
    }
}
