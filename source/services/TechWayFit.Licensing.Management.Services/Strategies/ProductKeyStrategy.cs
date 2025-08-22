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
/// Strategy for generating Product Keys (online activation)
/// </summary>
public class ProductKeyStrategy : BaseLicenseGenerationStrategy
{
    public override LicenseType SupportedType => LicenseType.ProductKey;

    public ProductKeyStrategy(
        IUnitOfWork unitOfWork,
        ILicenseGenerator licenseGenerator,
        IKeyManagementService keyManagementService,
        ILogger<ProductKeyStrategy> logger)
        : base(unitOfWork, licenseGenerator, keyManagementService, logger)
    {
    }

    protected override void ValidateRequest(LicenseGenerationRequest request, string generatedBy)
    {
        base.ValidateRequest(request, generatedBy);

        // Additional validation for Product Keys
        if (request.MaxDevices.HasValue && request.MaxDevices.Value > 10)
        {
            _logger.LogWarning("Product Key with {MaxDevices} devices may impact online activation performance", request.MaxDevices.Value);
        }

        _logger.LogDebug("Product Key request validation completed for product: {ProductId}", request.ProductId);
    }

    protected override async Task CustomizeGenerationRequestAsync(
        SimplifiedLicenseGenerationRequest generationRequest, LicenseGenerationRequest request)
    {
        // Generate the product key
        var productKey = ProductKeyGenerator.GenerateProductKey();
        
        // Set core license properties using the extension methods
        generationRequest
            .AddLicenseParameter(LicenseKeyParameter.LicenseType, "ProductKey")
            .AddLicenseParameter(LicenseKeyParameter.OnlineActivation, true)
            .AddLicenseParameter(LicenseKeyParameter.ProductKey, productKey)
            .AddLicenseParameter(LicenseKeyParameter.KeyFormat, "XXXX-XXXX-XXXX-XXXX");

        // Set activation requirements
        generationRequest
            .AddLicenseParameter(LicenseKeyParameter.RequiresActivation, true)
            .AddLicenseParameter(LicenseKeyParameter.ActivationUrl, "/api/activation/activate")
            .AddLicenseParameter(LicenseKeyParameter.ValidationUrl, "/api/activation/validate");

        // Configure device/machine settings
        if (request.MaxDevices.HasValue)
        {
            generationRequest
                .AddLicenseParameter(LicenseKeyParameter.MaxActivations, request.MaxDevices.Value)
                .AddLicenseParameter(LicenseKeyParameter.SupportMachineBinding, true);
        }
        else
        {
            // Default to single activation
            generationRequest.AddLicenseParameter(LicenseKeyParameter.MaxActivations, 1);
        }

        // Set validation requirements
        generationRequest
            .AddLicenseParameter(LicenseKeyParameter.RequireOnlineValidation, true)
            .AddLicenseParameter(LicenseKeyParameter.AllowOfflineGracePeriod, "72"); // 72 hours offline grace period

        // Enable key management features
        generationRequest
            .AddLicenseParameter(LicenseKeyParameter.SupportKeyDeactivation, true)
            .AddLicenseParameter(LicenseKeyParameter.SupportKeyTransfer, true);

        _logger.LogInformation("Generated Product Key {ProductKey} for product: {ProductId}", 
            productKey, request.ProductId);
        
        await Task.CompletedTask;
    }

    protected override async Task<ProductLicense> CreateLicenseEntityAsync(
        LicenseGenerationRequest request,
        SignedLicense signedLicense,
        SimplifiedLicenseGenerationRequest generationRequest,
        string generatedBy)
    {
        var licenseEntity = await base.CreateLicenseEntityAsync(request, signedLicense, generationRequest, generatedBy);

        // Override license key with formatted product key
        var productKey = generationRequest.GetLicenseParameterAsString(LicenseKeyParameter.ProductKey);
        if (!string.IsNullOrEmpty(productKey))
        {
            licenseEntity.LicenseKey = productKey;
        }

        // Add key-specific properties
        if (licenseEntity.Metadata == null)
            licenseEntity.Metadata = new Dictionary<string, object>();

        licenseEntity.Metadata["ProductKeyGenerated"] = "true";
        licenseEntity.Metadata["OnlineActivation"] = "true";
        licenseEntity.Metadata["KeyFormat"] = "XXXX-XXXX-XXXX-XXXX";
        licenseEntity.Metadata["RequiresActivation"] = "true";
        licenseEntity.Metadata["ActivationStatus"] = "Pending";
        licenseEntity.Metadata["ActivationsUsed"] = "0";
        licenseEntity.Metadata["MaxActivations"] = (request.MaxDevices ?? 1).ToString();

        // Activation tracking
        licenseEntity.Metadata["ActivationUrl"] = $"/api/activation/activate/{licenseEntity.LicenseId}";
        licenseEntity.Metadata["ValidationUrl"] = $"/api/activation/validate/{licenseEntity.LicenseId}";
        licenseEntity.Metadata["DeactivationUrl"] = $"/api/activation/deactivate/{licenseEntity.LicenseId}";

        _logger.LogInformation("Created Product Key entity for license: {LicenseId} with key: {ProductKey}", 
            licenseEntity.LicenseId, licenseEntity.LicenseKey);

        return licenseEntity;
    }

    protected override async Task PostProcessLicenseAsync(
        ProductLicense licenseEntity, LicenseGenerationRequest request, SignedLicense signedLicense)
    {
        // Post-processing for Product Keys
        _logger.LogInformation("Post-processing Product Key for license: {LicenseId}", licenseEntity.LicenseId);

        // TODO: In future implementations:
        // 1. Create activation record in activation tracking table
        // 2. Set up online validation endpoints
        // 3. Initialize machine binding records
        // 4. Send activation instructions email
        // 5. Create activation monitoring entries

        // For now, log the key generation
        _logger.LogInformation("Product Key {ProductKey} post-processing completed for license: {LicenseId}", 
            licenseEntity.LicenseKey, licenseEntity.LicenseId);
        
        await Task.CompletedTask;
    }

    public override Dictionary<string, object> GetRecommendedSettings()
    {
        return new Dictionary<string, object>
        {
            { "RecommendedExpiryMonths", 12 },
            { "MaxActivations", 1 },
            { "OnlineValidation", true },
            { "KeyFormat", "XXXX-XXXX-XXXX-XXXX" },
            { "OfflineGracePeriodHours", 72 },
            { "SupportMachineBinding", true },
            { "SupportKeyTransfer", true },
            { "RequiresActivation", true },
            { "RecommendedKeySize", 2048 },
            { "UsageScenario", "Standard online activation" },
            { "ActivationType", "Online" },
            { "ValidationMethod", "Server-side validation" }
        };
    }
}
