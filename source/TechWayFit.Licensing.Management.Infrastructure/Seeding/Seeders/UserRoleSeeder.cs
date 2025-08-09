using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Management.Core.Helpers;
using TechWayFit.Licensing.Management.Core.Models.User;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Infrastructure.Seeding;

namespace TechWayFit.Licensing.Management.Infrastructure.Seeding.Seeders;

/// <summary>
/// Seeder for default user roles
/// </summary>
public class UserRoleSeeder : BaseDataSeeder
{
    public UserRoleSeeder(IUnitOfWork unitOfWork, ILogger<BaseDataSeeder> logger) 
        : base(unitOfWork, logger)
    {
    }

    public override string SeederName => "UserRole";
    public override int Order => 2; // Run after tenant

    protected override async Task<int> ExecuteSeedingAsync(CancellationToken cancellationToken = default)
    {
        var recordsCreated = 0;

        // Check if any roles already exist
        var existingRoles = await _unitOfWork.UserRoles.GetAllAsync(cancellationToken);
        if (existingRoles.Any())
        {
            _logger.LogInformation("User roles already exist, skipping seeding");
            return 0;
        }
        Guid adminRoleId = IdConstants.AdminRoleId;
        Guid managerRoleId = IdConstants.ManagerRoleId;
        Guid userRoleId = IdConstants.UserRoleId;

        // Default roles for the licensing system
        var defaultRoles = new[]
        {
            new UserRole
            {
                RoleId = adminRoleId,
                RoleName = "Administrator",
                RoleDescription= "Administrative access to manage users and licenses",
                IsAdmin = true,
                IsActive = true,
                CreatedBy = "System",
                CreatedOn = DateTime.UtcNow,
                TenantId = IdConstants.SystemTenantId
            },
            new UserRole
            {
                RoleId = managerRoleId,
                RoleName = "Manager",
                RoleDescription = "Can create, modify, and manage licenses",
                IsAdmin = false,
                IsActive = true,
                CreatedBy = "System",
                CreatedOn = DateTime.UtcNow,
                TenantId = IdConstants.SystemTenantId
            },
            new UserRole
            {
                RoleId = userRoleId,
                RoleName = "User",
                RoleDescription = "Can manage their own profile and licenses",
                IsAdmin = false,
                IsActive = true,
                CreatedBy = "System",
                CreatedOn = DateTime.UtcNow,
                TenantId = IdConstants.SystemTenantId
            }
        };

        foreach (var role in defaultRoles)
        {
            try
            {
                await _unitOfWork.UserRoles.AddAsync(role, cancellationToken);
                recordsCreated++;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogDebug("Created role: {RoleName}", role.RoleName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create role: {RoleName}", role.RoleName);
            }
        }
        

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created {Count} default user roles", recordsCreated);
        return recordsCreated;
    }

    protected override Dictionary<string, object> GetMetadata()
    {
        var metadata = base.GetMetadata();
        metadata["RoleCount"] = 7;
        metadata["SystemRoles"] = "SuperAdmin,Administrator,LicenseManager,ProductManager,ConsumerManager,Viewer,Auditor";
        return metadata;
    }
}
