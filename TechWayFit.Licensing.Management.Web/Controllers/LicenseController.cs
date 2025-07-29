using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Core.Models; 
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.WebUI.ViewModels.License;
using TechWayFit.Licensing.WebUI.ViewModels.Dashboard;
using TechWayFit.Licensing.WebUI.Models;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Web.Extensions;
using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.WebUI.Controllers
{
    /// <summary>
    /// License Management Controller - Step 5 Implementation
    /// Handles license generation, validation, and lifecycle management
    /// </summary>
    [Authorize]
    public class LicenseController : Controller
    {
        private readonly ILogger<LicenseController> _logger;
        private readonly IProductLicenseService _licenseService;
        private readonly IEnterpriseProductService _productService;
        private readonly IConsumerAccountService _consumerService;

        public LicenseController(
            ILogger<LicenseController> logger,
            IProductLicenseService licenseService,
            IEnterpriseProductService productService,
            IConsumerAccountService consumerService)
        {
            _logger = logger;
            _licenseService = licenseService;
            _productService = productService;
            _consumerService = consumerService;
        }

        /// <summary>
        /// License Listing - Show all licenses with filtering and pagination
        /// </summary>
        public async Task<IActionResult> Index(LicenseFilterViewModel filter, int page = 1, int pageSize = 10)
        {
            try
            {
                // Initialize filter if null
                filter ??= new LicenseFilterViewModel();

                // Use the GetLicensesAsync method from the service
                var allLicenses = await _licenseService.GetLicensesAsync(
                    status: filter.Status,
                    searchTerm: filter.SearchTerm,
                    pageNumber: 1, // Get all for client-side filtering for now
                    pageSize: 10000 // Large number to get all licenses
                );

                // Apply additional filters that aren't handled by the service method
                var filteredLicenses = allLicenses.AsQueryable();

                // Note: ProductLicense doesn't have LicenseTier property, so we'll skip this filter for now
                // if (filter.Tier.HasValue)
                // {
                //     filteredLicenses = filteredLicenses.Where(l => l.LicenseTier == filter.Tier.Value);
                // }

                if (filter.ValidFromStart.HasValue)
                {
                    filteredLicenses = filteredLicenses.Where(l => l.ValidFrom >= filter.ValidFromStart.Value);
                }

                if (filter.ValidFromEnd.HasValue)
                {
                    filteredLicenses = filteredLicenses.Where(l => l.ValidFrom <= filter.ValidFromEnd.Value);
                }

                if (filter.ValidToStart.HasValue)
                {
                    filteredLicenses = filteredLicenses.Where(l => l.ValidTo >= filter.ValidToStart.Value);
                }

                if (filter.ValidToEnd.HasValue)
                {
                    filteredLicenses = filteredLicenses.Where(l => l.ValidTo <= filter.ValidToEnd.Value);
                }

                if (filter.ShowExpiring)
                {
                    var expiryDate = DateTime.UtcNow.AddDays(filter.ExpiringWithinDays);
                    filteredLicenses = filteredLicenses.Where(l => 
                        l.ValidTo <= expiryDate && l.ValidTo > DateTime.UtcNow && l.Status == LicenseStatus.Active);
                }

                var filteredList = filteredLicenses.OrderByDescending(l => l.CreatedAt).ToList();
                var totalItems = filteredList.Count;
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                // Convert to view models
                var licenseItems = filteredList
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(l => new LicenseItemViewModel
                    {
                        LicenseId = l.LicenseId,
                        LicenseCode = l.LicenseCode,
                        ConsumerName = l.LicenseConsumer.Consumer.CompanyName,
                        ContactEmail = l.LicenseConsumer.Consumer.PrimaryContact.Email,
                        Tier = LicenseTier.Community, // Default value since ProductLicense doesn't have LicenseTier
                        Status = l.Status,
                        ValidFrom = l.ValidFrom,
                        ValidTo = l.ValidTo,
                        CreatedAt = l.CreatedAt,
                        CreatedBy = l.IssuedBy, // Using IssuedBy instead of CreatedBy
                        Version = 1, // Default version since ProductLicense doesn't have Version property
                        DaysUntilExpiry = (int)(l.ValidTo - DateTime.UtcNow).TotalDays,
                        CanRenew = l.Status == LicenseStatus.Active && l.ValidTo <= DateTime.UtcNow.AddDays(30),
                        CanRevoke = l.Status == LicenseStatus.Active,
                        CanSuspend = l.Status == LicenseStatus.Active
                    }).ToList();

                var model = new LicenseListViewModel
                {
                    Licenses = licenseItems,
                    Filter = filter,
                    Pagination = new PaginationViewModel
                    {
                        CurrentPage = page,
                        TotalPages = totalPages,
                        TotalItems = totalItems,
                        PageSize = pageSize
                    }
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading license dashboard");
                TempData["ErrorMessage"] = "Failed to load license dashboard. Please try again.";
                return View(new DashboardViewModel());
            }
        }

        /// <summary>
        /// Show license creation form
        /// </summary>
        public async Task<IActionResult> Create(string? productId = null, string? consumerId = null)
        {
            try
            {
                var model = new LicenseGenerationViewModel
                {
                    ProductId = productId ?? string.Empty,
                    ConsumerId = consumerId ?? string.Empty,
                    CreatedBy = User.Identity?.Name ?? "System"
                };

                await PopulateCreateLicenseDropdowns(model);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading license creation form");
                TempData["ErrorMessage"] = "Failed to load license creation form. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Process license creation
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LicenseGenerationViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await PopulateCreateLicenseDropdowns(model);
                    return View(model);
                }

                // Map ViewModel to Core request model
                var licenseRequest = new LicenseGenerationRequest
                {
                    ProductId = model.ProductId,
                    ConsumerId = model.ConsumerId,
                    TierId = model.Tier.ToString(),
                    ExpiryDate = model.ValidTo, // Use ValidTo as ExpiryDate
                    MaxUsers = (int?)model.MaxApiCallsPerMonth, // Map API calls to Max Users for now
                    MaxDevices = model.MaxConcurrentConnections,
                    AllowOfflineUsage = false,
                    AllowVirtualization = false,
                    Notes = $"Generated via Web UI by {User.Identity?.Name}",
                    CustomProperties = model.Metadata.ToDictionary(kv => kv.Key, kv => (object)kv.Value),
                    Metadata = new Dictionary<string, object>
                    {
                        ["LicensedTo"] = model.LicensedTo,
                        ["ContactPerson"] = model.ContactPerson,
                        ["ContactEmail"] = model.ContactEmail,
                        ["SecondaryContactPerson"] = model.SecondaryContactPerson ?? "",
                        ["SecondaryContactEmail"] = model.SecondaryContactEmail ?? "",
                        ["SelectedFeatures"] = string.Join(",", model.SelectedFeatures),
                        ["CreatedBy"] = model.CreatedBy,
                        ["CreatedAt"] = DateTime.UtcNow.ToString("O"),
                        ["ValidFrom"] = model.ValidFrom.ToString("O") // Store ValidFrom in metadata
                    }
                };

                var license = await _licenseService.GenerateLicenseAsync(licenseRequest, User.Identity?.Name ?? "System");

                if (license != null)
                {
                    // Get additional data for the view
                    var product = await _productService.GetProductByIdAsync(model.ProductId);
                    var consumer = await _consumerService.GetConsumerAccountByIdAsync(model.ConsumerId);

                    // Return view with success
                    var viewModel = new LicenseDetailViewModel
                    {
                        License = license,
                        Consumer = consumer,
                        Product = product
                    };

                    TempData["SuccessMessage"] = "License generated successfully!";
                    return View("Details", viewModel);
                }

                ModelState.AddModelError("", "Failed to generate license. Please check your inputs and try again.");
                await PopulateCreateLicenseDropdowns(model);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating license");
                ModelState.AddModelError("", "An error occurred while creating the license. Please try again.");
                await PopulateCreateLicenseDropdowns(model);
                return View(model);
            }
        }

        /// <summary>
        /// Show license details
        /// </summary>
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return NotFound("License ID is required");
                }

                var license = await _licenseService.GetLicenseByIdAsync(id);
                if (license == null)
                {
                    return NotFound($"License with ID '{id}' not found");
                }

                // Get related data
                var product = license.LicenseConsumer.Product;
                var consumer = license.LicenseConsumer.Consumer;

                var model = new LicenseDetailViewModel
                {
                    License = license,
                    Consumer = consumer,
                    Product = product,
                    CanEdit = true,
                    CanRenew = license.Status == LicenseStatus.Active || license.Status == LicenseStatus.GracePeriod,
                    CanRevoke = license.Status == LicenseStatus.Active || license.Status == LicenseStatus.GracePeriod,
                    CanSuspend = license.Status == LicenseStatus.Active,
                    CanReactivate = license.Status == LicenseStatus.Suspended
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading license details for ID: {LicenseId}", id);
                TempData["ErrorMessage"] = "Failed to load license details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Show license validation form
        /// </summary>
        public IActionResult Validate()
        {
            return View(new LicenseValidationViewModel());
        }

        /// <summary>
        /// Process license validation
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Validate(LicenseValidationViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // TODO: Implement license validation logic when service is available
                await Task.CompletedTask; // Remove warning about missing await
                model.IsValid = false;
                model.ValidationMessage = "License validation functionality is not yet implemented.";

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating license");
                model.IsValid = false;
                model.ValidationMessage = "An error occurred during validation.";
                return View(model);
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Populate dropdowns for license creation form
        /// </summary>
        private async Task PopulateCreateLicenseDropdowns(LicenseGenerationViewModel model)
        {
            try
            {
                // Get products
                var products = await _productService.GetProductsAsync(pageNumber: 1, pageSize: 100);
                ViewBag.AvailableProducts = products.Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = p.ProductId,
                    Text = p.Name,
                    Selected = p.ProductId == model.ProductId
                }).ToList();

                // Get consumers
                var consumers = await _consumerService.GetConsumerAccountsAsync(pageNumber: 1, pageSize: 100);
                ViewBag.AvailableConsumers = consumers.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.ConsumerId,
                    Text = c.CompanyName,
                    Selected = c.ConsumerId == model.ConsumerId
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error populating create license dropdowns");
                // Initialize empty lists to prevent null reference exceptions
                ViewBag.AvailableProducts = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
                ViewBag.AvailableConsumers = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
            }
        }

        #endregion
    }
}
