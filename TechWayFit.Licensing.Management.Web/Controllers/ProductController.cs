using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TechWayFit.Licensing.Core.Contracts;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.WebUI.ViewModels.Product;

namespace TechWayFit.Licensing.WebUI.Controllers
{
    /// <summary>
    /// Controller for product management (placeholder implementation)
    /// </summary>
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IConsumerService _consumerService;
        private readonly ILicenseLifecycleService _licenseService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductService productService,
            IConsumerService consumerService,
            ILicenseLifecycleService licenseService,
            ILogger<ProductController> logger)
        {
            _productService = productService;
            _consumerService = consumerService;
            _licenseService = licenseService;
            _logger = logger;
        }

        /// <summary>
        /// List all products
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                var productItems = new List<ProductItemViewModel>();

                foreach (var product in products)
                {
                    var consumers = await _consumerService.GetConsumersByProductAsync(product.ProductId);
                    var licenses = await _licenseService.GetLicensesByProductAsync(product.ProductId);

                    productItems.Add(new ProductItemViewModel
                    {
                        ProductId = product.ProductId,
                        ProductName = product.ProductName,
                        ProductType = product.ProductType,
                        Version = product.Version,
                        IsActive = product.IsActive,
                        LicenseCount = licenses.Count(),
                        ConsumerCount = consumers.Count(),
                        CreatedAt = product.CreatedAt,
                        UpdatedAt = product.UpdatedAt
                    });
                }

                var viewModel = new ProductListViewModel
                {
                    Products = productItems
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products");
                TempData["Error"] = "Failed to load products";
                return View(new ProductListViewModel());
            }
        }

        /// <summary>
        /// Show product details
        /// </summary>
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                var product = await _productService.GetProductAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Product not found";
                    return NotFound();
                }

                var consumers = await _consumerService.GetConsumersByProductAsync(id);
                var licenses = await _licenseService.GetLicensesByProductAsync(id);

                // Build consumer summaries
                var consumerSummaries = new List<ConsumerSummaryViewModel>();
                foreach (var consumer in consumers)
                {
                    var consumerLicenses = licenses.Where(l => l.ConsumerId == consumer.ConsumerId).ToList();
                    var lastLicense = consumerLicenses.OrderByDescending(l => l.CreatedAt).FirstOrDefault();
                    
                    consumerSummaries.Add(new ConsumerSummaryViewModel
                    {
                        ConsumerId = consumer.ConsumerId,
                        OrganizationName = consumer.OrganizationName,
                        ContactPerson = consumer.ContactPerson,
                        ContactEmail = consumer.ContactEmail,
                        LicenseCount = consumerLicenses.Count,
                        LastLicenseDate = lastLicense?.CreatedAt ?? DateTime.MinValue,
                        LastLicenseStatus = lastLicense?.Status,
                        IsActive = consumer.IsActive
                    });
                }

                // Build recent licenses summary
                var recentLicenses = licenses.OrderByDescending(l => l.CreatedAt)
                    .Take(10)
                    .Select(l => new LicenseSummaryViewModel
                    {
                        LicenseId = l.LicenseId,
                        ConsumerName = consumers.FirstOrDefault(c => c.ConsumerId == l.ConsumerId)?.OrganizationName ?? "Unknown",
                        Tier = l.Tier,
                        Status = l.Status,
                        ValidFrom = l.ValidFrom,
                        ValidTo = l.ValidTo,
                        CreatedAt = l.CreatedAt,
                        DaysUntilExpiry = (int)(l.ValidTo - DateTime.UtcNow).TotalDays
                    }).ToList();

                // Build statistics
                var statistics = new ProductStatisticsViewModel
                {
                    TotalLicenses = licenses.Count(),
                    ActiveLicenses = licenses.Count(l => l.Status == LicenseStatus.Active && l.ValidTo > DateTime.UtcNow),
                    ExpiredLicenses = licenses.Count(l => l.ValidTo < DateTime.UtcNow),
                    SuspendedLicenses = licenses.Count(l => l.Status == LicenseStatus.Suspended),
                    RevokedLicenses = licenses.Count(l => l.Status == LicenseStatus.Revoked),
                    LicensesExpiringSoon = licenses.Count(l => l.Status == LicenseStatus.Active && l.ValidTo <= DateTime.UtcNow.AddDays(30) && l.ValidTo > DateTime.UtcNow),
                    LicensesByTier = licenses.GroupBy(l => l.Tier).ToDictionary(g => g.Key, g => g.Count())
                };

                var viewModel = new ProductDetailViewModel
                {
                    Product = product,
                    Consumers = consumerSummaries,
                    RecentLicenses = recentLicenses,
                    Statistics = statistics
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product {ProductId}", id);
                TempData["Error"] = "Failed to load product details";
                return NotFound();
            }
        }

        /// <summary>
        /// Create new product form
        /// </summary>
        public IActionResult Create()
        {
            var viewModel = new ProductEditViewModel();
            return View(viewModel);
        }

        /// <summary>
        /// Create new product
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var product = new ProductConfiguration
                {
                    ProductId = model.ProductId,
                    ProductName = model.ProductName,
                    ProductType = model.ProductType,
                    Version = model.Version,
                    IsActive = model.IsActive,
                    Metadata = model.Metadata ?? new Dictionary<string, string>(),
                    AvailableFeatures = model.AvailableFeatures?.Select(f => new ProductFeatureDefinition
                    {
                        FeatureId = f.FeatureId,
                        Name = f.Name,
                        Description = f.Description,
                        Category = f.Category,
                        IsEnabledByDefault = f.IsEnabledByDefault,
                        MinimumTier = f.MinimumTier
                    }).ToList() ?? new List<ProductFeatureDefinition>(),
                    FeatureTiers = new Dictionary<LicenseTier, List<ProductFeatureDefinition>>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdProduct = await _productService.CreateProductAsync(product);
                if (createdProduct != null)
                {
                    TempData["Success"] = $"Product '{model.ProductName}' created successfully";
                    return RedirectToAction(nameof(Details), new { id = model.ProductId });
                }
                else
                {
                    ModelState.AddModelError("", "Failed to create product. Product ID might already exist.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product {ProductId}", model.ProductId);
                ModelState.AddModelError("", "An error occurred while creating the product.");
                return View(model);
            }
        }

        /// <summary>
        /// Edit product form
        /// </summary>
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                var product = await _productService.GetProductAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Product not found";
                    return NotFound();
                }

                var viewModel = new ProductEditViewModel
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    ProductType = product.ProductType,
                    Version = product.Version,
                    IsActive = product.IsActive,
                    Metadata = product.Metadata,
                    AvailableFeatures = product.AvailableFeatures.Select(f => new FeatureDefinitionViewModel
                    {
                        FeatureId = f.FeatureId,
                        Name = f.Name,
                        Description = f.Description,
                        Category = f.Category,
                        IsEnabledByDefault = f.IsEnabledByDefault,
                        MinimumTier = f.MinimumTier
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product {ProductId} for editing", id);
                TempData["Error"] = "Failed to load product for editing";
                return NotFound();
            }
        }

        /// <summary>
        /// Update product
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ProductEditViewModel model)
        {
            if (id != model.ProductId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var existingProduct = await _productService.GetProductAsync(id);
                if (existingProduct == null)
                {
                    TempData["Error"] = "Product not found";
                    return NotFound();
                }

                var updatedProduct = new ProductConfiguration
                {
                    ProductId = model.ProductId,
                    ProductName = model.ProductName,
                    ProductType = model.ProductType,
                    Version = model.Version,
                    IsActive = model.IsActive,
                    Metadata = model.Metadata ?? new Dictionary<string, string>(),
                    AvailableFeatures = model.AvailableFeatures?.Select(f => new ProductFeatureDefinition
                    {
                        FeatureId = f.FeatureId,
                        Name = f.Name,
                        Description = f.Description,
                        Category = f.Category,
                        IsEnabledByDefault = f.IsEnabledByDefault,
                        MinimumTier = f.MinimumTier
                    }).ToList() ?? new List<ProductFeatureDefinition>(),
                    FeatureTiers = existingProduct.FeatureTiers,
                    CreatedAt = existingProduct.CreatedAt,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await _productService.UpdateProductAsync(updatedProduct);
                if (result != null)
                {
                    TempData["Success"] = $"Product '{model.ProductName}' updated successfully";
                    return RedirectToAction(nameof(Details), new { id = model.ProductId });
                }
                else
                {
                    ModelState.AddModelError("", "Failed to update product.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", model.ProductId);
                ModelState.AddModelError("", "An error occurred while updating the product.");
                return View(model);
            }
        }

        /// <summary>
        /// Delete product confirmation
        /// </summary>
        public IActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            TempData["Info"] = "Product deletion functionality is being implemented";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
