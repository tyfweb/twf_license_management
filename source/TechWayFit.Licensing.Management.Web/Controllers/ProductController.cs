using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Web.ViewModels.Product;
using TechWayFit.Licensing.Management.Web.Helpers;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace TechWayFit.Licensing.Management.Web.Controllers
{
    /// <summary>
    /// Enhanced Product Management Controller
    /// Handles product CRUD operations with comprehensive tier, version, and feature management
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
                .Select(pt => new SelectListItem { Value = ((int)pt).ToString(), Text = pt.ToString() })
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
                        .Select(pt => new SelectListItem { Value = ((int)pt).ToString(), Text = pt.ToString() })
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
                    .Select(pt => new SelectListItem { Value = ((int)pt).ToString(), Text = pt.ToString() })
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
                    .Select(pt => new SelectListItem { Value = ((int)pt).ToString(), Text = pt.ToString() })
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
                    .Select(pt => new SelectListItem { Value = ((int)pt).ToString(), Text = pt.ToString() })
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
                    .Select(pt => new SelectListItem { Value = ((int)pt).ToString(), Text = pt.ToString() })
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

        #region Enhanced Product Management - Simple Approach

        /// <summary>
        /// Enhanced Edit view with tabbed sections for Features, Tiers, and Versions
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditEnhanced(Guid id)
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

                // Convert to enhanced edit view model with tabs
                var viewModel = CreateEnhancedEditViewModel(product);
                viewModel.ActiveSection = "basic";

                SetupViewBagForEnhanced();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading enhanced product edit for {ProductId}", id);
                TempData["ErrorMessage"] = "Error loading product for editing. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Enhanced Product Management - Tiers Section
        /// </summary>
        [HttpGet]
        [Route("Product/EditEnhanced/{id:guid}/tiers")]
        public async Task<IActionResult> EditEnhancedTiers(Guid id)
        {
            return RedirectToAction("Tiers", new { id });
        }

        /// <summary>
        /// Enhanced Product Management - Versions Section
        /// </summary>
        [HttpGet]
        [Route("Product/EditEnhanced/{id:guid}/versions")]
        public async Task<IActionResult> EditEnhancedVersions(Guid id)
        {
            return RedirectToAction("Versions", new { id });
        }

        /// <summary>
        /// Enhanced Product Management - Features Section
        /// </summary>
        [HttpGet]
        [Route("Product/EditEnhanced/{id:guid}/features")]
        public async Task<IActionResult> EditEnhancedFeatures(Guid id)
        {
            return RedirectToAction("Features", new { id });
        }

        /// <summary>
        /// Dedicated Product Tiers management page
        /// </summary>
        [Route("Product/{id:guid}/tiers")]
        public async Task<IActionResult> Tiers(Guid id)
        {
            try
            {
                var product = await GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                var viewModel = CreateEnhancedEditViewModel(product);
                viewModel.ActiveSection = "tiers";

                return View("Tiers", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product tiers page for {ProductId}", id);
                TempData["ErrorMessage"] = "Error loading product tiers. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Dedicated Product Versions management page
        /// </summary>
        [Route("Product/{id:guid}/versions")]
        public async Task<IActionResult> Versions(Guid id)
        {
            try
            {
                var product = await GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                var viewModel = CreateEnhancedEditViewModel(product);
                viewModel.ActiveSection = "versions";

                return View("Versions", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product versions page for {ProductId}", id);
                TempData["ErrorMessage"] = "Error loading product versions. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Dedicated Product Features management page
        /// </summary>
        [Route("Product/{id:guid}/features")]
        public async Task<IActionResult> Features(Guid id)
        {
            try
            {
                var product = await GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                var viewModel = CreateEnhancedEditViewModel(product);
                viewModel.ActiveSection = "features";

                return View("Features", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product features page for {ProductId}", id);
                TempData["ErrorMessage"] = "Error loading product features. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Helper method to create enhanced edit view model
        /// </summary>
        private ProductEnhancedEditViewModel CreateEnhancedEditViewModel(ProductConfiguration product)
        {
            return new ProductEnhancedEditViewModel
            {
                // Basic product info
                ProductId = Guid.TryParse(product.ProductId, out var productGuid) ? productGuid : Guid.Empty,
                ProductName = product.ProductName,
                Description = product.Description,
                ProductType = product.ProductType,
                Version = product.Version,
                IsActive = product.IsActive,
                
                // Enhanced sections
                HasTiers = true,
                HasVersions = true,
                HasFeatures = true,
                ActiveSection = "basic" // Default, will be overridden
            };
        }

        /// <summary>
        /// Helper method to setup ViewBag for enhanced edit
        /// </summary>
        private void SetupViewBagForEnhanced()
        {
            ViewBag.ProductTypes = Enum.GetValues<ProductType>()
                .Select(pt => new SelectListItem { Value = ((int)pt).ToString(), Text = pt.ToString() })
                .ToList();

            ViewBag.LicenseTiers = Enum.GetValues<LicenseTier>()
                .Select(lt => new SelectListItem { Value = ((int)lt).ToString(), Text = lt.ToString() })
                .ToList();

            ViewBag.FeatureCategories = Enum.GetValues<FeatureCategory>()
                .Select(fc => new SelectListItem { Value = ((int)fc).ToString(), Text = fc.ToString() })
                .ToList();
        }

        /// <summary>
        /// Get product tiers for AJAX loading
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProductTiers(Guid productId)
        {
            try
            {
                // This would load from your actual data service
                var tiers = await GetProductTiersDataAsync(productId);
                return Json(new { success = true, data = tiers });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product tiers for {ProductId}", productId);
                return Json(new { success = false, message = "Error loading tiers" });
            }
        }

        /// <summary>
        /// Add a new product tier
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProductTier(Guid productId, ProductTierViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return Json(new { success = false, message = "Validation failed", errors });
                }

                // Simulate adding a product tier
                await Task.CompletedTask; // For now, as this is mock implementation
                
                var newTier = new 
                {
                    Id = Guid.NewGuid(),
                    Name = model.TierName,
                    Description = model.Description,
                    Price = model.MonthlyPrice.HasValue ? $"USD {model.MonthlyPrice:F2}/month" : "Free",
                    IsActive = model.IsActive,
                    CanDelete = true
                };

                _logger.LogInformation("Product tier '{TierName}' added to product {ProductId}", model.TierName, productId);
                
                return Json(new { 
                    success = true, 
                    message = "Product tier added successfully", 
                    data = newTier 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product tier for product {ProductId}", productId);
                return Json(new { success = false, message = "Error adding product tier" });
            }
        }

        /// <summary>
        /// Edit an existing product tier
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProductTier(Guid productId, Guid tierId, ProductTierViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return Json(new { success = false, message = "Validation failed", errors });
                }

                // Simulate updating a product tier
                await Task.CompletedTask; // For now, as this is mock implementation
                
                var updatedTier = new 
                {
                    Id = tierId,
                    Name = model.TierName,
                    Description = model.Description,
                    Price = model.MonthlyPrice.HasValue ? $"USD {model.MonthlyPrice:F2}/month" : "Free",
                    IsActive = model.IsActive,
                    CanDelete = true
                };

                _logger.LogInformation("Product tier '{TierName}' (ID: {TierId}) updated for product {ProductId}", 
                    model.TierName, tierId, productId);
                
                return Json(new { 
                    success = true, 
                    message = "Product tier updated successfully", 
                    data = updatedTier 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product tier {TierId} for product {ProductId}", tierId, productId);
                return Json(new { success = false, message = "Error updating product tier" });
            }
        }

        /// <summary>
        /// Delete a product tier
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProductTier(Guid productId, Guid tierId)
        {
            try
            {
                // Simulate tier validation and deletion
                await Task.CompletedTask; // For now, as this is mock implementation
                
                // In a real implementation, you would check if the tier can be deleted
                // (e.g., no active licenses, not a system tier, etc.)
                
                _logger.LogInformation("Product tier {TierId} deleted from product {ProductId}", tierId, productId);
                
                return Json(new { 
                    success = true, 
                    message = "Product tier deleted successfully" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product tier {TierId} for product {ProductId}", tierId, productId);
                return Json(new { success = false, message = "Error deleting product tier" });
            }
        }

        /// <summary>
        /// Get a specific product tier for editing
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProductTier(Guid productId, Guid tierId)
        {
            try
            {
                // Simulate getting a specific tier
                await Task.CompletedTask; // For now, as this is mock implementation
                
                // In a real implementation, this would fetch from the database
                var tier = new ProductTierViewModel
                {
                    TierId = tierId,
                    ProductId = productId,
                    TierName = "Professional", // Mock data
                    Description = "Professional tier with advanced features",
                    MonthlyPrice = 29.99m,
                    AnnualPrice = 299.99m,
                    IsActive = true,
                    IsFree = false,
                    TrialPeriodDays = 14,
                    MaxUsers = 10,
                    MaxProjects = 50
                };

                return Json(new { success = true, data = tier });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product tier {TierId} for product {ProductId}", tierId, productId);
                return Json(new { success = false, message = "Error loading product tier" });
            }
        }

        /// <summary>
        /// Get product versions for AJAX loading
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProductVersions(Guid productId)
        {
            try
            {
                // This would load from your actual data service
                var versions = await GetProductVersionsDataAsync(productId);
                return Json(new { success = true, data = versions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product versions for {ProductId}", productId);
                return Json(new { success = false, message = "Error loading versions" });
            }
        }

        /// <summary>
        /// Add a new product version
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProductVersion(Guid productId, ProductVersionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return Json(new { success = false, message = "Validation failed", errors });
                }

                // Simulate adding a product version
                await Task.CompletedTask; // For now, as this is mock implementation
                
                var newVersion = new 
                {
                    Id = Guid.NewGuid(),
                    VersionNumber = model.VersionNumber,
                    VersionName = model.VersionName,
                    ReleaseDate = model.ReleaseDate,
                    Description = model.Description,
                    IsStable = model.IsStable,
                    IsActive = model.IsActive,
                    IsApproved = model.IsApproved,
                    CanDelete = true
                };

                _logger.LogInformation("Product version '{VersionNumber}' added to product {ProductId}", model.VersionNumber, productId);
                
                return Json(new { 
                    success = true, 
                    message = "Product version added successfully", 
                    data = newVersion 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product version for product {ProductId}", productId);
                return Json(new { success = false, message = "Error adding product version" });
            }
        }

        /// <summary>
        /// Edit an existing product version
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProductVersion(Guid productId, Guid versionId, ProductVersionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return Json(new { success = false, message = "Validation failed", errors });
                }

                // Simulate updating a product version
                await Task.CompletedTask; // For now, as this is mock implementation
                
                var updatedVersion = new 
                {
                    Id = versionId,
                    VersionNumber = model.VersionNumber,
                    VersionName = model.VersionName,
                    ReleaseDate = model.ReleaseDate,
                    Description = model.Description,
                    IsStable = model.IsStable,
                    IsActive = model.IsActive,
                    IsApproved = model.IsApproved,
                    CanDelete = true
                };

                _logger.LogInformation("Product version '{VersionNumber}' (ID: {VersionId}) updated for product {ProductId}", 
                    model.VersionNumber, versionId, productId);
                
                return Json(new { 
                    success = true, 
                    message = "Product version updated successfully", 
                    data = updatedVersion 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product version {VersionId} for product {ProductId}", versionId, productId);
                return Json(new { success = false, message = "Error updating product version" });
            }
        }

        /// <summary>
        /// Delete a product version
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProductVersion(Guid productId, Guid versionId)
        {
            try
            {
                // Simulate version validation and deletion
                await Task.CompletedTask; // For now, as this is mock implementation
                
                _logger.LogInformation("Product version {VersionId} deleted from product {ProductId}", versionId, productId);
                
                return Json(new { 
                    success = true, 
                    message = "Product version deleted successfully" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product version {VersionId} for product {ProductId}", versionId, productId);
                return Json(new { success = false, message = "Error deleting product version" });
            }
        }

        /// <summary>
        /// Get a specific product version for editing
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProductVersion(Guid productId, Guid versionId)
        {
            try
            {
                // Simulate getting a specific version
                await Task.CompletedTask; // For now, as this is mock implementation
                
                var version = new ProductVersionViewModel
                {
                    VersionId = versionId,
                    ProductId = productId,
                    VersionNumber = "1.2.0", // Mock data
                    VersionName = "Feature Update",
                    ReleaseDate = DateTime.UtcNow.AddDays(-10),
                    Description = "Added new features and bug fixes",
                    IsStable = true,
                    IsActive = true,
                    IsApproved = true
                };

                return Json(new { success = true, data = version });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product version {VersionId} for product {ProductId}", versionId, productId);
                return Json(new { success = false, message = "Error loading product version" });
            }
        }

        /// <summary>
        /// Get product features for AJAX loading
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProductFeatures(Guid productId)
        {
            try
            {
                var features = await GetProductFeaturesDataAsync(productId);
                return Json(new { success = true, data = features });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product features for {ProductId}", productId);
                return Json(new { success = false, message = "Error loading features" });
            }
        }

        /// <summary>
        /// Add a new product feature
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProductFeature(Guid productId, ProductFeatureViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return Json(new { success = false, message = "Validation failed", errors });
                }

                // Simulate adding a product feature
                await Task.CompletedTask; // For now, as this is mock implementation
                
                var newFeature = new 
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    Description = model.Description,
                    Category = model.Category,
                    IsEnabled = model.IsEnabled,
                    MaxUsage = model.MaxUsage,
                    MinimumTier = model.MinimumTier,
                    IsActive = model.IsActive,
                    CanDelete = true
                };

                _logger.LogInformation("Product feature '{FeatureName}' added to product {ProductId}", model.Name, productId);
                
                return Json(new { 
                    success = true, 
                    message = "Product feature added successfully", 
                    data = newFeature 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product feature for product {ProductId}", productId);
                return Json(new { success = false, message = "Error adding product feature" });
            }
        }

        /// <summary>
        /// Edit an existing product feature
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProductFeature(Guid productId, Guid featureId, ProductFeatureViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return Json(new { success = false, message = "Validation failed", errors });
                }

                // Simulate updating a product feature
                await Task.CompletedTask; // For now, as this is mock implementation
                
                var updatedFeature = new 
                {
                    Id = featureId,
                    Name = model.Name,
                    Description = model.Description,
                    Category = model.Category,
                    IsEnabled = model.IsEnabled,
                    MaxUsage = model.MaxUsage,
                    MinimumTier = model.MinimumTier,
                    IsActive = model.IsActive,
                    CanDelete = true
                };

                _logger.LogInformation("Product feature '{FeatureName}' (ID: {FeatureId}) updated for product {ProductId}", 
                    model.Name, featureId, productId);
                
                return Json(new { 
                    success = true, 
                    message = "Product feature updated successfully", 
                    data = updatedFeature 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product feature {FeatureId} for product {ProductId}", featureId, productId);
                return Json(new { success = false, message = "Error updating product feature" });
            }
        }

        /// <summary>
        /// Delete a product feature
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProductFeature(Guid productId, Guid featureId)
        {
            try
            {
                // Simulate feature validation and deletion
                await Task.CompletedTask; // For now, as this is mock implementation
                
                _logger.LogInformation("Product feature {FeatureId} deleted from product {ProductId}", featureId, productId);
                
                return Json(new { 
                    success = true, 
                    message = "Product feature deleted successfully" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product feature {FeatureId} for product {ProductId}", featureId, productId);
                return Json(new { success = false, message = "Error deleting product feature" });
            }
        }

        /// <summary>
        /// Get a specific product feature for editing
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProductFeature(Guid productId, Guid featureId)
        {
            try
            {
                // Simulate getting a specific feature
                await Task.CompletedTask; // For now, as this is mock implementation
                
                var feature = new ProductFeatureViewModel
                {
                    FeatureId = featureId,
                    ProductId = productId,
                    Name = "Advanced Analytics", // Mock data
                    Description = "Comprehensive analytics and reporting capabilities",
                    Category = FeatureCategory.BusinessIntelligence,
                    IsEnabled = true,
                    MaxUsage = null,
                    MinimumTier = LicenseTier.Professional,
                    IsActive = true
                };

                return Json(new { success = true, data = feature });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product feature {FeatureId} for product {ProductId}", featureId, productId);
                return Json(new { success = false, message = "Error loading product feature" });
            }
        }

        #endregion

        #region Simple Data Access Methods for Enhanced Management

        /// <summary>
        /// Get product tiers data - simplified implementation
        /// </summary>
        private async Task<List<object>> GetProductTiersDataAsync(Guid productId)
        {
            // Simplified - return mock data for now
            // In real implementation, this would query your data layer
            await Task.CompletedTask;
            
            return new List<object>
            {
                new { 
                    Id = Guid.NewGuid(),
                    Name = "Community",
                    Description = "Free tier with basic features",
                    Price = "USD 0.00",
                    IsActive = true,
                    CanDelete = true
                },
                new { 
                    Id = Guid.NewGuid(),
                    Name = "Professional", 
                    Description = "Professional tier with advanced features",
                    Price = "USD 29.99/month",
                    IsActive = true,
                    CanDelete = true
                },
                new { 
                    Id = Guid.NewGuid(),
                    Name = "Enterprise", 
                    Description = "Enterprise tier with all features",
                    Price = "USD 99.99/month",
                    IsActive = true,
                    CanDelete = true
                }
            };
        }

        /// <summary>
        /// Get product versions data - simplified implementation
        /// </summary>
        private async Task<List<object>> GetProductVersionsDataAsync(Guid productId)
        {
            // Simplified - return mock data for now
            await Task.CompletedTask;
            
            return new List<object>
            {
                new { 
                    Id = Guid.NewGuid(),
                    Version = "1.0.0",
                    Name = "Initial Release",
                    ReleaseDate = DateTime.UtcNow.AddDays(-30),
                    IsActive = true,
                    IsApproved = true,
                    CanDelete = false // Approved versions cannot be deleted
                },
                new { 
                    Id = Guid.NewGuid(),
                    Version = "1.1.0",
                    Name = "Feature Update",
                    ReleaseDate = DateTime.UtcNow.AddDays(-10),
                    IsActive = true,
                    IsApproved = false,
                    CanDelete = true // Not approved yet, can be deleted
                }
            };
        }

        /// <summary>
        /// Get product features data - simplified implementation
        /// </summary>
        private async Task<List<object>> GetProductFeaturesDataAsync(Guid productId)
        {
            // Simplified - return mock data for now
            await Task.CompletedTask;
            
            return new List<object>
            {
                new { 
                    Id = Guid.NewGuid(),
                    Name = "Basic Functionality",
                    Description = "Core product functionality",
                    Category = "Core",
                    IsEnabled = true,
                    IsActive = true,
                    CanDelete = true
                },
                new { 
                    Id = Guid.NewGuid(),
                    Name = "Advanced Analytics",
                    Description = "Advanced analytics and reporting",
                    Category = "BusinessIntelligence",
                    IsEnabled = true,
                    IsActive = true,
                    CanDelete = true
                },
                new { 
                    Id = Guid.NewGuid(),
                    Name = "Priority Support",
                    Description = "24/7 priority customer support",
                    Category = "Core",
                    IsEnabled = true,
                    IsActive = true,
                    CanDelete = true
                }
            };
        }

        #endregion
    }
}
