using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.User;
using TechWayFit.Licensing.Management.Infrastructure.Data.Context;
using TechWayFit.Licensing.Management.Infrastructure.Data.Repositories;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.User;

namespace TechWayFit.Licensing.Management.Infrastructure.Implementations.Repositories.User;

/// <summary>
/// User profile repository implementation
/// </summary>
public class UserProfileRepository : BaseRepository<UserProfileEntity>, IUserProfileRepository
{
    public UserProfileRepository(LicensingDbContext context) : base(context)
    {
    }

    public async Task<UserProfileEntity?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserName == username && u.IsActive && !u.IsDeleted, cancellationToken);
    }

    public async Task<UserProfileEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive && !u.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<UserProfileEntity>> GetUsersWithRolesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => u.IsActive && !u.IsDeleted)
            .OrderBy(u => u.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserProfileEntity>> GetUsersByRoleAsync(string roleName, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => u.IsActive && !u.IsDeleted &&
                       u.UserRoles.Any(ur => ur.Role.RoleName == roleName && ur.IsActive))
            .OrderBy(u => u.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<UserProfileEntity> Users, int TotalCount)> SearchUsersAsync(
        string? searchTerm = null,
        string? departmentFilter = null,
        string? roleFilter = null,
        bool? isLockedFilter = null,
        bool? isAdminFilter = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => u.IsActive && !u.IsDeleted)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(u =>
                u.FullName.Contains(searchTerm) ||
                u.Email.Contains(searchTerm) ||
                u.UserName.Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(departmentFilter))
        {
            query = query.Where(u => u.Department == departmentFilter);
        }

        if (!string.IsNullOrWhiteSpace(roleFilter))
        {
            query = query.Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == roleFilter && ur.IsActive));
        }

        if (isLockedFilter.HasValue)
        {
            query = query.Where(u => u.IsLocked == isLockedFilter.Value);
        }

        if (isAdminFilter.HasValue)
        {
            query = query.Where(u => u.IsAdmin == isAdminFilter.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderBy(u => u.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (users, totalCount);
    }

    public async Task<(int TotalUsers, int ActiveUsers, int LockedUsers, int AdminUsers)> GetUserStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var allUsers = _dbSet.Where(u => u.IsActive && !u.IsDeleted);

        var totalUsers = await allUsers.CountAsync(cancellationToken);
        var activeUsers = await allUsers.Where(u => !u.IsLocked).CountAsync(cancellationToken);
        var lockedUsers = await allUsers.Where(u => u.IsLocked).CountAsync(cancellationToken);
        var adminUsers = await allUsers.Where(u => u.IsAdmin).CountAsync(cancellationToken);

        return (totalUsers, activeUsers, lockedUsers, adminUsers);
    }

    public async Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(u => u.UserName == username && u.IsActive && !u.IsDeleted);

        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.UserId != excludeUserId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(u => u.Email == email && u.IsActive && !u.IsDeleted);

        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.UserId != excludeUserId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbSet.FindAsync(new object[] { userId }, cancellationToken);
        if (user != null)
        {
            user.LastLoginDate = DateTime.UtcNow;
            user.FailedLoginAttempts = 0; // Reset failed attempts on successful login
            user.UpdatedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateFailedLoginAttemptsAsync(Guid userId, int attempts, CancellationToken cancellationToken = default)
    {
        var user = await _dbSet.FindAsync(new object[] { userId }, cancellationToken);
        if (user != null)
        {
            user.FailedLoginAttempts = attempts;
            user.UpdatedOn = DateTime.UtcNow;

            // Auto-lock after 5 failed attempts
            if (attempts >= 5)
            {
                user.IsLocked = true;
                user.LockedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task LockUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbSet.FindAsync(new object[] { userId }, cancellationToken);
        if (user != null)
        {
            user.IsLocked = true;
            user.LockedDate = DateTime.UtcNow;
            user.UpdatedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UnlockUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbSet.FindAsync(new object[] { userId }, cancellationToken);
        if (user != null)
        {
            user.IsLocked = false;
            user.LockedDate = null;
            user.FailedLoginAttempts = 0;
            user.UpdatedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    protected override IQueryable<UserProfileEntity> SearchQuery(IQueryable<UserProfileEntity> query, string searchQuery)
    {
        return base.SearchQuery(query, searchQuery)
                   .Where(u => u.FullName.Contains(searchQuery) ||
                              u.Email.Contains(searchQuery) ||
                              u.UserName.Contains(searchQuery) ||
                              (u.Department != null && u.Department.Contains(searchQuery)));
    }

    public async Task<bool> ValidatePasswordAsync(string username, string password)
    {
        var user = await _dbSet.FirstOrDefaultAsync(u => u.UserName == username && u.IsActive && !u.IsDeleted);
        if (user == null)
        {
            return false;
        }

        var salt = user.PasswordSalt;
        var hashedPassword = HashPasswordWithSalt(password, salt);  
        
        return user.PasswordHash == hashedPassword;
    }
    
     private static string HashPasswordWithSalt(string password, string salt)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password + salt);
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(passwordBytes);
        return Convert.ToBase64String(hashBytes);
    }
 
}
