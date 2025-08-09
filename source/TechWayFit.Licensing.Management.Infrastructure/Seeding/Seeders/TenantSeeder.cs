using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Management.Core.Helpers;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.Tenant;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Infrastructure.Seeding;

namespace TechWayFit.Licensing.Management.Infrastructure.Seeding.Seeders;

/// <summary>
/// Seeder for default system tenant
/// </summary>
public class TenantSeeder : BaseDataSeeder
{
    public TenantSeeder(IUnitOfWork unitOfWork, ILogger<BaseDataSeeder> logger) 
        : base(unitOfWork, logger)
    {
    }

    public override string SeederName => "Tenant";
    public override int Order => 1; // Run first as other entities depend on tenant

    protected override async Task<int> ExecuteSeedingAsync(CancellationToken cancellationToken = default)
    {
        var recordsCreated = 0;

        // Check if default tenant already exists
        var existingTenants = await _unitOfWork.Tenants.GetAllAsync(cancellationToken);
        if (existingTenants.Any())
        {
            _logger.LogInformation("Tenants already exist, skipping seeding");
            return 0;
        }

        // Create default system tenant
        var defaultTenant = new Tenant
        {
           TenantId = IdConstants.SystemTenantId,
           TenantName = "Default System Tenant",
           TenantCode = "SYSTEM",
           Description = "Default tenant for the system",
           Website = "https://www.techwayfit.com"
        };

        try
        {
            await _unitOfWork.Tenants.AddAsync(defaultTenant, cancellationToken);
            recordsCreated++;
            _logger.LogDebug("Created default system tenant: {TenantName}", defaultTenant.TenantName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create default system tenant");
            throw;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created {Count} default tenant", recordsCreated);
        return recordsCreated;
    }

    protected override Dictionary<string, object> GetMetadata()
    {
        var metadata = base.GetMetadata();
        metadata["TenantCount"] = 1;
        metadata["TenantType"] = "System";
        metadata["TenantCode"] = "SYSTEM";
        metadata["Features"] = "UserManagement,ProductManagement,LicenseManagement,Reporting,Audit";
        return metadata;
    }
}
