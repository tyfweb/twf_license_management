using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Web.ViewModels.Product;
using TechWayFit.Licensing.Management.Web.ViewModels.Shared;
using TechWayFit.Licensing.Management.Web.Helpers;
using ProductTierViewModel = TechWayFit.Licensing.Management.Web.ViewModels.Product.ProductTierViewModel;

namespace TechWayFit.Licensing.Management.Web.Controllers
{
    /// <summary>
    /// Product Tier Management Controller
    /// Handles all product tier-related operations
    /// </summary>
    [Authorize]
    public class ProductTierController : BaseController
    {
        private readonly ILogger<ProductTierController> _logger;
        private readonly IEnterpriseProductService _productService;
        private readonly IProductTierService _productTierService;

        public ProductTierController(
            ILogger<ProductTierController> logger,
            IEnterpriseProductService productService,
            IProductTierService productTierService)
        {
            _logger = logger;
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _productTierService = productTierService ?? throw new ArgumentNullException(nameof(productTierService));
        }

        /// <summary>
        /// Dedicated Product Tiers management page
        /// </summary>
        [Route("ProductTier/{productId:guid}")]
        public async Task<IActionResult> Index(Guid productId)
        {
            try
            {
                var product = await GetProductByIdAsync(productId);
                if (product == null)
                {
                    return NotFound();
                }

                // Load product tiers data directly
                var tiers = await GetProductTiersDataAsync(productId);
                ViewData["ProductTiers"] = tiers;

                var viewModel = CreateEnhancedEditViewModel(product, tiers);

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product tiers page for {ProductId}", productId);
                TempData["ErrorMessage"] = "Error loading product tiers. Please try again.";
                return RedirectToAction("Index", "Product");
            }
        }

        /// <summary>
        /// Get product tiers for AJAX loading
        /// </summary>
        [HttpGet]
        [Route("ProductTier/{productId:guid}/list")]
        public async Task<IActionResult> GetProductTiers(Guid productId)
        {
            try
            {
                var tiers = await GetProductTiersDataAsync(productId);
                return Json(JsonResponse.OK(tiers, "Product tiers loaded successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product tiers for {ProductId}", productId);
                return Json(JsonResponse.Error("Error loading tiers"));
            }
        }

        /// <summary>
        /// Get a specific product tier for editing
        /// </summary>
        [HttpGet]
        [Route("ProductTier/{productId:guid}/{tierId:guid}")]
        public async Task<IActionResult> GetProductTier(Guid productId, Guid tierId)
        {
            try
            {
                var tier = await _productTierService.GetTierByIdAsync(tierId);
                if (tier == null)
                {
                    return Json(JsonResponse.Error("Product tier not found"));
                }

                var viewModel = new ProductTierViewModel
                {
                    TierId = tier.TierId,
                    ProductId = tier.ProductId,
                    Name = tier.Name,
                    Description = tier.Description,
                    DisplayOrder = tier.DisplayOrder,
                    SupportSLA = tier.SupportSLA,
                    IsActive = tier.Audit.IsActive,
                    MaxUsers = tier.MaxUsers,
                    MaxDevices = tier.MaxDevices
                };

                return Json(JsonResponse.OK(viewModel, "Product tier loaded successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product tier {TierId} for product {ProductId}", tierId, productId);
                return Json(JsonResponse.Error("Error loading product tier"));
            }
        }

        /// <summary>
        /// Display the create tier view
        /// </summary>
        [HttpGet]
        [Route("ProductTier/{productId:guid}/create")]
        public async Task<IActionResult> Create(Guid productId)
        {
            try
            {
                var product = await GetProductByIdAsync(productId);
                if (product == null)
                {
                    return NotFound();
                }

                var model = new ProductTierViewModel
                {
                    ProductId = productId,
                    IsActive = true,
                    MonthlyPriceCurrency = "USD",
                    AnnualPriceCurrency = "USD"
                };

                ViewData["ProductName"] = product.Name;
                ViewData["ProductId"] = productId;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create tier view for product {ProductId}", productId);
                return RedirectToAction("Index", new { productId });
            }
        }

        /// <summary>
        /// Display the edit tier view
        /// </summary>
        [HttpGet]
        [Route("ProductTier/{productId:guid}/{tierId:guid}/edit")]
        public async Task<IActionResult> Edit(Guid productId, Guid tierId)
        {
            try
            {
                var product = await GetProductByIdAsync(productId);
                if (product == null)
                {
                    return NotFound();
                }

                var tier = await _productTierService.GetTierByIdAsync(tierId);
                if (tier == null)
                {
                    return NotFound();
                }

                var model = new ProductTierViewModel
                {
                    Id = tier.TierId,
                    TierId = tier.TierId,
                    ProductId = tier.ProductId,
                    Name = tier.Name,
                    Description = tier.Description,
                    DisplayOrder = tier.DisplayOrder,
                    SupportSLA = tier.SupportSLA,
                    IsActive = tier.Audit.IsActive,
                    MaxUsers = tier.MaxUsers,
                    MaxDevices = tier.MaxDevices,
                    CreatedAt = tier.Audit.CreatedOn,
                    UpdatedAt = tier.Audit.UpdatedOn ?? tier.Audit.CreatedOn,
                    
                    // Map SLA fields for form binding
                    SLAName = tier.SupportSLA?.Name ?? string.Empty,
                    SLADescription = tier.SupportSLA?.Description ?? string.Empty,
                    CriticalResponseHours = tier.SupportSLA?.CriticalResponseTime.TotalHours ?? 1.0,
                    HighPriorityResponseHours = tier.SupportSLA?.HighPriorityResponseTime.TotalHours ?? 4.0,
                    MediumPriorityResponseHours = tier.SupportSLA?.MediumPriorityResponseTime.TotalHours ?? 8.0,
                    LowPriorityResponseHours = tier.SupportSLA?.LowPriorityResponseTime.TotalHours ?? 24.0
                };

                // Convert prices to individual fields
                var monthlyPrice = tier.Prices?.FirstOrDefault(p => p.PriceType == TechWayFit.Licensing.Management.Core.Models.Product.PriceType.Monthly);
                var annualPrice = tier.Prices?.FirstOrDefault(p => p.PriceType == TechWayFit.Licensing.Management.Core.Models.Product.PriceType.Yearly);

                if (monthlyPrice != null)
                {
                    model.MonthlyPriceAmount = monthlyPrice.Price.Amount;
                    model.MonthlyPriceCurrency = monthlyPrice.Price.Currency;
                }

                if (annualPrice != null)
                {
                    model.AnnualPriceAmount = annualPrice.Price.Amount;
                    model.AnnualPriceCurrency = annualPrice.Price.Currency;
                }

                ViewData["ProductName"] = product.Name;
                ViewData["ProductId"] = productId;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit tier view for tier {TierId} in product {ProductId}", tierId, productId);
                return RedirectToAction("Index", new { productId });
            }
        }

        /// <summary>
        /// Add a new product tier - Web Form
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ProductTier/{productId:guid}/create")]
        public async Task<IActionResult> Create(Guid productId, ProductTierViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please correct the validation errors and try again.";
                    return View(model);
                }

                var serviceDto = new ProductTier
                {
                    ProductId = model.ProductId,
                    Name = model.Name,
                    Description = model.Description,
                    DisplayOrder = model.DisplayOrder,
                    SupportSLA = !string.IsNullOrEmpty(model.SLAName) ? new ProductSupportSLA
                    {
                        SlaId = Guid.NewGuid().ToString(),
                        Name = model.SLAName,
                        Description = model.SLADescription,
                        CriticalResponseTime = TimeSpan.FromHours(model.CriticalResponseHours),
                        HighPriorityResponseTime = TimeSpan.FromHours(model.HighPriorityResponseHours),
                        MediumPriorityResponseTime = TimeSpan.FromHours(model.MediumPriorityResponseHours),
                        LowPriorityResponseTime = TimeSpan.FromHours(model.LowPriorityResponseHours)
                    } : ProductSupportSLA.NoSLA,
                    Audit = new AuditInfo
                    {
                        IsActive = model.IsActive
                    },
                    MaxUsers = model.MaxUsers,
                    MaxDevices = model.MaxDevices,
                    Prices = new List<ProductTierPrice>()
                };

                // Add monthly price if specified
                if (model.MonthlyPriceAmount.HasValue && model.MonthlyPriceAmount > 0)
                {
                    serviceDto.Prices.Add(new ProductTierPrice
                    {
                        TierId = serviceDto.TierId,
                        ProductId = model.ProductId,
                        Price = new Money { Amount = model.MonthlyPriceAmount.Value, Currency = model.MonthlyPriceCurrency },
                        PriceType = TechWayFit.Licensing.Management.Core.Models.Product.PriceType.Monthly,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                // Add annual price if specified
                if (model.AnnualPriceAmount.HasValue && model.AnnualPriceAmount > 0)
                {
                    serviceDto.Prices.Add(new ProductTierPrice
                    {
                        TierId = serviceDto.TierId,
                        ProductId = model.ProductId,
                        Price = new Money { Amount = model.AnnualPriceAmount.Value, Currency = model.AnnualPriceCurrency },
                        PriceType = TechWayFit.Licensing.Management.Core.Models.Product.PriceType.Yearly,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                var productTier = await _productTierService.CreateTierAsync(serviceDto, User.Identity?.Name ?? "System");

                _logger.LogInformation("Product tier '{TierName}' added to product {ProductId}", model.Name, model.ProductId);
                TempData["Success"] = "Product tier added successfully.";
                return RedirectToAction("Index", new { productId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product tier for product {ProductId}", model.ProductId);
                TempData["Error"] = "Error adding product tier. Please try again.";
                return View(model);
            }
        }

        /// <summary>
        /// Add a new product tier - API
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ProductTier/add")]
        public async Task<IActionResult> AddProductTier(ProductTierViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return Json(JsonResponse.Error("Validation failed", errors));
                }

                var serviceDto = new ProductTier
                {
                    ProductId = model.ProductId,
                    Name = model.Name,
                    Description = model.Description,
                    DisplayOrder = model.DisplayOrder,
                    SupportSLA = model.SupportSLA,
                    Audit = new AuditInfo
                    {
                        IsActive = model.IsActive
                    },
                    MaxUsers = model.MaxUsers,
                    MaxDevices = model.MaxDevices,
                    Prices = new List<ProductTierPrice>()
                };

                // Add monthly price if specified
                if (model.MonthlyPriceAmount.HasValue && model.MonthlyPriceAmount > 0)
                {
                    serviceDto.Prices.Add(new ProductTierPrice
                    {
                        TierId = serviceDto.TierId,
                        ProductId = model.ProductId,
                        Price = new Money { Amount = model.MonthlyPriceAmount.Value, Currency = model.MonthlyPriceCurrency },
                        PriceType = TechWayFit.Licensing.Management.Core.Models.Product.PriceType.Monthly,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                // Add annual price if specified
                if (model.AnnualPriceAmount.HasValue && model.AnnualPriceAmount > 0)
                {
                    serviceDto.Prices.Add(new ProductTierPrice
                    {
                        TierId = serviceDto.TierId,
                        ProductId = model.ProductId,
                        Price = new Money { Amount = model.AnnualPriceAmount.Value, Currency = model.AnnualPriceCurrency },
                        PriceType = TechWayFit.Licensing.Management.Core.Models.Product.PriceType.Yearly,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                var productTier = await _productTierService.CreateTierAsync(serviceDto, User.Identity?.Name ?? "System");

                _logger.LogInformation("Product tier '{TierName}' added to product {ProductId}", model.Name, model.ProductId);
                return Json(JsonResponse.OK(productTier, "Product tier added successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product tier for product {ProductId}", model.ProductId);
                return Json(JsonResponse.Error("Error adding product tier"));
            }
        }

        /// <summary>
        /// Edit an existing product tier - Web Form
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ProductTier/{productId:guid}/{tierId:guid}/edit")]
        public async Task<IActionResult> Edit(Guid productId, Guid tierId, ProductTierViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please correct the validation errors and try again.";
                    return View(model);
                }

                var existingTier = await _productTierService.GetTierByIdAsync(tierId);
                if (existingTier == null)
                {
                    TempData["Error"] = "Product tier not found.";
                    return RedirectToAction("Index", new { productId });
                }

                // Update the tier properties
                existingTier.Name = model.Name;
                existingTier.Description = model.Description;
                existingTier.DisplayOrder = model.DisplayOrder;
                existingTier.SupportSLA = !string.IsNullOrEmpty(model.SLAName) ? new ProductSupportSLA
                {
                    SlaId = existingTier.SupportSLA?.SlaId ?? Guid.NewGuid().ToString(),
                    Name = model.SLAName,
                    Description = model.SLADescription,
                    CriticalResponseTime = TimeSpan.FromHours(model.CriticalResponseHours),
                    HighPriorityResponseTime = TimeSpan.FromHours(model.HighPriorityResponseHours),
                    MediumPriorityResponseTime = TimeSpan.FromHours(model.MediumPriorityResponseHours),
                    LowPriorityResponseTime = TimeSpan.FromHours(model.LowPriorityResponseHours)
                } : ProductSupportSLA.NoSLA;
                existingTier.Audit.IsActive = model.IsActive;
                existingTier.MaxUsers = model.MaxUsers;
                existingTier.MaxDevices = model.MaxDevices;

                // Update pricing - clear existing and add new prices
                existingTier.Prices.Clear();

                // Add monthly price if specified
                if (model.MonthlyPriceAmount.HasValue && model.MonthlyPriceAmount > 0)
                {
                    existingTier.Prices.Add(new ProductTierPrice
                    {
                        TierId = existingTier.TierId,
                        ProductId = productId,
                        Price = new Money { Amount = model.MonthlyPriceAmount.Value, Currency = model.MonthlyPriceCurrency },
                        PriceType = TechWayFit.Licensing.Management.Core.Models.Product.PriceType.Monthly,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                // Add annual price if specified
                if (model.AnnualPriceAmount.HasValue && model.AnnualPriceAmount > 0)
                {
                    existingTier.Prices.Add(new ProductTierPrice
                    {
                        TierId = existingTier.TierId,
                        ProductId = productId,
                        Price = new Money { Amount = model.AnnualPriceAmount.Value, Currency = model.AnnualPriceCurrency },
                        PriceType = TechWayFit.Licensing.Management.Core.Models.Product.PriceType.Yearly,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                var updatedTier = await _productTierService.UpdateTierAsync(existingTier, User.Identity?.Name ?? "System");

                _logger.LogInformation("Product tier '{TierName}' (ID: {TierId}) updated for product {ProductId}",
                    model.Name, tierId, productId);

                TempData["Success"] = "Product tier updated successfully.";
                return RedirectToAction("Index", new { productId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product tier {TierId} for product {ProductId}", tierId, productId);
                TempData["Error"] = "Error updating product tier. Please try again.";
                return View(model);
            }
        }

        /// <summary>
        /// Edit an existing product tier - API
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ProductTier/{productId:guid}/{tierId:guid}/update")]
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
                    return Json(JsonResponse.Error("Validation failed", errors));
                }

                var existingTier = await _productTierService.GetTierByIdAsync(tierId);
                if (existingTier == null)
                {
                    return Json(JsonResponse.Error("Product tier not found"));
                }

                // Update the tier properties
                existingTier.Name = model.Name;
                existingTier.Description = model.Description;
                existingTier.DisplayOrder = model.DisplayOrder;
                existingTier.SupportSLA = !string.IsNullOrEmpty(model.SLAName) ? new ProductSupportSLA
                {
                    SlaId = existingTier.SupportSLA?.SlaId ?? Guid.NewGuid().ToString(),
                    Name = model.SLAName,
                    Description = model.SLADescription,
                    CriticalResponseTime = TimeSpan.FromHours(model.CriticalResponseHours),
                    HighPriorityResponseTime = TimeSpan.FromHours(model.HighPriorityResponseHours),
                    MediumPriorityResponseTime = TimeSpan.FromHours(model.MediumPriorityResponseHours),
                    LowPriorityResponseTime = TimeSpan.FromHours(model.LowPriorityResponseHours)
                } : ProductSupportSLA.NoSLA;
                existingTier.Audit.IsActive = model.IsActive;
                existingTier.MaxUsers = model.MaxUsers;
                existingTier.MaxDevices = model.MaxDevices;

                // Update pricing - clear existing and add new prices
                existingTier.Prices.Clear();

                // Add monthly price if specified
                if (model.MonthlyPriceAmount.HasValue && model.MonthlyPriceAmount > 0)
                {
                    existingTier.Prices.Add(new ProductTierPrice
                    {
                        TierId = existingTier.TierId,
                        ProductId = productId,
                        Price = new Money { Amount = model.MonthlyPriceAmount.Value, Currency = model.MonthlyPriceCurrency },
                        PriceType = TechWayFit.Licensing.Management.Core.Models.Product.PriceType.Monthly,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                // Add annual price if specified
                if (model.AnnualPriceAmount.HasValue && model.AnnualPriceAmount > 0)
                {
                    existingTier.Prices.Add(new ProductTierPrice
                    {
                        TierId = existingTier.TierId,
                        ProductId = productId,
                        Price = new Money { Amount = model.AnnualPriceAmount.Value, Currency = model.AnnualPriceCurrency },
                        PriceType = TechWayFit.Licensing.Management.Core.Models.Product.PriceType.Yearly,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                var updatedTier = await _productTierService.UpdateTierAsync(existingTier, User.Identity?.Name ?? "System");

                _logger.LogInformation("Product tier '{TierName}' (ID: {TierId}) updated for product {ProductId}",
                    model.Name, tierId, productId);

                return Json(JsonResponse.OK(updatedTier, "Product tier updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product tier {TierId} for product {ProductId}", tierId, productId);
                return Json(JsonResponse.Error("Error updating product tier"));
            }
        }

        /// <summary>
        /// Delete a product tier
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ProductTier/{productId:guid}/{tierId:guid}/delete")]
        public async Task<IActionResult> DeleteProductTier(Guid productId, Guid tierId)
        {
            try
            {
                var tier = await _productTierService.GetTierByIdAsync(tierId);
                if (tier == null)
                {
                    return Json(JsonResponse.Error("Product tier not found"));
                }

                // Attempt to delete the tier
                var deleted = await _productTierService.DeleteTierAsync(tierId, User.Identity?.Name ?? "System");
                if (!deleted)
                {
                    return Json(JsonResponse.Error("Cannot delete tier. It may have active licenses or other dependencies."));
                }

                _logger.LogInformation("Product tier {TierId} deleted from product {ProductId}", tierId, productId);

                return Json(JsonResponse.OK(null, "Product tier deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product tier {TierId} for product {ProductId}", tierId, productId);
                return Json(JsonResponse.Error("Error deleting product tier"));
            }
        }

        /// <summary>
        /// Toggle tier active status
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ProductTier/{productId:guid}/{tierId:guid}/toggle")]
        public async Task<IActionResult> ToggleProductTier(Guid productId, Guid tierId)
        {
            try
            {
                var tier = await _productTierService.GetTierByIdAsync(tierId);
                if (tier == null)
                {
                    return Json(JsonResponse.Error("Product tier not found"));
                }

                tier.Audit.IsActive = !tier.Audit.IsActive;
                var updatedTier = await _productTierService.UpdateTierAsync(tier, User.Identity?.Name ?? "System");

                _logger.LogInformation("Product tier {TierId} status toggled to {Status} for product {ProductId}",
                    tierId, tier.Audit.IsActive ? "Active" : "Inactive", productId);

                return Json(JsonResponse.OK(updatedTier, $"Product tier {(tier.Audit.IsActive ? "activated" : "deactivated")} successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling product tier {TierId} for product {ProductId}", tierId, productId);
                return Json(JsonResponse.Error("Error toggling product tier status"));
            }
        }

        #region Helper Methods

        /// <summary>
        /// Get product by ID
        /// </summary>
        private async Task<EnterpriseProduct?> GetProductByIdAsync(Guid productId)
        {
            return await _productService.GetProductByIdAsync(productId);
        }

        /// <summary>
        /// Get product tiers data
        /// </summary>
        private async Task<List<TierInfoViewModel>> GetProductTiersDataAsync(Guid productId)
        {
            var tiers = await _productTierService.GetTiersByProductAsync(productId);
            return tiers.Select(tier => new TierInfoViewModel
            {
                Id = tier.TierId,
                Name = tier.Name,
                Description = tier.Description,
                Price = tier.Prices?.FirstOrDefault()?.Price?.ToString() ?? "Free",
                DisplayOrder = tier.DisplayOrder,
                SupportSLA = tier.SupportSLA != null ? new TierSupportSlaViewModel
                {
                    Name = tier.SupportSLA.Name,
                    Description = tier.SupportSLA.Description,
                    CriticalResponseHours = tier.SupportSLA.CriticalResponseTime.TotalHours,
                    HighPriorityResponseHours = tier.SupportSLA.HighPriorityResponseTime.TotalHours,
                    MediumPriorityResponseHours = tier.SupportSLA.MediumPriorityResponseTime.TotalHours,
                    LowPriorityResponseHours = tier.SupportSLA.LowPriorityResponseTime.TotalHours
                } : null,
                IsActive = tier.Audit.IsActive,
                MaxUsers = tier.MaxUsers,
                MaxDevices = tier.MaxDevices,
                CanDelete = true // You might want to add logic to determine this
            }).OrderBy(t => t.DisplayOrder).ToList();
        }

        /// <summary>
        /// Create enhanced edit view model
        /// </summary>
        private ProductEnhancedEditViewModel CreateEnhancedEditViewModel(EnterpriseProduct product, List<TierInfoViewModel>? tiers = null)
        {
            return new ProductEnhancedEditViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                Description = product.Description,
                IsActive = product.Audit.IsActive,
                Version = product.Version,
                HasTiers = true,
                HasVersions = false,
                HasFeatures = false,
                ActiveTab = "tiers",
                ActiveSection = "tiers",
                StatsTiles = CreateProductTiersStatsTiles(product, tiers)
            };
        }

        /// <summary>
        /// Create product tiers stats tiles
        /// </summary>
        private List<StatsTileViewModel> CreateProductTiersStatsTiles(EnterpriseProduct product, List<TierInfoViewModel>? tiers = null)
        {
            // Calculate real statistics based on the tiers data
            var tiersCount = tiers?.Count ?? 0;
            var activeTiersCount = tiers?.Count(t => t.IsActive) ?? 0;
            var freeTiersCount = tiers?.Count(t => t.Price == "Free") ?? 0;
            var premiumTiersCount = tiers?.Count(t => t.Price != "Free") ?? 0;

            return new List<StatsTileViewModel>
            {
                new StatsTileViewModel
                {
                    Label = "Total Tiers",
                    Value = tiersCount.ToString(),
                    IconClass = "fas fa-layer-group",
                    CssClass = "stats-card-primary"
                },
                new StatsTileViewModel
                {
                    Label = "Active Tiers",
                    Value = activeTiersCount.ToString(),
                    IconClass = "fas fa-check-circle",
                    CssClass = "stats-card-success"
                },
                new StatsTileViewModel
                {
                    Label = "Free Tiers",
                    Value = freeTiersCount.ToString(),
                    IconClass = "fas fa-gift",
                    CssClass = "stats-card-info"
                },
                new StatsTileViewModel
                {
                    Label = "Premium Tiers",
                    Value = premiumTiersCount.ToString(),
                    IconClass = "fas fa-crown",
                    CssClass = "stats-card-warning"
                }
            };
        }

        #endregion
    }
}
