using TechWayFit.Licensing.Infrastructure.Contracts.Repositories.Common;
using TechWayFit.Licensing.Infrastructure.Models.Entities.User;

namespace TechWayFit.Licensing.Infrastructure.Contracts.Repositories.User;

/// <summary>
/// Repository interface for UserRole entities
/// </summary>
public interface IUserRoleRepository : IBaseRepository<UserRoleEntity>
{
    /// <summary>
    /// Gets a role by name
    /// </summary>
    Task<UserRoleEntity?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active roles
    /// </summary>
    Task<IEnumerable<UserRoleEntity>> GetActiveRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if role name exists
    /// </summary>
    Task<bool> RoleNameExistsAsync(string roleName, Guid? excludeRoleId = null, CancellationToken cancellationToken = default);
}
