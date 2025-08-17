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
        // var existingSettings = await _unitOfWork.Settings.GetAllAsync(cancellationToken);
        // if (existingSettings.Any())
        // {
        //     _logger.LogInformation("System settings already exist, skipping seeding");
        //     return 0;
        // }

        // Default system settings based on SQL structure
        var defaultSettings = new[]
        {
            // System Configuration
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "System",
                Key = "ApplicationName",
                Value = "TechWayFit License Management",
                DefaultValue = "TechWayFit License Management",
                DataType = "string",
                DisplayName = "Application Name",
                Description = "The name of the application displayed in the UI",
                GroupName = "Branding",
                SortOrder = 1,
                IsRequired = true,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "System",
                Key = "ApplicationVersion",
                Value = "1.0.0",
                DefaultValue = "1.0.0",
                DataType = "string",
                DisplayName = "Application Version",
                Description = "Current version of the application",
                GroupName = "Branding",
                SortOrder = 2,
                IsRequired = true,
                IsReadOnly = true,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "System",
                Key = "CompanyName",
                Value = "TechWayFit Solutions",
                DefaultValue = "TechWayFit Solutions",
                DataType = "string",
                DisplayName = "Company Name",
                Description = "Name of the company owning this system",
                GroupName = "Branding",
                SortOrder = 3,
                IsRequired = true,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "System",
                Key = "CompanyLogo",
                Value = "/images/logo.png",
                DefaultValue = "/images/logo.png",
                DataType = "string",
                DisplayName = "Company Logo URL",
                Description = "URL path to the company logo image",
                GroupName = "Branding",
                SortOrder = 4,
                IsRequired = false,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "System",
                Key = "FaviconUrl",
                Value = "/favicon.ico",
                DefaultValue = "/favicon.ico",
                DataType = "string",
                DisplayName = "Favicon URL",
                Description = "URL path to the favicon",
                GroupName = "Branding",
                SortOrder = 5,
                IsRequired = false,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },

            // Email Configuration
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "Email",
                Key = "SmtpServer",
                Value = "smtp.gmail.com",
                DefaultValue = "smtp.gmail.com",
                DataType = "string",
                DisplayName = "SMTP Server",
                Description = "SMTP server hostname for sending emails",
                GroupName = "Email Server",
                SortOrder = 1,
                IsRequired = true,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "Email",
                Key = "SmtpPort",
                Value = "587",
                DefaultValue = "587",
                DataType = "int",
                DisplayName = "SMTP Port",
                Description = "SMTP server port number",
                GroupName = "Email Server",
                SortOrder = 2,
                IsRequired = true,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "Email",
                Key = "SmtpUsername",
                Value = "",
                DefaultValue = "",
                DataType = "string",
                DisplayName = "SMTP Username",
                Description = "Username for SMTP authentication",
                GroupName = "Email Server",
                SortOrder = 3,
                IsRequired = true,
                IsReadOnly = false,
                IsSensitive = true,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "Email",
                Key = "SmtpPassword",
                Value = "",
                DefaultValue = "",
                DataType = "string",
                DisplayName = "SMTP Password",
                Description = "Password for SMTP authentication",
                GroupName = "Email Server",
                SortOrder = 4,
                IsRequired = true,
                IsReadOnly = false,
                IsSensitive = true,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "Email",
                Key = "FromAddress",
                Value = "noreply@techwayfitsolutions.com",
                DefaultValue = "noreply@techwayfitsolutions.com",
                DataType = "string",
                DisplayName = "From Email Address",
                Description = "Email address used as sender for outgoing emails",
                GroupName = "Email Server",
                SortOrder = 5,
                IsRequired = true,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "Email",
                Key = "FromName",
                Value = "TechWayFit License Management",
                DefaultValue = "TechWayFit License Management",
                DataType = "string",
                DisplayName = "From Display Name",
                Description = "Display name used as sender for outgoing emails",
                GroupName = "Email Server",
                SortOrder = 6,
                IsRequired = true,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "Email",
                Key = "EnableSsl",
                Value = "true",
                DefaultValue = "true",
                DataType = "bool",
                DisplayName = "Enable SSL",
                Description = "Whether to use SSL/TLS for SMTP connection",
                GroupName = "Email Server",
                SortOrder = 7,
                IsRequired = true,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },

            // Security Settings
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "Security",
                Key = "SessionTimeoutMinutes",
                Value = "30",
                DefaultValue = "30",
                DataType = "int",
                DisplayName = "User Session Timeout (Minutes)",
                Description = "User session timeout in minutes",
                GroupName = "Authentication",
                SortOrder = 1,
                IsRequired = true,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "Security",
                Key = "PasswordMinLength",
                Value = "8",
                DefaultValue = "8",
                DataType = "int",
                DisplayName = "Minimum Password Length",
                Description = "Minimum required password length",
                GroupName = "Password Policy",
                SortOrder = 2,
                IsRequired = true,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "Security",
                Key = "RequireUppercase",
                Value = "true",
                DefaultValue = "true",
                DataType = "bool",
                DisplayName = "Require Uppercase Letters",
                Description = "Whether passwords must contain uppercase letters",
                GroupName = "Password Policy",
                SortOrder = 3,
                IsRequired = true,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "Security",
                Key = "RequireLowercase",
                Value = "true",
                DefaultValue = "true",
                DataType = "bool",
                DisplayName = "Require Lowercase Letters",
                Description = "Whether passwords must contain lowercase letters",
                GroupName = "Password Policy",
                SortOrder = 4,
                IsRequired = true,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "Security",
                Key = "RequireNumbers",
                Value = "true",
                DefaultValue = "true",
                DataType = "bool",
                DisplayName = "Require Numbers",
                Description = "Whether passwords must contain numeric digits",
                GroupName = "Password Policy",
                SortOrder = 5,
                IsRequired = true,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "Security",
                Key = "RequireSymbols",
                Value = "true",
                DefaultValue = "true",
                DataType = "bool",
                DisplayName = "Require Special Characters",
                Description = "Whether passwords must contain special characters",
                GroupName = "Password Policy",
                SortOrder = 6,
                IsRequired = true,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },

            // License Configuration
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "License",
                Key = "DefaultDurationDays",
                Value = "365",
                DefaultValue = "365",
                DataType = "int",
                DisplayName = "Default License Duration (Days)",
                Description = "Default duration for new licenses in days",
                GroupName = "License Defaults",
                SortOrder = 1,
                IsRequired = true,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "License",
                Key = "GracePeriodDays",
                Value = "30",
                DefaultValue = "30",
                DataType = "int",
                DisplayName = "Grace Period (Days)",
                Description = "Grace period after license expiration in days",
                GroupName = "License Defaults",
                SortOrder = 2,
                IsRequired = true,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "License",
                Key = "AutoRenewEnabled",
                Value = "false",
                DefaultValue = "false",
                DataType = "bool",
                DisplayName = "Enable Auto-Renewal",
                Description = "Whether licenses can be automatically renewed",
                GroupName = "License Features",
                SortOrder = 3,
                IsRequired = false,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "License",
                Key = "MaxActivationsPerLicense",
                Value = "5",
                DefaultValue = "5",
                DataType = "int",
                DisplayName = "Maximum Activations per License",
                Description = "Maximum number of activations allowed per license",
                GroupName = "License Limits",
                SortOrder = 4,
                IsRequired = true,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },

            // UI Settings
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "UI",
                Key = "CurrentTheme",
                Value = "default",
                DefaultValue = "default",
                DataType = "string",
                DisplayName = "Current Theme",
                Description = "Currently active UI theme for the application",
                GroupName = "Appearance",
                SortOrder = 1,
                IsRequired = false,
                IsReadOnly = false,
                IsSensitive = false,
                PossibleValues = "default,dark,blue,green,purple",
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "UI",
                Key = "ItemsPerPage",
                Value = "25",
                DefaultValue = "25",
                DataType = "int",
                DisplayName = "Items per Page",
                Description = "Default number of items to display per page in lists",
                GroupName = "Pagination",
                SortOrder = 2,
                IsRequired = false,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "UI",
                Key = "AvailableThemes",
                Value = "default,dark,blue,green,purple,clean,modern1,modern2",
                DefaultValue = "default,dark,blue,green,purple,clean,modern1,modern2",
                DataType = "string",
                DisplayName = "Available Themes",
                Description = "Comma-separated list of available themes for users to choose from",
                GroupName = "Appearance",
                SortOrder = 3,
                IsRequired = false,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "UI",
                Key = "ThemeAutoDetect",
                Value = "false",
                DefaultValue = "false",
                DataType = "bool",
                DisplayName = "Auto-Detect System Theme",
                Description = "Whether to automatically detect and use system dark/light mode preference",
                GroupName = "Appearance",
                SortOrder = 4,
                IsRequired = false,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "UI",
                Key = "AllowThemeCustomization",
                Value = "true",
                DefaultValue = "true",
                DataType = "bool",
                DisplayName = "Allow Theme Customization",
                Description = "Whether administrators can customize and create new themes",
                GroupName = "Appearance",
                SortOrder = 5,
                IsRequired = false,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "UI",
                Key = "ThemeTransitionDuration",
                Value = "300",
                DefaultValue = "300",
                DataType = "int",
                DisplayName = "Theme Transition Duration (ms)",
                Description = "Duration in milliseconds for theme transition animations",
                GroupName = "Appearance",
                SortOrder = 6,
                IsRequired = false,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },

            // Notification Settings
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "Notification",
                Key = "EmailNotificationsEnabled",
                Value = "true",
                DefaultValue = "true",
                DataType = "bool",
                DisplayName = "Enable Email Notifications",
                Description = "Whether email notifications are enabled system-wide",
                GroupName = "Email",
                SortOrder = 1,
                IsRequired = false,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "Notification",
                Key = "LicenseExpiryNotificationDays",
                Value = "30,7,1",
                DefaultValue = "30,7,1",
                DataType = "string",
                DisplayName = "License Expiry Notification Days",
                Description = "Days before expiry to send notifications (comma-separated)",
                GroupName = "License Alerts",
                SortOrder = 2,
                IsRequired = true,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },
       
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "Product Settings",
                Key = "AvailableCurrencies",
                Value = "USD",
                DefaultValue = "USD",
                DataType = "list",
                DisplayName = "Available Currencies",
                Description = "List of currencies available for product pricing",
                GroupName = "Product",
                SortOrder = 3,
                IsRequired = true,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                PossibleValues = "[\"USD\", \"EUR\", \"GBP\", \"INR\", \"SGD\", \"JPY\", \"CAD\", \"AUD\", \"CHF\", \"CNY\"]",
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },
            
            // Multi-list test setting
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "Product Settings",
                Key = "AvailableFeatures",
                Value = "[\"Analytics\", \"Reporting\"]",
                DefaultValue = "[]",
                DataType = "multi-list",
                DisplayName = "Available Features",
                Description = "Select multiple features available for products",
                GroupName = "Product",
                SortOrder = 4,
                IsRequired = false,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                PossibleValues = "[\"Analytics\", \"Reporting\", \"Dashboard\", \"API Access\", \"Mobile App\", \"Integrations\", \"Custom Branding\", \"Advanced Security\"]",
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            },
            
            // Image test setting
            new Setting
            {
                SettingId = Guid.NewGuid(),
                Category = "Branding",
                Key = "CompanyLogo",
                Value = "",
                DefaultValue = "",
                DataType = "image",
                DisplayName = "Company Logo",
                Description = "Upload company logo for branding purposes",
                GroupName = "Branding",
                SortOrder = 1,
                IsRequired = false,
                IsReadOnly = false,
                IsSensitive = false,
                TenantId = IdConstants.SystemTenantId,
                PossibleValues = "",
                Audit = new AuditInfo { CreatedBy = "System", CreatedOn = DateTime.UtcNow, UpdatedBy = "System", UpdatedOn = DateTime.UtcNow, IsActive = true }
            }
        };

        foreach (var setting in defaultSettings)
        {
            try
            {
                
                // Check if setting already exists by Category, Key and TenantId
                var existingSetting = await _unitOfWork.Settings.GetByKeyAsync(
                    setting.Category,
                    setting.Key);
                         
                if (existingSetting != null)
                {
                    _logger.LogDebug("Setting already exists: {SettingKey}", setting.FullKey);
                    continue;
                }
                await _unitOfWork.Settings.AddAsync(setting, cancellationToken);
                recordsCreated++;
                _logger.LogDebug("Created setting: {SettingKey}", setting.FullKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create setting: {SettingKey}", setting.FullKey);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created {Count} default system settings", recordsCreated);
        return recordsCreated;
    }

    protected override Dictionary<string, object> GetMetadata()
    {
        var metadata = base.GetMetadata();
        metadata["SettingCount"] = 22;
        metadata["Categories"] = "System,Email,Security,License,UI,Notification";
        metadata["SystemSettings"] = 5;
        metadata["EmailSettings"] = 7;
        metadata["SecuritySettings"] = 6;
        metadata["LicenseSettings"] = 4;
        metadata["ConfigurableSettings"] = 17;
        return metadata;
    }
}
