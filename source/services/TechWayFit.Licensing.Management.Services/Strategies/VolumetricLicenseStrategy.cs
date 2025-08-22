using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Enums;
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Core.Utilities;
using TechWayFit.Licensing.Management.Core.Extensions;
using TechWayFit.Licensing.Management.Services.Contracts;
using TechWayFit.Licensing.Management.Services.Strategies.Base;
using TechWayFit.Licensing.Generator.Services;
using TechWayFit.Licensing.Generator.Models;
using TechWayFit.Licensing.Core.Models;
using Microsoft.Extensions.Logging;

namespace TechWayFit.Licensing.Management.Services.Strategies;

/// <summary>
/// Strategy for generating Volumetric Licenses (multi-user keys)
/// </summary>
public class VolumetricLicenseStrategy : BaseLicenseGenerationStrategy
{
    public override LicenseType SupportedType => LicenseType.VolumetricLicense;

    public VolumetricLicenseStrategy(
        IUnitOfWork unitOfWork,
        ILicenseGenerator licenseGenerator,
        IKeyManagementService keyManagementService,
        ILogger<VolumetricLicenseStrategy> logger)
        : base(unitOfWork, licenseGenerator, keyManagementService, logger)
    {
    }

    protected override void ValidateRequest(LicenseGenerationRequest request, string generatedBy)
    {
        base.ValidateRequest(request, generatedBy);

        // Additional validation for Volumetric Licenses
        if (!request.MaxUsers.HasValue || request.MaxUsers.Value < 2)
        {
            throw new ArgumentException("Volumetric licenses require MaxUsers to be specified and >= 2");
        }

        if (request.MaxUsers.Value > 9999)
        {
            throw new ArgumentException("Maximum users for volumetric licenses cannot exceed 9999");
        }

        _logger.LogDebug("Volumetric License request validation completed for product: {ProductId}, MaxUsers: {MaxUsers}", 
            request.ProductId, request.MaxUsers.Value);
    }

    protected override async Task CustomizeGenerationRequestAsync(
        SimplifiedLicenseGenerationRequest generationRequest, LicenseGenerationRequest request)
    {
        // Generate base volumetric key
        var baseKey = ProductKeyGenerator.GenerateProductKey();
        var userSlotCount = request.MaxUsers!.Value;
        
        // Format: XXXX-XXXX-XXXX-NNNN where NNNN is the user count
        var volumetricKey = $"{baseKey[..14]}-{userSlotCount:D4}";
        
        // Set core license properties using extension methods
        generationRequest
            .AddLicenseParameter(LicenseKeyParameter.LicenseType, "VolumetricLicense")
            .AddLicenseParameter(LicenseKeyParameter.BaseKey, baseKey)
            .AddLicenseParameter(LicenseKeyParameter.VolumetricKey, volumetricKey)
            .AddLicenseParameter(LicenseKeyParameter.KeyFormat, "XXXX-XXXX-XXXX-NNNN")
            .AddLicenseParameter(LicenseKeyParameter.MaxUsers, userSlotCount);
        
        // Multi-user specific settings
        generationRequest
            .AddLicenseParameter(LicenseKeyParameter.SupportsConcurrentUsers, true)
            .AddLicenseParameter(LicenseKeyParameter.UserSlotAllocation, userSlotCount)
            .AddLicenseParameter(LicenseKeyParameter.UsageTracking, true)
            .AddLicenseParameter(LicenseKeyParameter.ConcurrentUserLimit, userSlotCount);
        
        // Advanced volumetric features
        generationRequest
            .AddLicenseParameter(LicenseKeyParameter.SupportUserPooling, true)
            .AddLicenseParameter(LicenseKeyParameter.SupportDynamicScaling, false) // Could be enabled in future
            .AddLicenseParameter(LicenseKeyParameter.SupportUsageReporting, true)
            .AddLicenseParameter(LicenseKeyParameter.SupportUserManagement, true);
        
        // Online validation for user tracking
        generationRequest
            .AddLicenseParameter(LicenseKeyParameter.RequireOnlineValidation, true)
            .AddLicenseParameter(LicenseKeyParameter.OnlineActivation, true)
            .AddLicenseParameter(LicenseKeyParameter.UserTrackingUrl, "/api/volumetric/track-usage")
            .AddLicenseParameter(LicenseKeyParameter.UsageReportingUrl, "/api/volumetric/usage-report");
        
        // Set concurrent connections to match user count
        generationRequest.MaxConcurrentConnections = userSlotCount;
        
        _logger.LogInformation("Generated Volumetric License {VolumetricKey} for product: {ProductId} with {MaxUsers} user slots", 
            volumetricKey, request.ProductId, userSlotCount);
        
        await Task.CompletedTask;
    }

    protected override async Task<ProductLicense> CreateLicenseEntityAsync(
        LicenseGenerationRequest request,
        SignedLicense signedLicense,
        SimplifiedLicenseGenerationRequest generationRequest,
        string generatedBy)
    {
        var licenseEntity = await base.CreateLicenseEntityAsync(request, signedLicense, generationRequest, generatedBy);

        // Override license key with formatted volumetric key
        var volumetricKey = generationRequest.GetLicenseParameterAsString(LicenseKeyParameter.VolumetricKey);
        if (!string.IsNullOrEmpty(volumetricKey))
        {
            licenseEntity.LicenseKey = volumetricKey;
        }

        // Ensure MaxAllowedUsers is set
        licenseEntity.MaxAllowedUsers = request.MaxUsers!.Value;

        // Add volumetric-specific properties
        if (licenseEntity.Metadata == null)
            licenseEntity.Metadata = new Dictionary<string, object>();

        licenseEntity.Metadata["VolumetricLicenseGenerated"] = "true";
        licenseEntity.Metadata["OnlineActivation"] = "true";
        licenseEntity.Metadata["KeyFormat"] = "XXXX-XXXX-XXXX-NNNN";
        licenseEntity.Metadata["MaxUsers"] = request.MaxUsers.Value.ToString();
        licenseEntity.Metadata["ConcurrentUserLimit"] = request.MaxUsers.Value.ToString();
        licenseEntity.Metadata["CurrentActiveUsers"] = "0";
        licenseEntity.Metadata["TotalUsersRegistered"] = "0";
        
        // Usage tracking
        licenseEntity.Metadata["UsageTrackingEnabled"] = "true";
        licenseEntity.Metadata["LastUsageUpdate"] = DateTime.UtcNow.ToString("O");
        licenseEntity.Metadata["UsageReportingEnabled"] = "true";
        
        // User management URLs
        licenseEntity.Metadata["UserTrackingUrl"] = $"/api/volumetric/{licenseEntity.LicenseId}/track-usage";
        licenseEntity.Metadata["UsageReportUrl"] = $"/api/volumetric/{licenseEntity.LicenseId}/usage-report";
        licenseEntity.Metadata["UserManagementUrl"] = $"/api/volumetric/{licenseEntity.LicenseId}/manage-users";

        _logger.LogInformation("Created Volumetric License entity for license: {LicenseId} with key: {VolumetricKey}, MaxUsers: {MaxUsers}", 
            licenseEntity.LicenseId, licenseEntity.LicenseKey, request.MaxUsers.Value);

        return licenseEntity;
    }

    protected override async Task PostProcessLicenseAsync(
        ProductLicense licenseEntity, LicenseGenerationRequest request, SignedLicense signedLicense)
    {
        // Post-processing for Volumetric Licenses
        _logger.LogInformation("Post-processing Volumetric License for license: {LicenseId}", licenseEntity.LicenseId);

        // TODO: In future implementations:
        // 1. Create user slot allocation table
        // 2. Initialize usage tracking records
        // 3. Set up concurrent user monitoring
        // 4. Create user management API endpoints
        // 5. Initialize usage reporting system
        // 6. Set up usage analytics
        // 7. Send multi-user activation instructions

        // For now, log the volumetric key generation
        _logger.LogInformation("Volumetric License {VolumetricKey} with {MaxUsers} user slots post-processing completed for license: {LicenseId}", 
            licenseEntity.LicenseKey, licenseEntity.MaxAllowedUsers, licenseEntity.LicenseId);
        
        await Task.CompletedTask;
    }

    public override Dictionary<string, object> GetRecommendedSettings()
    {
        return new Dictionary<string, object>
        {
            { "RecommendedExpiryMonths", 12 },
            { "MinUsers", 2 },
            { "MaxUsers", 9999 },
            { "DefaultUsers", 10 },
            { "OnlineValidation", true },
            { "KeyFormat", "XXXX-XXXX-XXXX-NNNN" },
            { "SupportsConcurrentUsers", true },
            { "UsageTracking", true },
            { "UserPooling", true },
            { "RequiresActivation", true },
            { "RecommendedKeySize", 2048 },
            { "UsageScenario", "Multi-user team licensing" },
            { "ActivationType", "Online" },
            { "ValidationMethod", "Server-side with user tracking" },
            { "SupportUsageReporting", true },
            { "SupportUserManagement", true }
        };
    }
}
