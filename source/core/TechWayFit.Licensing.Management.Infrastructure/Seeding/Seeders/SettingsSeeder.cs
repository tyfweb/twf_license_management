using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Management.Core.Helpers;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.Settings;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Infrastructure.Seeding;

namespace TechWayFit.Licensing.Management.Infrastructure.Seeding.Seeders;

/// <summary>
/// Seeder for default system settings
/// </summary>
public class SettingsSeeder : BaseDataSeeder
{
    public SettingsSeeder(IUnitOfWork unitOfWork, ILogger<BaseDataSeeder> logger) 
        : base(unitOfWork, logger)
    {
    }

    public override string SeederName => "Settings";
    public override int Order => 3; // Run after user roles

    protected override async Task<int> ExecuteSeedingAsync(CancellationToken cancellationToken = default)
    {
        var recordsCreated = 0;

        // Check if any settings already exist
        var existingSettings = await _unitOfWork.Settings.GetAllAsync(cancellationToken);
        if (existingSettings.Any())
        {
            _logger.LogInformation("System settings already exist, skipping seeding");
            return 0;
        }

        // Default system settings
        var defaultSettings = new[]
        {
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Key = "System.ApplicationName",
                Value = "TechWayFit Licensing Management",
                Description = "The display name of the application",
                Category = "System",
                DataType = "string",
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo
                {
                    CreatedBy = "System",
                    CreatedOn = DateTime.UtcNow,
                    UpdatedBy = "System",
                    UpdatedOn = DateTime.UtcNow
                }
            },
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Key = "System.Version",
                Value = "1.0.0",
                Description = "Current system version",
                Category = "System",
                DataType = "string",
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo
                {
                    CreatedBy = "System",
                    CreatedOn = DateTime.UtcNow,
                    UpdatedBy = "System",
                    UpdatedOn = DateTime.UtcNow
                }
            },
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Key = "License.DefaultExpiryDays",
                Value = "365",
                Description = "Default number of days for license expiry",
                Category = "License",
                DataType = "integer",
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo
                {
                    CreatedBy = "System",
                    CreatedOn = DateTime.UtcNow,
                    UpdatedBy = "System",
                    UpdatedOn = DateTime.UtcNow
                }
            }
        };

        foreach (var setting in defaultSettings)
        {
            try
            {
                await _unitOfWork.Settings.AddAsync(setting, cancellationToken);
                recordsCreated++;
                _logger.LogDebug("Created setting: {SettingKey}", setting.Key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create setting: {SettingKey}", setting.Key);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created {Count} default system settings", recordsCreated);
        return recordsCreated;
    }

    protected override Dictionary<string, object> GetMetadata()
    {
        var metadata = base.GetMetadata();
        metadata["SettingCount"] = 10;
        metadata["Categories"] = "System,License,Security,Email,Audit";
        metadata["SystemSettings"] = 2;
        metadata["ConfigurableSettings"] = 8;
        return metadata;
    }
}
