using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Web.ViewModels.Product;
using TechWayFit.Licensing.Management.Web.Helpers;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace TechWayFit.Licensing.Management.Web.Controllers
{
    /// <summary>
    /// Product Management Controller - Step 3 Implementation
    /// Handles product CRUD operations, tier management, and feature configuration
    /// </summary>
    [Authorize]
    public class ProductController : Controller
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IEnterpriseProductService _productService;
        private readonly IProductLicenseService _licenseService;
        private readonly IConsumerAccountService _consumerService;

        public ProductController(
            ILogger<ProductController> logger,
            IEnterpriseProductService productService,
            IProductLicenseService licenseService,
            IConsumerAccountService consumerService)
        {
            _logger = logger;
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _licenseService = licenseService ?? throw new ArgumentNullException(nameof(licenseService));
            _consumerService = consumerService ?? throw new ArgumentNullException(nameof(consumerService));
        }

        /// <summary>
        /// Product listing page with search and filtering
        /// </summary>
        public async Task<IActionResult> Index(string searchTerm = "", bool showInactive = false)
        {
            try
            {
                // Get products from service layer
                var status = showInactive ? (ProductStatus?)null : ProductStatus.Active;
                var enterpriseProducts = await _productService.GetProductsAsync(status, searchTerm);
                
                // Map to ProductConfiguration for view compatibility
                var products = enterpriseProducts.Select(ep => new ProductConfiguration
                {
                    ProductId = ep.ProductId.ConvertToString(),
                    ProductName = ep.Name,
                    Description = ep.Description,
                    Version = ep.Version,
                    ProductType = ProductType.ApiGateway, // Default mapping - can be enhanced later
                    IsActive = ep.Status == ProductStatus.Active,
                    CreatedAt = ep.ReleaseDate,
                    UpdatedAt = ep.ReleaseDate
                }).ToList();

                var viewModel = new ProductListViewModel
                {
                    Products = (await Task.WhenAll(products.Select(async p => new ProductItemViewModel
                    {
                        ProductId = p.ProductId,
                        ProductName = p.ProductName,
                        ProductType = p.ProductType,
                        Version = p.Version,
                        IsActive = p.IsActive,
                        LicenseCount = await GetLicenseCountAsync(p.ProductId.ToGuid()),
                        ConsumerCount = await GetConsumerCountAsync(p.ProductId.ToGuid()),
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    }))).ToList(),
                    SearchTerm = searchTerm,
                    ShowInactiveProducts = showInactive
                };

                ViewBag.TotalProducts = products.Count;
                ViewBag.ActiveProducts = products.Count(p => p.IsActive);
                ViewBag.InactiveProducts = products.Count(p => !p.IsActive);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products list");
                TempData["ErrorMessage"] = "Error loading products. Please try again.";
                return View(new ProductListViewModel());
            }
        }

        /// <summary>
        /// Product details view
        /// </summary>
        /// <param name="id">Product ID</param>
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            try
            {
                var product = await GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                var viewModel = new ProductDetailViewModel
                {
                    Product = product,
                    Consumers = await GetProductConsumersAsync(id),
                    RecentLicenses = await GetRecentLicensesAsync(id),
                    Statistics = await GetProductStatisticsAsync(id)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product details for {ProductId}", id);
                TempData["ErrorMessage"] = "Error loading product details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Create new product form
        /// </summary>
        public IActionResult Create()
        {
            var viewModel = new ProductEditViewModel
            {
                IsActive = true,
                Version = "1.0.0",
                FeatureTiers = InitializeFeatureTiers(),
                AvailableFeatures = GetDefaultFeatures()
            };

            ViewBag.ProductTypes = Enum.GetValues<ProductType>()
                .Select(pt => new { Value = (int)pt, Text = pt.ToString() })
                .ToList();

            return View(viewModel);
        }

        /// <summary>
        /// Create new product - POST action
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ProductTypes = Enum.GetValues<ProductType>()
                    .Select(pt => new { Value = (int)pt, Text = pt.ToString() })
                    .ToList();
                return View(model);
            }

            try
            {
                // Check if product ID already exists
                var existingProduct = await GetProductByIdAsync(model.ProductId.ToGuid());
                if (existingProduct != null)
                {
                    ModelState.AddModelError("ProductId", "A product with this ID already exists.");
                    ViewBag.ProductTypes = Enum.GetValues<ProductType>()
                        .Select(pt => new { Value = (int)pt, Text = pt.ToString() })
                        .ToList();
                    return View(model);
                }

                var productConfig = new ProductConfiguration
                {
                    ProductId = model.ProductId.ToString(),
                    ProductName = model.ProductName,
                    Description = model.Description ?? "",
                    ProductType = model.ProductType,
                    Version = model.Version,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = User.Identity?.Name ?? "system",
                    FeatureTiers = ConvertFeatureTiers(model.FeatureTiers),
                    AvailableFeatures = ConvertFeatureDefinitions(model.AvailableFeatures),
                    DefaultLimitations = model.DefaultLimitations.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value),
                    Metadata = model.Metadata
                };

                await SaveProductAsync(productConfig);

                TempData["SuccessMessage"] = $"Product '{model.ProductName}' created successfully!";
                return RedirectToAction(nameof(Details), new { id = model.ProductId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product {ProductId}", model.ProductId);
                ModelState.AddModelError("", "Error creating product. Please try again.");
                ViewBag.ProductTypes = Enum.GetValues<ProductType>()
                    .Select(pt => new { Value = (int)pt, Text = pt.ToString() })
                    .ToList();
                return View(model);
            }
        }

        /// <summary>
        /// Edit existing product form
        /// </summary>
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            try
            {
                var product = await GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                var viewModel = new ProductEditViewModel
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Description = product.Description,
                    ProductType = product.ProductType,
                    Version = product.Version,
                    IsActive = product.IsActive,
                    FeatureTiers = ConvertToFeatureTiers(product.FeatureTiers),
                    AvailableFeatures = ConvertToFeatureDefinitions(product.AvailableFeatures),
                    DefaultLimitations = product.DefaultLimitations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString() ?? ""),
                    Metadata = product.Metadata
                };

                ViewBag.ProductTypes = Enum.GetValues<ProductType>()
                    .Select(pt => new { Value = (int)pt, Text = pt.ToString() })
                    .ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product for edit {ProductId}", id);
                TempData["ErrorMessage"] = "Error loading product for editing. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Update existing product - POST action
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ProductTypes = Enum.GetValues<ProductType>()
                    .Select(pt => new { Value = (int)pt, Text = pt.ToString() })
                    .ToList();
                return View(model);
            }

            try
            {
                var existingProduct = await GetProductByIdAsync(model.ProductId.ToGuid());
                if (existingProduct == null)
                {
                    return NotFound();
                }

                existingProduct.ProductName = model.ProductName;
                existingProduct.Description = model.Description ?? "";
                existingProduct.ProductType = model.ProductType;
                existingProduct.Version = model.Version;
                existingProduct.IsActive = model.IsActive;
                existingProduct.UpdatedAt = DateTime.UtcNow;
                existingProduct.FeatureTiers = ConvertFeatureTiers(model.FeatureTiers);
                existingProduct.AvailableFeatures = ConvertFeatureDefinitions(model.AvailableFeatures);
                existingProduct.DefaultLimitations = model.DefaultLimitations.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
                existingProduct.Metadata = model.Metadata;

                await SaveProductAsync(existingProduct);

                TempData["SuccessMessage"] = $"Product '{model.ProductName}' updated successfully!";
                return RedirectToAction(nameof(Details), new { id = model.ProductId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", model.ProductId);
                ModelState.AddModelError("", "Error updating product. Please try again.");
                ViewBag.ProductTypes = Enum.GetValues<ProductType>()
                    .Select(pt => new { Value = (int)pt, Text = pt.ToString() })
                    .ToList();
                return View(model);
            }
        }

        /// <summary>
        /// Delete product confirmation
        /// </summary>
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            try
            {
                var product = await GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                var viewModel = new ProductDetailViewModel
                {
                    Product = product,
                    Statistics = await GetProductStatisticsAsync(id)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product for deletion {ProductId}", id);
                TempData["ErrorMessage"] = "Error loading product for deletion. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Delete product - POST action
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var product = await GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                // Check if product has active licenses
                var licenseCount = await GetLicenseCountAsync(id);
                if (licenseCount > 0)
                {
                    TempData["ErrorMessage"] = $"Cannot delete product '{product.ProductName}' because it has {licenseCount} active licenses. Please revoke all licenses first.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                await DeleteProductAsync(id);

                TempData["SuccessMessage"] = $"Product '{product.ProductName}' deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                TempData["ErrorMessage"] = "Error deleting product. Please try again.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        #region Private Helper Methods

        private async Task<List<ProductConfiguration>> GetProductsAsync()
        {
            var enterpriseProducts = await _productService.GetProductsAsync();
            return enterpriseProducts.Select(ep => new ProductConfiguration
            {
                ProductId = ep.ProductId.ToString(),
                ProductName = ep.Name,
                Description = ep.Description,
                Version = ep.Version,
                ProductType = ProductType.ApiGateway,
                IsActive = ep.Status == ProductStatus.Active,
                CreatedAt = ep.ReleaseDate,
                UpdatedAt = ep.ReleaseDate
            }).ToList();
        }

        private async Task<ProductConfiguration?> GetProductByIdAsync(Guid productId)
        {
            var enterpriseProduct = await _productService.GetProductByIdAsync(productId);
            if (enterpriseProduct == null) return null;

            return new ProductConfiguration
            {
                ProductId = enterpriseProduct.ProductId.ToString(),
                ProductName = enterpriseProduct.Name,
                Description = enterpriseProduct.Description,
                Version = enterpriseProduct.Version,
                ProductType = ProductType.ApiGateway,
                IsActive = enterpriseProduct.Status == ProductStatus.Active,
                CreatedAt = enterpriseProduct.ReleaseDate,
                UpdatedAt = enterpriseProduct.ReleaseDate
            };
        }

        private async Task SaveProductAsync(ProductConfiguration product)
        {
            var enterpriseProduct = new EnterpriseProduct
            {
                ProductId = Guid.Parse(product.ProductId),
                Name = product.ProductName,
                Description = product.Description,
                Version = product.Version,
                Status = product.IsActive ? ProductStatus.Active : ProductStatus.Inactive,
                ReleaseDate = product.CreatedAt
            };

            if (await _productService.GetProductByIdAsync(Guid.Parse(product.ProductId)) != null)
            {
                await _productService.UpdateProductAsync(enterpriseProduct, "system");
            }
            else
            {
                await _productService.CreateProductAsync(enterpriseProduct, "system");
            }
        }

        private async Task DeleteProductAsync(Guid productId)
        {
            await _productService.DeleteProductAsync(productId, "system");
        }

        private async Task<int> GetLicenseCountAsync(Guid productId)
        {
            var licenses = await _licenseService.GetLicensesByProductAsync(productId);
            // TODO: Implement actual license counting logic
            // For now, return a random number for demonstration
            return licenses.Count();
        }

        private async Task<int> GetConsumerCountAsync(Guid productId)
        {
            var consumers = await _consumerService.GetConsumersByProductAsync(productId);
            // TODO: Implement actual consumer counting logic
            // For now, return a random number for demonstration
            return consumers.Count();
        }

        private async Task<List<ConsumerSummaryViewModel>> GetProductConsumersAsync(Guid productId)
        {
            // TODO: Implement actual consumer retrieval logic
            // For now, return sample data
            return new List<ConsumerSummaryViewModel>
            {
                new ConsumerSummaryViewModel
                {
                    ConsumerId = Guid.NewGuid().ConvertToString(),
                    OrganizationName = "Demo Organization",
                    ContactPerson = "John Doe",
                    ContactEmail = "john@demo.com",
                    LicenseCount = 2,
                    LastLicenseDate = DateTime.UtcNow.AddDays(-30),
                    IsActive = true
                }
            };
        }

        private async Task<List<LicenseSummaryViewModel>> GetRecentLicensesAsync(Guid productId)
        {
            // TODO: Implement actual license retrieval logic
            // For now, return sample data
            return new List<LicenseSummaryViewModel>
            {
                new LicenseSummaryViewModel
                {
                    LicenseId = Guid.NewGuid().ConvertToString(),
                    ConsumerName = "Demo Organization",
                    Tier = LicenseTier.Professional,
                    Status = LicenseStatus.Active,
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    ExpiresAt = DateTime.UtcNow.AddDays(345)
                }
            };
        }

        private async Task<ProductStatisticsViewModel> GetProductStatisticsAsync(Guid productId)
        {
            // TODO: Implement actual statistics calculation
            // For now, return sample data
            return new ProductStatisticsViewModel
            {
                TotalLicenses = 5,
                ActiveLicenses = 4,
                ExpiredLicenses = 1,
                SuspendedLicenses = 0,
                RevokedLicenses = 0,
                LicensesExpiringSoon = 1,
                LicensesByTier = new Dictionary<LicenseTier, int>
                {
                    { LicenseTier.Community, 1 },
                    { LicenseTier.Professional, 2 },
                    { LicenseTier.Enterprise, 2 }
                },
                FeatureUsage = new Dictionary<string, int>
                {
                    { "API Gateway", 4 },
                    { "Rate Limiting", 3 },
                    { "Authentication", 4 },
                    { "Analytics", 2 }
                }
            };
        }

        private Dictionary<LicenseTier, List<FeatureDefinitionViewModel>> InitializeFeatureTiers()
        {
            return new Dictionary<LicenseTier, List<FeatureDefinitionViewModel>>
            {
                {
                    LicenseTier.Community,
                    new List<FeatureDefinitionViewModel>
                    {
                        new FeatureDefinitionViewModel
                        {
                            FeatureId = Guid.NewGuid().ConvertToString(),
                            Name = "Basic API Gateway",
                            Description = "Basic API routing and proxy functionality",
                            Category = FeatureCategory.Core,
                            IsEnabledByDefault = true,
                            MinimumTier = LicenseTier.Community
                        }
                    }
                },
                {
                    LicenseTier.Professional,
                    new List<FeatureDefinitionViewModel>
                    {
                        new FeatureDefinitionViewModel
                        {
                            FeatureId = Guid.NewGuid().ConvertToString(),
                            Name = "Rate Limiting",
                            Description = "Advanced rate limiting and throttling",
                            Category = FeatureCategory.Performance,
                            IsEnabledByDefault = true,
                            MinimumTier = LicenseTier.Professional
                        }
                    }
                },
                {
                    LicenseTier.Enterprise,
                    new List<FeatureDefinitionViewModel>
                    {
                        new FeatureDefinitionViewModel
                        {
                            FeatureId = Guid.NewGuid().ConvertToString(),
                            Name = "Advanced Analytics",
                            Description = "Comprehensive analytics and reporting",
                            Category = FeatureCategory.BusinessIntelligence,
                            IsEnabledByDefault = true,
                            MinimumTier = LicenseTier.Enterprise
                        }
                    }
                }
            };
        }

        private List<FeatureDefinitionViewModel> GetDefaultFeatures()
        {
            return new List<FeatureDefinitionViewModel>
            {
                new FeatureDefinitionViewModel
                {
                    FeatureId = Guid.NewGuid().ConvertToString(),
                    Name = "Authentication",
                    Description = "User authentication and authorization",
                    Category = FeatureCategory.Security,
                    IsEnabledByDefault = true,
                    MinimumTier = LicenseTier.Community
                },
                new FeatureDefinitionViewModel
                {
                    FeatureId = Guid.NewGuid().ConvertToString(),
                    Name = "Request Logging",
                    Description = "Comprehensive request and response logging",
                    Category = FeatureCategory.Core,
                    IsEnabledByDefault = true,
                    MinimumTier = LicenseTier.Community
                }
            };
        }

        private Dictionary<LicenseTier, List<ProductFeatureDefinition>> ConvertFeatureTiers(Dictionary<LicenseTier, List<FeatureDefinitionViewModel>> featureTiers)
        {
            return featureTiers.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Select(f => new ProductFeatureDefinition
                {
                    FeatureId = f.FeatureId.ToString(),
                    Name = f.Name,
                    Description = f.Description,
                    Category = f.Category,
                    IsEnabledByDefault = f.IsEnabledByDefault,
                    MinimumTier = f.MinimumTier,
                    Limitations = f.Limitations.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value),
                    Dependencies = f.Dependencies
                }).ToList()
            );
        }

        private List<ProductFeatureDefinition> ConvertFeatureDefinitions(List<FeatureDefinitionViewModel> features)
        {
            return features.Select(f => new ProductFeatureDefinition
            {
                FeatureId = f.FeatureId.ToString(),
                Name = f.Name,
                Description = f.Description,
                Category = f.Category,
                IsEnabledByDefault = f.IsEnabledByDefault,
                MinimumTier = f.MinimumTier,
                Limitations = f.Limitations.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value),
                Dependencies = f.Dependencies
            }).ToList();
        }

        private Dictionary<LicenseTier, List<FeatureDefinitionViewModel>> ConvertToFeatureTiers(Dictionary<LicenseTier, List<ProductFeatureDefinition>> featureTiers)
        {
            return featureTiers.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Select(f => new FeatureDefinitionViewModel
                {
                    FeatureId = f.FeatureId,
                    Name = f.Name,
                    Description = f.Description,
                    Category = f.Category,
                    IsEnabledByDefault = f.IsEnabledByDefault,
                    MinimumTier = f.MinimumTier,
                    Limitations = f.Limitations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? ""),
                    Dependencies = f.Dependencies
                }).ToList()
            );
        }

        private List<FeatureDefinitionViewModel> ConvertToFeatureDefinitions(List<ProductFeatureDefinition> features)
        {
            return features.Select(f => new FeatureDefinitionViewModel
            {
                FeatureId = f.FeatureId,
                Name = f.Name,
                Description = f.Description,
                Category = f.Category,
                IsEnabledByDefault = f.IsEnabledByDefault,
                MinimumTier = f.MinimumTier,
                Limitations = f.Limitations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? ""),
                Dependencies = f.Dependencies
            }).ToList();
        }

        #endregion
    }
}
