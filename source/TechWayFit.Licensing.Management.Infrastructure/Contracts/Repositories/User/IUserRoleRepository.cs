using TechWayFit.Licensing.Management.Core.Models.User;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common; 

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.User;

/// <summary>
/// Repository interface for UserRole entities
/// </summary>
public interface IUserRoleRepository : IDataRepository<UserRole>
{
    /// <summary>
    /// Gets a role by name
    /// </summary>
    Task<UserRole?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active roles
    /// </summary>
    Task<IEnumerable<UserRole>> GetActiveRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if role name exists
    /// </summary>
    Task<bool> RoleNameExistsAsync(string roleName, Guid? excludeRoleId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserRole>> GetActiveRolesByIdsAsync(List<Guid> roleIds);
}
