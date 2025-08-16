using TechWayFit.Licensing.Management.Core.Contracts.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace TechWayFit.Licensing.Management.Web.Data.Seeders
{
    /// <summary>
    /// Seeder to initialize database with default configuration settings
    /// This replaces the need for static configuration files
    /// </summary>
    public class ConfigurationSettingsSeeder
    {
        private readonly ISettingService _settingService;
        private readonly ILogger<ConfigurationSettingsSeeder> _logger;

        public ConfigurationSettingsSeeder(
            ISettingService settingService,
            ILogger<ConfigurationSettingsSeeder> logger)
        {
            _settingService = settingService;
            _logger = logger;
        }

        /// <summary>
        /// Seed all configuration settings if they don't already exist
        /// </summary>
        public async Task SeedConfigurationSettingsAsync()
        {
            _logger.LogInformation("Starting configuration settings seeding");

            await SeedCurrencySettingsAsync();
            await SeedSlaTemplateSettingsAsync();
            await SeedProductDisplaySettingsAsync();
            await SeedTierLimitSettingsAsync();
            await SeedSystemDefaultSettingsAsync();
            await SeedLicenseSettingsAsync();
            await SeedNotificationSettingsAsync();
            await SeedSecuritySettingsAsync();

            _logger.LogInformation("Configuration settings seeding completed");
        }

        private async Task SeedCurrencySettingsAsync()
        {
            if (await _settingService.SettingExistsAsync("ProductConfiguration", "SupportedCurrencies"))
            {
                _logger.LogDebug("Currency settings already exist, skipping");
                return;
            }

            var currencies = new[]
            {
                new { Code = "USD", Symbol = "$", Name = "US Dollar", IsDefault = true },
                new { Code = "EUR", Symbol = "€", Name = "Euro", IsDefault = false },
                new { Code = "GBP", Symbol = "£", Name = "British Pound", IsDefault = false },
                new { Code = "INR", Symbol = "₹", Name = "Indian Rupee", IsDefault = false },
                new { Code = "CAD", Symbol = "C$", Name = "Canadian Dollar", IsDefault = false },
                new { Code = "AUD", Symbol = "A$", Name = "Australian Dollar", IsDefault = false },
                new { Code = "JPY", Symbol = "¥", Name = "Japanese Yen", IsDefault = false },
                new { Code = "SGD", Symbol = "S$", Name = "Singapore Dollar", IsDefault = false }
            };

            var settings = new Dictionary<string, object?>
            {
                ["ProductConfiguration.SupportedCurrencies"] = JsonSerializer.Serialize(currencies),
                ["ProductConfiguration.DefaultCurrency"] = "USD",
                ["ProductConfiguration.AllowCurrencyChange"] = true,
                ["ProductConfiguration.CurrencyDisplayFormat"] = "Symbol"
            };

            await _settingService.UpdateMultipleSettingsAsync(settings, "System Seeder");
            _logger.LogInformation("Seeded {Count} currency settings", settings.Count);
        }

        private async Task SeedSlaTemplateSettingsAsync()
        {
            if (await _settingService.SettingExistsAsync("ProductConfiguration", "SlaTemplates"))
            {
                _logger.LogDebug("SLA template settings already exist, skipping");
                return;
            }

            var slaTemplates = new[]
            {
                new { 
                    Name = "Basic Support",
                    ResponseTime = "48 hours",
                    ResolutionTime = "5 business days",
                    Channels = new[] { "Email" },
                    Priority = "Normal",
                    BusinessHours = "9 AM - 5 PM (Mon-Fri)",
                    IsDefault = false
                },
                new { 
                    Name = "Standard Support",
                    ResponseTime = "24 hours",
                    ResolutionTime = "3 business days",
                    Channels = new[] { "Email", "Phone" },
                    Priority = "Normal",
                    BusinessHours = "8 AM - 6 PM (Mon-Fri)",
                    IsDefault = true
                },
                new { 
                    Name = "Premium Support",
                    ResponseTime = "4 hours",
                    ResolutionTime = "1 business day",
                    Channels = new[] { "Email", "Phone", "Live Chat" },
                    Priority = "High",
                    BusinessHours = "24/7",
                    IsDefault = false
                },
                new { 
                    Name = "Enterprise Support",
                    ResponseTime = "1 hour",
                    ResolutionTime = "4 hours",
                    Channels = new[] { "Email", "Phone", "Live Chat", "Dedicated Manager" },
                    Priority = "Critical",
                    BusinessHours = "24/7",
                    IsDefault = false
                }
            };

            var settings = new Dictionary<string, object?>
            {
                ["ProductConfiguration.SlaTemplates"] = JsonSerializer.Serialize(slaTemplates),
                ["ProductConfiguration.DefaultSlaTemplate"] = "Standard Support",
                ["ProductConfiguration.AllowCustomSla"] = true
            };

            await _settingService.UpdateMultipleSettingsAsync(settings, "System Seeder");
            _logger.LogInformation("Seeded {Count} SLA template settings", settings.Count);
        }

        private async Task SeedProductDisplaySettingsAsync()
        {
            var settings = new Dictionary<string, object?>
            {
                ["ProductDisplay.ProductsPerPage"] = 10,
                ["ProductDisplay.DefaultSortColumn"] = "Name",
                ["ProductDisplay.DefaultSortDirection"] = "ASC",
                ["ProductDisplay.ShowDescriptionInList"] = true,
                ["ProductDisplay.ShowPricingInList"] = true,
                ["ProductDisplay.EnableProductCategories"] = true,
                ["ProductDisplay.AllowProductComparison"] = true,
                ["ProductDisplay.MaxComparisonItems"] = 3,
                ["ProductDisplay.EnableProductSearch"] = true,
                ["ProductDisplay.SearchMinCharacters"] = 2,
                ["ProductDisplay.ShowProductImages"] = true,
                ["ProductDisplay.DefaultViewMode"] = "Grid" // Grid, List, Card
            };

            await _settingService.UpdateMultipleSettingsAsync(settings, "System Seeder");
            _logger.LogInformation("Seeded {Count} product display settings", settings.Count);
        }

        private async Task SeedTierLimitSettingsAsync()
        {
            var settings = new Dictionary<string, object?>
            {
                // User limits per tier
                ["TierLimits.MaxUsers.Basic"] = 5,
                ["TierLimits.MaxUsers.Standard"] = 25,
                ["TierLimits.MaxUsers.Premium"] = 100,
                ["TierLimits.MaxUsers.Enterprise"] = -1, // Unlimited
                
                // Device limits per tier
                ["TierLimits.MaxDevices.Basic"] = 2,
                ["TierLimits.MaxDevices.Standard"] = 10,
                ["TierLimits.MaxDevices.Premium"] = 50,
                ["TierLimits.MaxDevices.Enterprise"] = -1, // Unlimited
                
                // Storage limits (GB) per tier
                ["TierLimits.StorageGB.Basic"] = 5,
                ["TierLimits.StorageGB.Standard"] = 50,
                ["TierLimits.StorageGB.Premium"] = 500,
                ["TierLimits.StorageGB.Enterprise"] = -1, // Unlimited
                
                // API rate limits per tier
                ["TierLimits.ApiRateLimit.Basic"] = 1000,
                ["TierLimits.ApiRateLimit.Standard"] = 10000,
                ["TierLimits.ApiRateLimit.Premium"] = 100000,
                ["TierLimits.ApiRateLimit.Enterprise"] = -1, // Unlimited
                
                // Feature limits
                ["TierLimits.MaxProjects.Basic"] = 3,
                ["TierLimits.MaxProjects.Standard"] = 10,
                ["TierLimits.MaxProjects.Premium"] = 50,
                ["TierLimits.MaxProjects.Enterprise"] = -1, // Unlimited
                
                // Support limits
                ["TierLimits.SupportTicketsPerMonth.Basic"] = 5,
                ["TierLimits.SupportTicketsPerMonth.Standard"] = 20,
                ["TierLimits.SupportTicketsPerMonth.Premium"] = 100,
                ["TierLimits.SupportTicketsPerMonth.Enterprise"] = -1 // Unlimited
            };

            await _settingService.UpdateMultipleSettingsAsync(settings, "System Seeder");
            _logger.LogInformation("Seeded {Count} tier limit settings", settings.Count);
        }

        private async Task SeedSystemDefaultSettingsAsync()
        {
            var settings = new Dictionary<string, object?>
            {
                ["SystemDefaults.DefaultBillingCycle"] = "Monthly",
                ["SystemDefaults.TrialPeriodDays"] = 30,
                ["SystemDefaults.GracePeriodDays"] = 7,
                ["SystemDefaults.AutoRenewal"] = true,
                ["SystemDefaults.SendExpirationNotifications"] = true,
                ["SystemDefaults.NotificationDaysBeforeExpiry"] = "30,15,7,1", // Comma-separated days
                ["SystemDefaults.AllowDowngrade"] = true,
                ["SystemDefaults.AllowMidCycleUpgrade"] = true,
                ["SystemDefaults.ProrationPolicy"] = "Immediate", // Immediate, NextBilling, NoProration
                ["SystemDefaults.RequireApprovalForTierChange"] = false,
                ["SystemDefaults.DefaultTimeZone"] = "UTC",
                ["SystemDefaults.DateFormat"] = "MM/dd/yyyy",
                ["SystemDefaults.TimeFormat"] = "HH:mm",
                ["SystemDefaults.NumberFormat"] = "en-US",
                ["SystemDefaults.PageSize"] = 10,
                ["SystemDefaults.SessionTimeoutMinutes"] = 30,
                ["SystemDefaults.EnableAuditLogging"] = true
            };

            await _settingService.UpdateMultipleSettingsAsync(settings, "System Seeder");
            _logger.LogInformation("Seeded {Count} system default settings", settings.Count);
        }

        private async Task SeedLicenseSettingsAsync()
        {
            var settings = new Dictionary<string, object?>
            {
                ["LicenseSettings.DefaultLicenseType"] = "Standard",
                ["LicenseSettings.MaxActivationsPerLicense"] = 1,
                ["LicenseSettings.AllowOfflineActivation"] = true,
                ["LicenseSettings.OfflineActivationDays"] = 30,
                ["LicenseSettings.HeartbeatIntervalMinutes"] = 60,
                ["LicenseSettings.GracePeriodForHeartbeat"] = 24, // hours
                ["LicenseSettings.AllowLicenseTransfer"] = true,
                ["LicenseSettings.TransferCooldownDays"] = 30,
                ["LicenseSettings.EnableHardwareFingerprinting"] = true,
                ["LicenseSettings.FingerprintToleranceLevel"] = "Medium", // Low, Medium, High
                ["LicenseSettings.AutoRevokeSuspiciousActivations"] = false,
                ["LicenseSettings.RequireActivationApproval"] = false,
                ["LicenseSettings.LicenseKeyFormat"] = "XXXX-XXXX-XXXX-XXXX",
                ["LicenseSettings.LicenseKeyLength"] = 16
            };

            await _settingService.UpdateMultipleSettingsAsync(settings, "System Seeder");
            _logger.LogInformation("Seeded {Count} license settings", settings.Count);
        }

        private async Task SeedNotificationSettingsAsync()
        {
            var settings = new Dictionary<string, object?>
            {
                ["NotificationSettings.EnableEmailNotifications"] = true,
                ["NotificationSettings.EnableSmsNotifications"] = false,
                ["NotificationSettings.EnableInAppNotifications"] = true,
                ["NotificationSettings.DefaultFromEmail"] = "noreply@techwayfitlicensing.com",
                ["NotificationSettings.DefaultFromName"] = "TechWayFit Licensing",
                ["NotificationSettings.SmtpServer"] = "",
                ["NotificationSettings.SmtpPort"] = 587,
                ["NotificationSettings.SmtpUsername"] = "",
                ["NotificationSettings.SmtpPassword"] = "",
                ["NotificationSettings.SmtpEnableSsl"] = true,
                ["NotificationSettings.RetryAttempts"] = 3,
                ["NotificationSettings.RetryIntervalMinutes"] = 15,
                ["NotificationSettings.NotificationRetentionDays"] = 90,
                ["NotificationSettings.BatchSize"] = 50,
                ["NotificationSettings.SendWelcomeEmail"] = true,
                ["NotificationSettings.SendLicenseExpiryWarnings"] = true,
                ["NotificationSettings.SendActivationNotifications"] = true
            };

            await _settingService.UpdateMultipleSettingsAsync(settings, "System Seeder");
            _logger.LogInformation("Seeded {Count} notification settings", settings.Count);
        }

        private async Task SeedSecuritySettingsAsync()
        {
            var settings = new Dictionary<string, object?>
            {
                ["SecuritySettings.RequireStrongPasswords"] = true,
                ["SecuritySettings.MinPasswordLength"] = 8,
                ["SecuritySettings.RequireUppercase"] = true,
                ["SecuritySettings.RequireLowercase"] = true,
                ["SecuritySettings.RequireNumbers"] = true,
                ["SecuritySettings.RequireSpecialCharacters"] = true,
                ["SecuritySettings.PasswordExpirationDays"] = 90,
                ["SecuritySettings.MaxFailedLoginAttempts"] = 5,
                ["SecuritySettings.LockoutDurationMinutes"] = 30,
                ["SecuritySettings.EnableTwoFactorAuth"] = false,
                ["SecuritySettings.SessionIdleTimeoutMinutes"] = 30,
                ["SecuritySettings.RequireEmailVerification"] = true,
                ["SecuritySettings.AllowPasswordReset"] = true,
                ["SecuritySettings.PasswordResetTokenExpiryMinutes"] = 60,
                ["SecuritySettings.EnableAccountLockout"] = true,
                ["SecuritySettings.EnableIpWhitelist"] = false,
                ["SecuritySettings.AllowedIpAddresses"] = "",
                ["SecuritySettings.EnableCors"] = true,
                ["SecuritySettings.AllowedOrigins"] = "*"
            };

            await _settingService.UpdateMultipleSettingsAsync(settings, "System Seeder");
            _logger.LogInformation("Seeded {Count} security settings", settings.Count);
        }
    }
}
