using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Enums;
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Services.Contracts;
using TechWayFit.Licensing.Generator.Services;
using TechWayFit.Licensing.Generator.Models;
using TechWayFit.Licensing.Core.Models;
using Microsoft.Extensions.Logging;

namespace TechWayFit.Licensing.Management.Services.Strategies.Base;

/// <summary>
/// Base class for license generation strategies with common functionality
/// </summary>
public abstract class BaseLicenseGenerationStrategy : ILicenseGenerationStrategy
{
    protected readonly IUnitOfWork _unitOfWork;
    protected readonly ILicenseGenerator _licenseGenerator;
    protected readonly IKeyManagementService _keyManagementService;
    protected readonly ILogger _logger;

    public abstract LicenseType SupportedType { get; }

    protected BaseLicenseGenerationStrategy(
        IUnitOfWork unitOfWork,
        ILicenseGenerator licenseGenerator,
        IKeyManagementService keyManagementService,
        ILogger logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _licenseGenerator = licenseGenerator ?? throw new ArgumentNullException(nameof(licenseGenerator));
        _keyManagementService = keyManagementService ?? throw new ArgumentNullException(nameof(keyManagementService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public virtual bool CanHandle(LicenseGenerationRequest request)
    {
        return request != null && request.LicenseModel == SupportedType;
    }

    public async Task<ProductLicense> GenerateAsync(LicenseGenerationRequest request, string generatedBy)
    {
        _logger.LogInformation("Starting {StrategyType} license generation for product: {ProductId}, consumer: {ConsumerId}",
            GetType().Name, request.ProductId, request.ConsumerId);

        // Input validation
        ValidateRequest(request, generatedBy);

        try
        {
            // Get or generate cryptographic keys
            var privateKey = await GetOrGeneratePrivateKeyAsync(request.ProductId);

            // Create base license generation request
            var generationRequest = await CreateGenerationRequestAsync(request, privateKey);

            // Type-specific customization
            await CustomizeGenerationRequestAsync(generationRequest, request);

            // Generate the cryptographically signed license
            var signedLicense = await _licenseGenerator.GenerateLicenseAsync(generationRequest);

            // Create the license entity
            var licenseEntity = await CreateLicenseEntityAsync(request, signedLicense, generationRequest, generatedBy);

            // Type-specific post-processing
            await PostProcessLicenseAsync(licenseEntity, request, signedLicense);

            // Save to repository
            var createdEntity = await _unitOfWork.Licenses.AddAsync(licenseEntity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully generated {LicenseType} license with ID: {LicenseId}", 
                SupportedType, createdEntity.LicenseId);

            return createdEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating {LicenseType} license for product: {ProductId}", 
                SupportedType, request.ProductId);
            throw;
        }
    }

    public abstract Dictionary<string, object> GetRecommendedSettings();

    #region Protected Virtual Methods for Customization

    /// <summary>
    /// Validates the license generation request
    /// </summary>
    protected virtual void ValidateRequest(LicenseGenerationRequest request, string generatedBy)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(generatedBy))
            throw new ArgumentException("GeneratedBy cannot be null or empty", nameof(generatedBy));
        if (request.LicenseModel != SupportedType)
            throw new ArgumentException($"Request license model '{request.LicenseModel}' does not match strategy type '{SupportedType}'");
    }

    /// <summary>
    /// Gets or generates the private key for the product
    /// </summary>
    protected virtual async Task<string> GetOrGeneratePrivateKeyAsync(Guid productId)
    {
        var privateKey = await _keyManagementService.GetPrivateKeyAsync(productId);
        if (string.IsNullOrEmpty(privateKey))
        {
            _logger.LogInformation("No private key found for product {ProductId}, generating new key pair", productId);
            var publicKey = await _keyManagementService.GenerateKeyPairForProductAsync(productId);
            privateKey = await _keyManagementService.GetPrivateKeyAsync(productId);
            _logger.LogInformation("Generated new key pair for product {ProductId}, public key length: {PublicKeyLength}",
                productId, publicKey.Length);
        }
        return privateKey;
    }

    /// <summary>
    /// Creates the base license generation request
    /// </summary>
    protected virtual async Task<SimplifiedLicenseGenerationRequest> CreateGenerationRequestAsync(
        LicenseGenerationRequest request, string privateKey)
    {
        return await Task.FromResult(new SimplifiedLicenseGenerationRequest
        {
            LicenseId = Guid.NewGuid(),
            ProductId = request.ProductId,
            ProductName = request.ProductName ?? "Unknown Product",
            LicensedTo = request.ConsumerName ?? "Unknown Consumer",
            ContactEmail = "unknown@example.com",
            ContactPerson = "Unknown Contact",
            ValidFrom = request.ValidFrom ?? DateTime.UtcNow,
            ValidTo = request.ExpiryDate ?? DateTime.UtcNow.AddYears(1),
            Tier = MapTierFromRequest(request.ProductTier),
            Features = MapFeaturesFromRequest(request).Select(f => new LicenseFeature { Name = f }).ToList(),
            PrivateKeyPem = privateKey
        });
    }    /// <summary>
    /// Customizes the generation request for the specific license type
    /// </summary>
    protected abstract Task CustomizeGenerationRequestAsync(
        SimplifiedLicenseGenerationRequest generationRequest, LicenseGenerationRequest request);

    /// <summary>
    /// Creates the license entity from the generation request and signed license
    /// </summary>
    protected virtual async Task<ProductLicense> CreateLicenseEntityAsync(
        LicenseGenerationRequest request,
        SignedLicense signedLicense,
        SimplifiedLicenseGenerationRequest generationRequest,
        string generatedBy)
    {
        return new ProductLicense
        {
            ProductId = request.ProductId,
            ConsumerId = request.ConsumerId,
            ProductTierId = request.TierId,
            ValidProductVersionFrom = request.ValidProductVersionFrom,
            ValidProductVersionTo = request.ValidProductVersionTo,
            LicenseKey = signedLicense.LicenseData ?? string.Empty,
            ValidFrom = generationRequest.ValidFrom,
            ValidTo = generationRequest.ValidTo,
            Status = LicenseStatus.Active,
            LicenseModel = request.LicenseModel,
            MaxAllowedUsers = request.MaxUsers,
            Metadata = request.Metadata ?? new Dictionary<string, object>(),
            CreatedBy = generatedBy,
            CreatedOn = DateTime.UtcNow,
            UpdatedBy = generatedBy,
            UpdatedOn = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Performs type-specific post-processing after license creation
    /// </summary>
    protected virtual async Task PostProcessLicenseAsync(
        ProductLicense licenseEntity, LicenseGenerationRequest request, SignedLicense signedLicense)
    {
        // Default implementation does nothing - override in derived classes
        await Task.CompletedTask;
    }

    #endregion

    #region Protected Helper Methods

    protected virtual LicenseTier MapTierFromRequest(string? productTier)
    {
        return productTier?.ToLowerInvariant() switch
        {
            "enterprise" => LicenseTier.Enterprise,
            "professional" => LicenseTier.Professional,
            "community" => LicenseTier.Community,
            "premium" => LicenseTier.Custom,
            _ => LicenseTier.Community
        };
    }

    protected virtual List<string> MapFeaturesFromRequest(LicenseGenerationRequest request)
    {
        var features = new List<string>();
        
        if (request.Metadata != null)
        {
            foreach (var metadata in request.Metadata)
            {
                if (metadata.Key.StartsWith("Feature_") && metadata.Value is bool isEnabled && isEnabled)
                {
                    features.Add(metadata.Key.Replace("Feature_", ""));
                }
            }
        }

        return features;
    }

    #endregion
}
