using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TechWayFit.Licensing.Core.Contracts;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.WebUI.ViewModels.Product;

namespace TechWayFit.Licensing.WebUI.Controllers
{
    /// <summary>
    /// Controller for product-specific license management
    /// </summary>
    [Authorize]
    public class ProductLicenseController : Controller
    {
        private readonly IProductService _productService;
        private readonly IConsumerService _consumerService;
        private readonly ILicenseLifecycleService _licenseService;
        private readonly ILogger<ProductLicenseController> _logger;

        public ProductLicenseController(
            IProductService productService,
            IConsumerService consumerService,
            ILicenseLifecycleService licenseService,
            ILogger<ProductLicenseController> logger)
        {
            _productService = productService;
            _consumerService = consumerService;
            _licenseService = licenseService;
            _logger = logger;
        }

        /// <summary>
        /// Show all licenses for a specific product, organized by consumer
        /// </summary>
        public async Task<IActionResult> Index(string productId)
        {
            if (string.IsNullOrEmpty(productId))
            {
                TempData["Error"] = "Product ID is required";
                return RedirectToAction("Index", "License");
            }

            try
            {
                var product = await _productService.GetProductAsync(productId);
                if (product == null)
                {
                    TempData["Error"] = "Product not found";
                    return RedirectToAction("Index", "License");
                }

                var consumers = await _consumerService.GetConsumersByProductAsync(productId);
                var licenses = await _licenseService.GetLicensesByProductAsync(productId);

                var viewModel = new ProductLicenseViewModel
                {
                    Product = product,
                    Consumers = consumers.Select(c => new ConsumerLicenseViewModel
                    {
                        Consumer = c,
                        Licenses = licenses.Where(l => l.ConsumerId == c.ConsumerId).ToList()
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product licenses for {ProductId}", productId);
                TempData["Error"] = "Failed to load product licenses";
                return RedirectToAction("Index", "License");
            }
        }

        /// <summary>
        /// Get product licenses as JSON for AJAX calls
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProductLicenses(string productId)
        {
            try
            {
                var product = await _productService.GetProductAsync(productId);
                if (product == null)
                {
                    return Json(new { success = false, message = "Product not found" });
                }

                var consumers = await _consumerService.GetConsumersByProductAsync(productId);
                var licenses = await _licenseService.GetLicensesByProductAsync(productId);

                var result = new
                {
                    success = true,
                    product = product,
                    consumers = consumers.Select(c => new
                    {
                        consumer = c,
                        licenses = licenses.Where(l => l.ConsumerId == c.ConsumerId).Select(l => new
                        {
                            licenseId = l.LicenseId,
                            validFrom = l.ValidFrom,
                            validTo = l.ValidTo,
                            tier = l.Tier,
                            isActive = l.Status == LicenseStatus.Active && l.ValidTo > DateTime.UtcNow,
                            isExpired = l.ValidTo < DateTime.UtcNow
                        })
                    })
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product licenses for {ProductId}", productId);
                return Json(new { success = false, message = "Failed to load product licenses" });
            }
        }
    }
}
