using TechWayFit.Licensing.Management.Core.Models.Enums;
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Services.Contracts;
using TechWayFit.Licensing.Management.Services.Strategies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace TechWayFit.Licensing.Management.Services.Factories;

/// <summary>
/// Factory for license generation strategies
/// </summary>
public interface ILicenseGenerationFactory
{
    /// <summary>
    /// Gets the appropriate strategy for the given license type
    /// </summary>
    /// <param name="licenseType">License type</param>
    /// <returns>License generation strategy</returns>
    ILicenseGenerationStrategy GetStrategy(LicenseType licenseType);

    /// <summary>
    /// Generates a license using the appropriate strategy
    /// </summary>
    /// <param name="request">License generation request</param>
    /// <param name="generatedBy">User generating the license</param>
    /// <returns>Generated product license</returns>
    Task<ProductLicense> GenerateAsync(LicenseGenerationRequest request, string generatedBy);

    /// <summary>
    /// Gets all available strategies
    /// </summary>
    /// <returns>Collection of all registered strategies</returns>
    IEnumerable<ILicenseGenerationStrategy> GetAllStrategies();
}

/// <summary>
/// Implementation of license generation factory
/// </summary>
public class LicenseGenerationFactory : ILicenseGenerationFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<LicenseType, Type> _strategyTypes;
    private readonly ILogger<LicenseGenerationFactory> _logger;

    public LicenseGenerationFactory(
        IServiceProvider serviceProvider,
        ILogger<LicenseGenerationFactory> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Register strategy types
        _strategyTypes = new Dictionary<LicenseType, Type>
        {
            { LicenseType.ProductLicenseFile, typeof(ProductLicenseFileStrategy) },
            { LicenseType.ProductKey, typeof(ProductKeyStrategy) },
            { LicenseType.VolumetricLicense, typeof(VolumetricLicenseStrategy) }
        };
    }

    public ILicenseGenerationStrategy GetStrategy(LicenseType licenseType)
    {
        if (!_strategyTypes.TryGetValue(licenseType, out var strategyType))
        {
            _logger.LogError("No strategy found for license type: {LicenseType}", licenseType);
            throw new NotSupportedException($"License type '{licenseType}' is not supported");
        }

        var strategy = _serviceProvider.GetService(strategyType) as ILicenseGenerationStrategy;
        if (strategy == null)
        {
            _logger.LogError("Failed to resolve strategy for license type: {LicenseType}", licenseType);
            throw new InvalidOperationException($"Strategy for license type '{licenseType}' could not be resolved");
        }

        _logger.LogDebug("Retrieved strategy {StrategyType} for license type {LicenseType}", 
            strategy.GetType().Name, licenseType);

        return strategy;
    }

    public async Task<ProductLicense> GenerateAsync(LicenseGenerationRequest request, string generatedBy)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(generatedBy))
            throw new ArgumentException("GeneratedBy cannot be null or empty", nameof(generatedBy));

        _logger.LogInformation("Generating license using factory for type: {LicenseType}", request.LicenseModel);

        var strategy = GetStrategy(request.LicenseModel);
        
        if (!strategy.CanHandle(request))
        {
            _logger.LogError("Strategy {StrategyType} cannot handle the request for license type {LicenseType}", 
                strategy.GetType().Name, request.LicenseModel);
            throw new InvalidOperationException($"Strategy for '{request.LicenseModel}' cannot handle the provided request");
        }

        return await strategy.GenerateAsync(request, generatedBy);
    }

    public IEnumerable<ILicenseGenerationStrategy> GetAllStrategies()
    {
        var strategies = new List<ILicenseGenerationStrategy>();

        foreach (var strategyType in _strategyTypes.Values)
        {
            var strategy = _serviceProvider.GetService(strategyType) as ILicenseGenerationStrategy;
            if (strategy != null)
            {
                strategies.Add(strategy);
            }
        }

        return strategies;
    }
}
