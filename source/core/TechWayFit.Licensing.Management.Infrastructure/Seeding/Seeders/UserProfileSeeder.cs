using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Management.Core.Helpers;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.User;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Infrastructure.Seeding;

namespace TechWayFit.Licensing.Management.Infrastructure.Seeding.Seeders;

/// <summary>
/// Seeder for default user profiles
/// </summary>
public class UserProfileSeeder : BaseDataSeeder
{
    public UserProfileSeeder(IUnitOfWork unitOfWork, ILogger<BaseDataSeeder> logger) 
        : base(unitOfWork, logger)
    {
    }

    public override string SeederName => "UserProfile";
    public override int Order => 4; // Run after settings

    protected override async Task<int> ExecuteSeedingAsync(CancellationToken cancellationToken = default)
    {
        var recordsCreated = 0;

        // Check if any user profiles already exist
        var existingUsers = await _unitOfWork.Users.GetAllAsync(cancellationToken);
        if (existingUsers.Any())
        {
            _logger.LogInformation("User profiles already exist, skipping seeding");
            return 0;
        }


        // Default system users
        var defaultUsers = new[]
        {
            new UserProfile
            {
                UserId = IdConstants.AdminUserId,
                TenantId = IdConstants.SystemTenantId,
                UserName = "admin",
                Email = "admin@system.local",
                FullName = "System Administrator",
                IsAdmin = true,
                Password = "Admin@123",
                
            },
            new UserProfile
            {
                UserId = IdConstants.ManagerUserId,
                TenantId = IdConstants.SystemTenantId,
                UserName = "demo.manager",
                Email = "manager@demo.local",
                FullName = "Demo Manager",
                IsAdmin = false,
                Password = "Manager@123",
            }
        };

        foreach (var user in defaultUsers)
        {
            try
            {
                await _unitOfWork.Users.AddAsync(user, cancellationToken);
                recordsCreated++;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogDebug("Created user profile: {Username}", user.UserName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create user profile: {Username}", user.UserName);
            }
        }
        await _unitOfWork.UserRoleMappings.AddAsync(new UserRoleMapping
        {
            RoleId = IdConstants.AdminRoleId,
            UserId = IdConstants.AdminUserId,
            CreatedBy = "System",
            CreatedOn = DateTime.UtcNow,
            TenantId = IdConstants.SystemTenantId
        }, cancellationToken);
        await _unitOfWork.UserRoleMappings.AddAsync(new UserRoleMapping
        {
            RoleId = IdConstants.ManagerRoleId,
            UserId = IdConstants.ManagerUserId,
            CreatedBy = "System",
            CreatedOn = DateTime.UtcNow,
            TenantId = IdConstants.SystemTenantId
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created {Count} default user profiles", recordsCreated);
        return recordsCreated;
    }

    protected override Dictionary<string, object> GetMetadata()
    {
        var metadata = base.GetMetadata();
        metadata["UserCount"] = 2;
        metadata["SystemUsers"] = 1;
        metadata["DemoUsers"] = 1;
        metadata["Usernames"] = "admin,demo.manager";
        return metadata;
    }
}
