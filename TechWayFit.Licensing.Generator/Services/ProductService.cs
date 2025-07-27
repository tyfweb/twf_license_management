using TechWayFit.Licensing.Core.Contracts;
using TechWayFit.Licensing.Core.Models;
using Microsoft.Extensions.Logging;

namespace TechWayFit.Licensing.Generator.Services
{
    /// <summary>
    /// Service for managing products
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository productRepository, ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductConfiguration>> GetAllProductsAsync()
        {
            return await _productRepository.GetAllProductsAsync();
        }

        public async Task<ProductConfiguration?> GetProductAsync(string productId)
        {
            return await _productRepository.GetProductByIdAsync(productId);
        }

        public async Task<ProductConfiguration> CreateProductAsync(ProductConfiguration product)
        {
            return await _productRepository.CreateProductAsync(product);
        }

        public async Task<ProductConfiguration> UpdateProductAsync(ProductConfiguration product)
        {
            return await _productRepository.UpdateProductAsync(product);
        }

        public async Task<ProductConfiguration> InitializeProductAsync(string productId, string productName, ProductType productType)
        {
            _logger.LogInformation("Initializing product {ProductId} of type {ProductType}", productId, productType);

            var product = new ProductConfiguration
            {
                ProductId = productId,
                ProductName = productName,
                ProductType = productType,
                Version = "1.0",
                IsActive = true,
                FeatureTiers = CreateDefaultFeatureTiers(productType),
                AvailableFeatures = CreateDefaultFeatures(productType),
                DefaultLimitations = CreateDefaultLimitations(productType)
            };

            return await _productRepository.CreateProductAsync(product);
        }

        public async Task<List<ProductFeatureDefinition>> GetFeaturesForTierAsync(string productId, LicenseTier tier)
        {
            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null)
                return new List<ProductFeatureDefinition>();

            if (product.FeatureTiers.TryGetValue(tier, out var tierFeatures))
                return tierFeatures;

            return new List<ProductFeatureDefinition>();
        }

        private Dictionary<LicenseTier, List<ProductFeatureDefinition>> CreateDefaultFeatureTiers(ProductType productType)
        {
            var tiers = new Dictionary<LicenseTier, List<ProductFeatureDefinition>>();

            switch (productType)
            {
                case ProductType.ApiGateway:
                    tiers[LicenseTier.Community] = new List<ProductFeatureDefinition>
                    {
                        new ProductFeatureDefinition { FeatureId = "BasicRouting", Name = "Basic Routing", Category = FeatureCategory.Core }
                    };
                    tiers[LicenseTier.Professional] = new List<ProductFeatureDefinition>
                    {
                        new ProductFeatureDefinition { FeatureId = "BasicRouting", Name = "Basic Routing", Category = FeatureCategory.Core },
                        new ProductFeatureDefinition { FeatureId = "LoadBalancing", Name = "Load Balancing", Category = FeatureCategory.Performance },
                        new ProductFeatureDefinition { FeatureId = "BasicAuth", Name = "Basic Authentication", Category = FeatureCategory.Security }
                    };
                    tiers[LicenseTier.Enterprise] = new List<ProductFeatureDefinition>
                    {
                        new ProductFeatureDefinition { FeatureId = "BasicRouting", Name = "Basic Routing", Category = FeatureCategory.Core },
                        new ProductFeatureDefinition { FeatureId = "LoadBalancing", Name = "Load Balancing", Category = FeatureCategory.Performance },
                        new ProductFeatureDefinition { FeatureId = "BasicAuth", Name = "Basic Authentication", Category = FeatureCategory.Security },
                        new ProductFeatureDefinition { FeatureId = "AdvancedAuth", Name = "Advanced Authentication", Category = FeatureCategory.Security },
                        new ProductFeatureDefinition { FeatureId = "Monitoring", Name = "Advanced Monitoring", Category = FeatureCategory.Monitoring },
                        new ProductFeatureDefinition { FeatureId = "Analytics", Name = "Analytics & Reporting", Category = FeatureCategory.BusinessIntelligence }
                    };
                    break;

                case ProductType.WebApplication:
                    tiers[LicenseTier.Community] = new List<ProductFeatureDefinition>
                    {
                        new ProductFeatureDefinition { FeatureId = "BasicUI", Name = "Basic User Interface", Category = FeatureCategory.Core }
                    };
                    tiers[LicenseTier.Professional] = new List<ProductFeatureDefinition>
                    {
                        new ProductFeatureDefinition { FeatureId = "BasicUI", Name = "Basic User Interface", Category = FeatureCategory.Core },
                        new ProductFeatureDefinition { FeatureId = "UserMgmt", Name = "User Management", Category = FeatureCategory.Management },
                        new ProductFeatureDefinition { FeatureId = "Reporting", Name = "Basic Reporting", Category = FeatureCategory.BusinessIntelligence }
                    };
                    tiers[LicenseTier.Enterprise] = new List<ProductFeatureDefinition>
                    {
                        new ProductFeatureDefinition { FeatureId = "BasicUI", Name = "Basic User Interface", Category = FeatureCategory.Core },
                        new ProductFeatureDefinition { FeatureId = "UserMgmt", Name = "User Management", Category = FeatureCategory.Management },
                        new ProductFeatureDefinition { FeatureId = "Reporting", Name = "Basic Reporting", Category = FeatureCategory.BusinessIntelligence },
                        new ProductFeatureDefinition { FeatureId = "AdvancedReporting", Name = "Advanced Analytics", Category = FeatureCategory.BusinessIntelligence },
                        new ProductFeatureDefinition { FeatureId = "APIAccess", Name = "API Access", Category = FeatureCategory.Integration }
                    };
                    break;

                default:
                    // Default feature set for other products
                    tiers[LicenseTier.Community] = new List<ProductFeatureDefinition>
                    {
                        new ProductFeatureDefinition { FeatureId = "BasicFeatures", Name = "Basic Features", Category = FeatureCategory.Core }
                    };
                    tiers[LicenseTier.Professional] = new List<ProductFeatureDefinition>
                    {
                        new ProductFeatureDefinition { FeatureId = "BasicFeatures", Name = "Basic Features", Category = FeatureCategory.Core },
                        new ProductFeatureDefinition { FeatureId = "AdvancedFeatures", Name = "Advanced Features", Category = FeatureCategory.Management }
                    };
                    tiers[LicenseTier.Enterprise] = new List<ProductFeatureDefinition>
                    {
                        new ProductFeatureDefinition { FeatureId = "BasicFeatures", Name = "Basic Features", Category = FeatureCategory.Core },
                        new ProductFeatureDefinition { FeatureId = "AdvancedFeatures", Name = "Advanced Features", Category = FeatureCategory.Management },
                        new ProductFeatureDefinition { FeatureId = "EnterpriseFeatures", Name = "Enterprise Features", Category = FeatureCategory.BusinessIntelligence }
                    };
                    break;
            }

            return tiers;
        }

        private List<ProductFeatureDefinition> CreateDefaultFeatures(ProductType productType)
        {
            // Return all available features across all tiers
            var allFeatures = new List<ProductFeatureDefinition>();
            var tiers = CreateDefaultFeatureTiers(productType);
            
            foreach (var tierFeatures in tiers.Values)
            {
                foreach (var feature in tierFeatures)
                {
                    if (!allFeatures.Any(f => f.FeatureId == feature.FeatureId))
                        allFeatures.Add(feature);
                }
            }
            
            return allFeatures;
        }

        private Dictionary<string, object> CreateDefaultLimitations(ProductType productType)
        {
            return productType switch
            {
                ProductType.ApiGateway => new Dictionary<string, object>
                {
                    ["MaxApiCallsPerMonth"] = 10000,
                    ["MaxConcurrentConnections"] = 100,
                    ["RateLimitPerSecond"] = 10
                },
                ProductType.WebApplication => new Dictionary<string, object>
                {
                    ["MaxUsers"] = 100,
                    ["MaxDataStorageGB"] = 10,
                    ["MaxReportsPerMonth"] = 50
                },
                _ => new Dictionary<string, object>
                {
                    ["MaxUsers"] = 10,
                    ["MaxOperationsPerMonth"] = 1000
                }
            };
        }
    }
}
