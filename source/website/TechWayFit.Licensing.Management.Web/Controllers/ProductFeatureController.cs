using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechWayFit.Licensing.Management.Web.Controllers;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Web.ViewModels.Product;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Web.Controllers;

/// <summary>
/// Product Feature Management Controller
/// Handles product feature CRUD operations
/// </summary>
[Authorize]
public class ProductFeatureController : BaseController
{
    private readonly ILogger<ProductFeatureController> _logger;
    private readonly IEnterpriseProductService _productService;
    private readonly IProductTierService _productTierService;
    private readonly IProductFeatureService _productFeatureService;

    public ProductFeatureController(
        ILogger<ProductFeatureController> logger,
        IEnterpriseProductService productService,
        IProductTierService productTierService,
        IProductFeatureService productFeatureService)
    {
        _logger = logger;
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _productTierService = productTierService ?? throw new ArgumentNullException(nameof(productTierService));
        _productFeatureService = productFeatureService ?? throw new ArgumentNullException(nameof(productFeatureService));
    }

    /// <summary>
    /// Product Feature Index - Show all features for a product
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(Guid productId)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                return NotFound($"Product with ID {productId} not found");
            }

            // Get product features
            var features = await _productFeatureService.GetFeaturesByproductIdAsync(productId);
            var featureViewModels = features.Select(f => new FeatureInfoViewModel
            {
                Id = f.FeatureId,
                ProductId = f.ProductId,
                TierId = f.TierId,
                Name = f.Name,
                Description = f.Description,
                Code = f.Code,
                IsEnabled = f.IsEnabled,
                DisplayOrder = f.DisplayOrder,
                SupportFromVersion = f.SupportFromVersion?.ToString() ?? "",
                SupportToVersion = f.SupportToVersion?.ToString() ?? "",
                CanDelete = true // Most features can be deleted
            }).ToList();

            // Get available tiers for the product
            var tiers = await _productTierService.GetTiersByProductAsync(productId);
            var tierViewModels = tiers.Select(t => new TierInfoViewModel
            {
                Id = t.TierId,
                Name = t.Name,
                Description = t.Description,
                DisplayOrder = t.DisplayOrder
            }).ToList();

            var indexViewModel = new ProductFeatureIndexViewModel
            {
                ProductId = productId,
                ProductName = product.Name,
                ProductDescription = product.Description
            };

            ViewData["Product"] = indexViewModel;
            ViewData["Features"] = featureViewModels;
            ViewData["Tiers"] = tierViewModels;

            return View(indexViewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading product features page for {ProductId}", productId);
            return StatusCode(500, "An error occurred while loading the page");
        }
    }

    /// <summary>
    /// Show Create Feature Form
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create(Guid productId)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                return NotFound($"Product with ID {productId} not found");
            }

            // Get available tiers for the product
            var tiers = await _productTierService.GetTiersByProductAsync(productId);
            var tierViewModels = tiers.Select(t => new TierInfoViewModel
            {
                Id = t.TierId,
                Name = t.Name,
                Description = t.Description,
                DisplayOrder = t.DisplayOrder
            }).ToList();

            // Get available versions for the product
            var versions = await _productService.GetProductVersionsAsync(productId);
            var versionViewModels = versions.Select(v => new VersionInfoViewModel
            {
                Id = v.VersionId,
                Version = v.Version?.ToString() ?? "",
                Name = v.Name
            }).ToList();

            var viewModel = new ProductFeatureCreateEditViewModel
            {
                ProductId = productId,
                Name = "",
                Description = "",
                Code = "",
                IsEnabled = true,
                DisplayOrder = 0
            };

            ViewData["ProductName"] = product.Name;
            ViewData["ProductId"] = productId;
            ViewData["Tiers"] = tierViewModels;
            ViewData["Versions"] = versionViewModels.Select(v => new SelectListItem
            {
                Value = v.Id.ToString(),
                Text = $"{v.Version} - {v.Name}"
            }).ToList();

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create feature form for product {ProductId}", productId);
            return StatusCode(500, "An error occurred while loading the form");
        }
    }

    /// <summary>
    /// Create New Feature
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Guid productId, ProductFeatureCreateEditViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var product = await _productService.GetProductByIdAsync(productId);
                var tiers = await _productTierService.GetTiersByProductAsync(productId);
                var tierViewModels = tiers.Select(t => new TierInfoViewModel
                {
                    Id = t.TierId,
                    Name = t.Name,
                    Description = t.Description,
                    DisplayOrder = t.DisplayOrder
                }).ToList();

                // Get versions for error case
                var versions = await _productService.GetProductVersionsAsync(productId);
                var versionViewModels = versions.Select(v => new VersionInfoViewModel
                {
                    Id = v.VersionId,
                    Version = v.Version?.ToString() ?? "",
                    Name = v.Name
                }).ToList();

                ViewData["ProductName"] = product?.Name ?? "";
                ViewData["ProductId"] = productId;
                ViewData["Tiers"] = tierViewModels;
                ViewData["Versions"] = versionViewModels.Select(v => new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = $"{v.Version} - {v.Name}"
                }).ToList();
                return View(model);
            }

            var newFeature = new ProductFeature
            {
                ProductId = productId,
                TierId = model.TierId,
                Name = model.Name,
                Description = model.Description,
                Code = model.Code,
                IsEnabled = model.IsEnabled,
                DisplayOrder = model.DisplayOrder,
                SupportFromVersionId = model.SupportFromVersionId,
                SupportToVersionId = model.SupportToVersionId,
                // Set default semantic versions for compatibility
                SupportFromVersion = SemanticVersion.Default,
                SupportToVersion = SemanticVersion.Max
            };

            await _productFeatureService.CreateFeatureAsync(newFeature, CurrentUserName);

            return RedirectToAction(nameof(Index), new { productId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating feature for product {ProductId}", productId);
            var product = await _productService.GetProductByIdAsync(productId);
            var tiers = await _productTierService.GetTiersByProductAsync(productId);
            var tierViewModels = tiers.Select(t => new TierInfoViewModel
            {
                Id = t.TierId,
                Name = t.Name,
                Description = t.Description,
                DisplayOrder = t.DisplayOrder
            }).ToList();

            ViewData["ProductName"] = product?.Name ?? "";
            ViewData["ProductId"] = productId;
            ViewData["Tiers"] = tierViewModels;
            ModelState.AddModelError("", "An error occurred while creating the feature");
            return View(model);
        }
    }

    /// <summary>
    /// Show Edit Feature Form
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid productId, Guid featureId)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                return NotFound($"Product with ID {productId} not found");
            }

            var feature = await _productFeatureService.GetFeatureByIdAsync(featureId);
            if (feature == null || feature.ProductId != productId)
            {
                return NotFound($"Feature with ID {featureId} not found");
            }

            // Get available tiers for the product
            var tiers = await _productTierService.GetTiersByProductAsync(productId);
            var tierViewModels = tiers.Select(t => new TierInfoViewModel
            {
                Id = t.TierId,
                Name = t.Name,
                Description = t.Description,
                DisplayOrder = t.DisplayOrder
            }).ToList();

            var viewModel = new ProductFeatureCreateEditViewModel
            {
                FeatureId = featureId,
                ProductId = productId,
                TierId = feature.TierId,
                Name = feature.Name,
                Description = feature.Description,
                Code = feature.Code,
                IsEnabled = feature.IsEnabled,
                DisplayOrder = feature.DisplayOrder,
                SupportFromVersionId = feature.SupportFromVersionId,
                SupportToVersionId = feature.SupportToVersionId
            };

            // Get available versions for the product
            var versions = await _productService.GetProductVersionsAsync(productId);
            var versionViewModels = versions.Select(v => new VersionInfoViewModel
            {
                Id = v.VersionId,
                Version = v.Version?.ToString() ?? "",
                Name = v.Name
            }).ToList();

            ViewData["ProductName"] = product.Name;
            ViewData["ProductId"] = productId;
            ViewData["FeatureId"] = featureId;
            ViewData["Tiers"] = tierViewModels;
            ViewData["Versions"] = versionViewModels.Select(v => new SelectListItem
            {
                Value = v.Id.ToString(),
                Text = $"{v.Version} - {v.Name}"
            }).ToList();

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit feature form for {FeatureId} in product {ProductId}", featureId, productId);
            return StatusCode(500, "An error occurred while loading the form");
        }
    }

    /// <summary>
    /// Update Feature
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid productId, Guid featureId, ProductFeatureCreateEditViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var product = await _productService.GetProductByIdAsync(productId);
                var tiers = await _productTierService.GetTiersByProductAsync(productId);
                var tierViewModels = tiers.Select(t => new TierInfoViewModel
                {
                    Id = t.TierId,
                    Name = t.Name,
                    Description = t.Description,
                    DisplayOrder = t.DisplayOrder
                }).ToList();

                // Get versions for error case
                var versions = await _productService.GetProductVersionsAsync(productId);
                var versionViewModels = versions.Select(v => new VersionInfoViewModel
                {
                    Id = v.VersionId,
                    Version = v.Version?.ToString() ?? "",
                    Name = v.Name
                }).ToList();

                ViewData["ProductName"] = product?.Name ?? "";
                ViewData["ProductId"] = productId;
                ViewData["FeatureId"] = featureId;
                ViewData["Tiers"] = tierViewModels;
                ViewData["Versions"] = versionViewModels.Select(v => new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = $"{v.Version} - {v.Name}"
                }).ToList();
                return View(model);
            }

            var existingFeature = await _productFeatureService.GetFeatureByIdAsync(featureId);
            if (existingFeature == null || existingFeature.ProductId != productId)
            {
                return NotFound($"Feature with ID {featureId} not found");
            }

            // Update feature properties
            existingFeature.TierId = model.TierId;
            existingFeature.Name = model.Name;
            existingFeature.Description = model.Description;
            existingFeature.Code = model.Code;
            existingFeature.IsEnabled = model.IsEnabled;
            existingFeature.DisplayOrder = model.DisplayOrder;
            existingFeature.SupportFromVersionId = model.SupportFromVersionId;
            existingFeature.SupportToVersionId = model.SupportToVersionId;
            // Keep the semantic versions for backward compatibility (will be updated by service if needed)
            existingFeature.SupportFromVersion = existingFeature.SupportFromVersion;
            existingFeature.SupportToVersion = existingFeature.SupportToVersion;

            await _productFeatureService.UpdateFeatureAsync(existingFeature, CurrentUserName);

            return RedirectToAction(nameof(Index), new { productId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating feature {FeatureId} for product {ProductId}", featureId, productId);
            var product = await _productService.GetProductByIdAsync(productId);
            var tiers = await _productTierService.GetTiersByProductAsync(productId);
            var tierViewModels = tiers.Select(t => new TierInfoViewModel
            {
                Id = t.TierId,
                Name = t.Name,
                Description = t.Description,
                DisplayOrder = t.DisplayOrder
            }).ToList();

            ViewData["ProductName"] = product?.Name ?? "";
            ViewData["ProductId"] = productId;
            ViewData["FeatureId"] = featureId;
            ViewData["Tiers"] = tierViewModels;
            ModelState.AddModelError("", "An error occurred while updating the feature");
            return View(model);
        }
    }

    /// <summary>
    /// Delete Feature
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid productId, Guid featureId)
    {
        try
        {
            var feature = await _productFeatureService.GetFeatureByIdAsync(featureId);
            if (feature == null || feature.ProductId != productId)
            {
                return NotFound($"Feature with ID {featureId} not found");
            }

            await _productFeatureService.DeleteFeatureAsync(featureId, CurrentUserName);

            return RedirectToAction(nameof(Index), new { productId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting feature {FeatureId} for product {ProductId}", featureId, productId);
            return StatusCode(500, "An error occurred while deleting the feature");
        }
    }

    /// <summary>
    /// Toggle feature enabled/disabled status
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleEnabled(Guid productId, Guid featureId)
    {
        try
        {
            var feature = await _productFeatureService.GetFeatureByIdAsync(featureId);
            if (feature == null || feature.ProductId != productId)
            {
                return NotFound($"Feature with ID {featureId} not found");
            }

            feature.IsEnabled = !feature.IsEnabled;
            await _productFeatureService.UpdateFeatureAsync(feature, CurrentUserName);

            return RedirectToAction(nameof(Index), new { productId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling feature {FeatureId} for product {ProductId}", featureId, productId);
            return StatusCode(500, "An error occurred while updating the feature");
        }
    }
}
