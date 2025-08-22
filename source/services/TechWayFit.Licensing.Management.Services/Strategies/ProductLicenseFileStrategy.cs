using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Enums;
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Core.Extensions;
using TechWayFit.Licensing.Management.Services.Contracts;
using TechWayFit.Licensing.Management.Services.Strategies.Base;
using TechWayFit.Licensing.Generator.Services;
using TechWayFit.Licensing.Generator.Models;
using TechWayFit.Licensing.Core.Models;
using Microsoft.Extensions.Logging;

namespace TechWayFit.Licensing.Management.Services.Strategies;

/// <summary>
/// Strategy for generating Product License Files (offline activation)
/// </summary>
public class ProductLicenseFileStrategy : BaseLicenseGenerationStrategy
{
    public override LicenseType SupportedType => LicenseType.ProductLicenseFile;

    public ProductLicenseFileStrategy(
        IUnitOfWork unitOfWork,
        ILicenseGenerator licenseGenerator,
        IKeyManagementService keyManagementService,
        ILogger<ProductLicenseFileStrategy> logger)
        : base(unitOfWork, licenseGenerator, keyManagementService, logger)
    {
    }

    protected override void ValidateRequest(LicenseGenerationRequest request, string generatedBy)
    {
        base.ValidateRequest(request, generatedBy);

        // Additional validation for Product License Files
        if (request.ExpiryDate.HasValue && request.ExpiryDate.Value <= DateTime.UtcNow)
        {
            throw new ArgumentException("Expiry date must be in the future for Product License Files");
        }

        _logger.LogDebug("Product License File request validation completed for product: {ProductId}", request.ProductId);
    }

    protected override async Task CustomizeGenerationRequestAsync(
        SimplifiedLicenseGenerationRequest generationRequest, LicenseGenerationRequest request)
    {
        // Set core license properties using extension methods
        generationRequest
            .AddLicenseParameter(LicenseKeyParameter.LicenseType, "ProductLicenseFile")
            .AddLicenseParameter(LicenseKeyParameter.OfflineActivation, true)
            .AddLicenseParameter(LicenseKeyParameter.LicenseFileFormat, "XML");
        
        // Add machine binding capabilities for offline licenses
        if (request.MaxDevices.HasValue)
        {
            generationRequest.AddLicenseParameter(LicenseKeyParameter.MaxMachineBindings, request.MaxDevices.Value);
        }

        // Enhanced features for enterprise offline use
        generationRequest
            .AddLicenseParameter(LicenseKeyParameter.SupportOfflineValidation, true)
            .AddLicenseParameter(LicenseKeyParameter.IncludeProductMetadata, true);
        
        // File-specific settings
        generationRequest
            .AddLicenseParameter(LicenseKeyParameter.GenerateLicenseFile, true)
            .AddLicenseParameter(LicenseKeyParameter.IncludePublicKey, true);

        _logger.LogInformation("Customized Product License File generation request for product: {ProductId}", request.ProductId);
        
        await Task.CompletedTask;
    }

    protected override async Task<ProductLicense> CreateLicenseEntityAsync(
        LicenseGenerationRequest request,
        SignedLicense signedLicense,
        SimplifiedLicenseGenerationRequest generationRequest,
        string generatedBy)
    {
        var licenseEntity = await base.CreateLicenseEntityAsync(request, signedLicense, generationRequest, generatedBy);

        // Add file-specific properties
        if (licenseEntity.Metadata == null)
            licenseEntity.Metadata = new Dictionary<string, object>();

        licenseEntity.Metadata["LicenseFileGenerated"] = true;
        licenseEntity.Metadata["OfflineActivation"] = true;
        licenseEntity.Metadata["FileFormat"] = "XML";
        licenseEntity.Metadata["SupportsOfflineValidation"] = true;

        // Add download information (would be implemented in future)
        licenseEntity.Metadata["DownloadUrl"] = $"/api/licenses/{licenseEntity.LicenseId}/download";
        licenseEntity.Metadata["FileSize"] = signedLicense.LicenseData?.Length ?? 0;

        _logger.LogInformation("Created Product License File entity for license: {LicenseId}", licenseEntity.LicenseId);

        return licenseEntity;
    }

    protected override async Task PostProcessLicenseAsync(
        ProductLicense licenseEntity, LicenseGenerationRequest request, SignedLicense signedLicense)
    {
        // Post-processing for Product License Files
        _logger.LogInformation("Post-processing Product License File for license: {LicenseId}", licenseEntity.LicenseId);

        // TODO: In future implementations:
        // 1. Generate downloadable license file
        // 2. Store file in secure storage
        // 3. Create download token
        // 4. Send notification email with download instructions

        // For now, log the completion
        _logger.LogInformation("Product License File post-processing completed for license: {LicenseId}", licenseEntity.LicenseId);
        
        await Task.CompletedTask;
    }

    public override Dictionary<string, object> GetRecommendedSettings()
    {
        return new Dictionary<string, object>
        {
            { "RecommendedExpiryMonths", 12 },
            { "MaxDevices", 5 },
            { "OfflineValidation", true },
            { "FileFormat", "XML" },
            { "IncludePublicKey", true },
            { "SupportMachineBinding", true },
            { "RecommendedKeySize", 2048 },
            { "UsageScenario", "Enterprise offline deployment" },
            { "ActivationType", "Offline" },
            { "ValidationMethod", "Local file validation" }
        };
    }
}
