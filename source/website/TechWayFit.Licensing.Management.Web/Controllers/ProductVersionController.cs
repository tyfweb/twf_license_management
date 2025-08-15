using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Management.Web.Controllers;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Web.ViewModels.Product;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Web.Controllers;

/// <summary>
/// Product Version Management Controller
/// Handles product version CRUD operations
/// </summary>
[Authorize]
public class ProductVersionController : BaseController
{
    private readonly ILogger<ProductVersionController> _logger;
    private readonly IEnterpriseProductService _productService;

    public ProductVersionController(
        ILogger<ProductVersionController> logger,
        IEnterpriseProductService productService)
    {
        _logger = logger;
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
    }

    /// <summary>
    /// Product Version Index - Show all versions for a product
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

            // Get product versions
            var versions = await _productService.GetProductVersionsAsync(productId);
            var versionViewModels = versions.Select(v => new VersionInfoViewModel
            {
                Id = v.VersionId,
                ProductId = v.ProductId,
                Version = v.Version?.ToString() ?? "",
                Name = v.Name,
                ReleaseDate = v.ReleaseDate,
                EndOfLifeDate = v.EndOfLifeDate,
                SupportEndDate = v.SupportEndDate,
                ReleaseNotes = v.ChangeLog ?? "",
                IsActive = true, // Default to active
                IsCurrent = v.IsCurrent,
                CanDelete = !v.IsCurrent // Can't delete current version
            }).ToList();

            var indexViewModel = new ProductVersionIndexViewModel
            {
                ProductId = productId,
                ProductName = product.Name,
                ProductDescription = product.Description
            };

            ViewData["Product"] = indexViewModel;
            ViewData["Versions"] = versionViewModels;

            return View(indexViewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading product versions page for {ProductId}", productId);
            return StatusCode(500, "An error occurred while loading the page");
        }
    }

    /// <summary>
    /// Show Create Version Form
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

            var viewModel = new ProductVersionCreateEditViewModel
            {
                ProductId = productId,
                Version = "",
                Name = "",
                ReleaseNotes = "",
                ReleaseDate = DateTime.Now,
                IsActive = true,
                IsCurrent = false
            };

            ViewData["ProductName"] = product.Name;
            ViewData["ProductId"] = productId;

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create version form for product {ProductId}", productId);
            return StatusCode(500, "An error occurred while loading the form");
        }
    }

    /// <summary>
    /// Create New Version
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Guid productId, ProductVersionCreateEditViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var product = await _productService.GetProductByIdAsync(productId);
                ViewData["ProductName"] = product?.Name ?? "";
                ViewData["ProductId"] = productId;
                return View(model);
            }

            // Parse version string to create SemanticVersion
            var versionParts = model.Version.Split('.');
            var major = versionParts.Length > 0 ? int.Parse(versionParts[0]) : 1;
            var minor = versionParts.Length > 1 ? int.Parse(versionParts[1]) : 0;
            var patch = versionParts.Length > 2 ? int.Parse(versionParts[2]) : 0;

            var newVersion = new ProductVersion
            {
                ProductId = productId,
                Version = new SemanticVersion(major, minor, patch),
                Name = model.Name,
                ReleaseDate = model.ReleaseDate ?? DateTime.Now,
                EndOfLifeDate = model.EndOfLifeDate,
                SupportEndDate = model.SupportEndDate,
                ChangeLog = model.ReleaseNotes,
                IsCurrent = model.IsCurrent
            };

            await _productService.AddProductVersionAsync(productId, newVersion, CurrentUserName);

            return RedirectToAction(nameof(Index), new { productId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating version for product {ProductId}", productId);
            var product = await _productService.GetProductByIdAsync(productId);
            ViewData["ProductName"] = product?.Name ?? "";
            ViewData["ProductId"] = productId;
            ModelState.AddModelError("", "An error occurred while creating the version");
            return View(model);
        }
    }

    /// <summary>
    /// Show Edit Version Form
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid productId, Guid versionId)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                return NotFound($"Product with ID {productId} not found");
            }

            var versions = await _productService.GetProductVersionsAsync(productId);
            var version = versions.FirstOrDefault(v => v.VersionId == versionId);
            if (version == null)
            {
                return NotFound($"Version with ID {versionId} not found");
            }

            var viewModel = new ProductVersionCreateEditViewModel
            {
                VersionId = versionId,
                ProductId = productId,
                Version = version.Version?.ToString() ?? "",
                Name = version.Name,
                ReleaseDate = version.ReleaseDate,
                EndOfLifeDate = version.EndOfLifeDate,
                SupportEndDate = version.SupportEndDate,
                ReleaseNotes = version.ChangeLog ?? "",
                IsCurrent = version.IsCurrent,
                IsActive = true // Default for edit, could be based on status
            };

            ViewData["ProductName"] = product.Name;
            ViewData["ProductId"] = productId;
            ViewData["VersionId"] = versionId;

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit version form for {VersionId} in product {ProductId}", versionId, productId);
            return StatusCode(500, "An error occurred while loading the form");
        }
    }

    /// <summary>
    /// Update Version
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid productId, Guid versionId, ProductVersionCreateEditViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var product = await _productService.GetProductByIdAsync(productId);
                ViewData["ProductName"] = product?.Name ?? "";
                ViewData["ProductId"] = productId;
                ViewData["VersionId"] = versionId;
                return View(model);
            }

            var versions = await _productService.GetProductVersionsAsync(productId);
            var existingVersion = versions.FirstOrDefault(v => v.VersionId == versionId);
            if (existingVersion == null)
            {
                return NotFound($"Version with ID {versionId} not found");
            }

            // Parse version string to create SemanticVersion
            var versionParts = model.Version.Split('.');
            var major = versionParts.Length > 0 ? int.Parse(versionParts[0]) : 1;
            var minor = versionParts.Length > 1 ? int.Parse(versionParts[1]) : 0;
            var patch = versionParts.Length > 2 ? int.Parse(versionParts[2]) : 0;

            // Update version properties
            existingVersion.Version = new SemanticVersion(major, minor, patch);
            existingVersion.Name = model.Name;
            existingVersion.ReleaseDate = model.ReleaseDate ?? DateTime.Now;
            existingVersion.EndOfLifeDate = model.EndOfLifeDate;
            existingVersion.SupportEndDate = model.SupportEndDate;
            existingVersion.ChangeLog = model.ReleaseNotes;

            await _productService.UpdateProductVersionAsync(productId, existingVersion, CurrentUserName);

            return RedirectToAction(nameof(Index), new { productId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating version {VersionId} for product {ProductId}", versionId, productId);
            var product = await _productService.GetProductByIdAsync(productId);
            ViewData["ProductName"] = product?.Name ?? "";
            ViewData["ProductId"] = productId;
            ViewData["VersionId"] = versionId;
            ModelState.AddModelError("", "An error occurred while updating the version");
            return View(model);
        }
    }

    /// <summary>
    /// Delete Version
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid productId, Guid versionId)
    {
        try
        {
            var versions = await _productService.GetProductVersionsAsync(productId);
            var version = versions.FirstOrDefault(v => v.VersionId == versionId);
            if (version == null)
            {
                return NotFound($"Version with ID {versionId} not found");
            }

            if (version.IsCurrent)
            {
                return BadRequest("Cannot delete the current version");
            }

            await _productService.DeleteProductVersionAsync(productId, version, CurrentUserName);

            return RedirectToAction(nameof(Index), new { productId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting version {VersionId} for product {ProductId}", versionId, productId);
            return StatusCode(500, "An error occurred while deleting the version");
        }
    }

    /// <summary>
    /// Set a version as current
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetCurrent(Guid productId, Guid versionId)
    {
        try
        {
            // This would require a method to set current version
            // For now, redirect back to index
            // TODO: Implement SetCurrentVersionAsync in service

            await Task.CompletedTask; // Remove warning about async method without await

            return RedirectToAction(nameof(Index), new { productId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting current version {VersionId} for product {ProductId}", versionId, productId);
            return StatusCode(500, "An error occurred while setting the current version");
        }
    }

    /// <summary>
    /// Product Version Details - Show detailed information about a specific version
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(Guid productId, Guid versionId)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                return NotFound($"Product with ID {productId} not found");
            }

            var versions = await _productService.GetProductVersionsAsync(productId);
            var version = versions.FirstOrDefault(v => v.VersionId == versionId);
            if (version == null)
            {
                return NotFound($"Version with ID {versionId} not found");
            }

            var viewModel = new ProductVersionDetailsViewModel
            {
                Id = version.VersionId,
                ProductId = version.ProductId,
                ProductName = product.Name,
                ProductDescription = product.Description,
                Version = version.Version?.ToString() ?? "",
                Name = version.Name,
                ReleaseNotes = version.ChangeLog, // Using ChangeLog from ProductVersion model
                ReleaseDate = version.ReleaseDate,
                EndOfLifeDate = version.EndOfLifeDate,
                IsCurrent = version.IsCurrent,
                IsActive = version.Audit.IsActive, // Using Audit.IsActive from ProductVersion model
                CreatedAt = version.Audit.CreatedOn, // Using Audit.CreatedOn from ProductVersion model
                UpdatedAt = version.Audit.UpdatedOn ?? version.Audit.CreatedOn // Using Audit.UpdatedOn from ProductVersion model
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting version details for version {VersionId} in product {ProductId}", versionId, productId);
            return StatusCode(500, "An error occurred while getting version details");
        }
    }
}
