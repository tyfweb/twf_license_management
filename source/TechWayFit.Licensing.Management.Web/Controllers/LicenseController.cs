using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Core.Models; 
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Web.ViewModels.License;
using TechWayFit.Licensing.Management.Web.ViewModels.Dashboard;
using TechWayFit.Licensing.Management.Web.Models;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Web.Extensions;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Web.Helpers;

namespace TechWayFit.Licensing.Management.Web.Controllers
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
                        LicenseId = l.LicenseId.ConvertToString(),
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
                    ProductId = model.ProductId.ToGuid(),
                    ConsumerId = model.ConsumerId.ToGuid(),
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
                    var product = await _productService.GetProductByIdAsync(model.ProductId.ToGuid());
                    var consumer = await _consumerService.GetConsumerAccountByIdAsync(model.ConsumerId.ToGuid());

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
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
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
        /// Download License Key - Generate and download license file
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Download(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    TempData["ErrorMessage"] = "License ID is required.";
                    return RedirectToAction(nameof(Index));
                }

                // Get the license details
                var license = await _licenseService.GetLicenseByIdAsync(id);
                if (license == null)
                {
                    TempData["ErrorMessage"] = "License not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if license is valid for download
                if (license.Status != LicenseStatus.Active)
                {
                    TempData["ErrorMessage"] = "Only active licenses can be downloaded.";
                    return RedirectToAction(nameof(Index));
                }

                // Generate license file content
                var licenseFileContent = GenerateLicenseFileContent(license);
                var fileName = $"License_{license.LicenseCode}_{DateTime.UtcNow:yyyyMMdd}.lic";

                // Log the download action
                _logger.LogInformation("License {LicenseId} downloaded by user", id);

                // Return file for download
                var fileBytes = System.Text.Encoding.UTF8.GetBytes(licenseFileContent);
                return File(fileBytes, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading license {LicenseId}", id);
                TempData["ErrorMessage"] = "An error occurred while downloading the license.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Download License as JSON format
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DownloadJson(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    TempData["ErrorMessage"] = "License ID is required.";
                    return RedirectToAction(nameof(Index));
                }

                var license = await _licenseService.GetLicenseByIdAsync(id);
                if (license == null)
                {
                    TempData["ErrorMessage"] = "License not found.";
                    return RedirectToAction(nameof(Index));
                }

                if (license.Status != LicenseStatus.Active)
                {
                    TempData["ErrorMessage"] = "Only active licenses can be downloaded.";
                    return RedirectToAction(nameof(Index));
                }

                // Generate JSON license content
                var licenseJson = GenerateLicenseJsonContent(license);
                var fileName = $"License_{license.LicenseCode}_{DateTime.UtcNow:yyyyMMdd}.json";

                _logger.LogInformation("License {LicenseId} downloaded as JSON by user", id);

                var fileBytes = System.Text.Encoding.UTF8.GetBytes(licenseJson);
                return File(fileBytes, "application/json", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading license JSON {LicenseId}", id);
                TempData["ErrorMessage"] = "An error occurred while downloading the license.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Download License as ZIP bundle containing both formats
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DownloadZip(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    TempData["ErrorMessage"] = "License ID is required.";
                    return RedirectToAction(nameof(Index));
                }

                var license = await _licenseService.GetLicenseByIdAsync(id);
                if (license == null)
                {
                    TempData["ErrorMessage"] = "License not found.";
                    return RedirectToAction(nameof(Index));
                }

                if (license.Status != LicenseStatus.Active)
                {
                    TempData["ErrorMessage"] = "Only active licenses can be downloaded.";
                    return RedirectToAction(nameof(Index));
                }

                // Generate both license formats
                var licenseFileContent = GenerateLicenseFileContent(license);
                var licenseJsonContent = GenerateLicenseJsonContent(license);
                
                // Create ZIP file in memory
                using var memoryStream = new MemoryStream();
                using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
                {
                    // Add .lic file
                    var licenseEntry = archive.CreateEntry($"License_{license.LicenseCode}.lic");
                    using (var licenseStream = licenseEntry.Open())
                    {
                        var licenseBytes = System.Text.Encoding.UTF8.GetBytes(licenseFileContent);
                        await licenseStream.WriteAsync(licenseBytes, 0, licenseBytes.Length);
                    }

                    // Add .json file
                    var jsonEntry = archive.CreateEntry($"License_{license.LicenseCode}.json");
                    using (var jsonStream = jsonEntry.Open())
                    {
                        var jsonBytes = System.Text.Encoding.UTF8.GetBytes(licenseJsonContent);
                        await jsonStream.WriteAsync(jsonBytes, 0, jsonBytes.Length);
                    }

                    // Add README file with instructions
                    var readmeEntry = archive.CreateEntry("README.txt");
                    using (var readmeStream = readmeEntry.Open())
                    {
                        var readmeContent = GenerateReadmeContent(license);
                        var readmeBytes = System.Text.Encoding.UTF8.GetBytes(readmeContent);
                        await readmeStream.WriteAsync(readmeBytes, 0, readmeBytes.Length);
                    }
                }

                var fileName = $"License_{license.LicenseCode}_{DateTime.UtcNow:yyyyMMdd}.zip";
                _logger.LogInformation("License {LicenseId} downloaded as ZIP bundle by user", id);

                return File(memoryStream.ToArray(), "application/zip", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading license ZIP {LicenseId}", id);
                TempData["ErrorMessage"] = "An error occurred while downloading the license bundle.";
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
        /// Generate license file content in proprietary format
        /// </summary>
        private string GenerateLicenseFileContent(ProductLicense license)
        {
            var content = new System.Text.StringBuilder();
            content.AppendLine("=== TechWayFit License File ===");
            content.AppendLine($"License ID: {license.LicenseId}");
            content.AppendLine($"License Code: {license.LicenseCode}");
            content.AppendLine($"Product: {license.LicenseConsumer.Product.Name}");
            content.AppendLine($"Version: {license.LicenseConsumer.Product.Version}");
            content.AppendLine($"Licensed To: {license.LicenseConsumer.Consumer.CompanyName}");
            content.AppendLine($"Contact: {license.LicenseConsumer.Consumer.PrimaryContact.Name}");
            content.AppendLine($"Email: {license.LicenseConsumer.Consumer.PrimaryContact.Email}");
            content.AppendLine($"Valid From: {license.ValidFrom:yyyy-MM-dd HH:mm:ss} UTC");
            content.AppendLine($"Valid To: {license.ValidTo:yyyy-MM-dd HH:mm:ss} UTC");
            content.AppendLine($"Status: {license.Status}");
            content.AppendLine($"Issued By: {license.IssuedBy}");
            content.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            content.AppendLine();
            
            content.AppendLine("=== Features ===");
            foreach (var feature in license.Features)
            {
                content.AppendLine($"- {feature.Name}: {(feature.IsEnabled ? "Enabled" : "Disabled")}");
                if (!string.IsNullOrEmpty(feature.Description))
                {
                    content.AppendLine($"  Description: {feature.Description}");
                }
            }
            content.AppendLine();
            
            content.AppendLine("=== License Key ===");
            content.AppendLine($"Key: {license.LicenseKey}");
            content.AppendLine($"Signature: {license.LicenseSignature}");
            content.AppendLine($"Encryption: {license.Encryption}");
            content.AppendLine($"Signature Algorithm: {license.Signature}");
            content.AppendLine();
            
            if (license.Metadata.Any())
            {
                content.AppendLine("=== Metadata ===");
                foreach (var meta in license.Metadata)
                {
                    content.AppendLine($"{meta.Key}: {meta.Value}");
                }
                content.AppendLine();
            }
            
            content.AppendLine("=== Important Notice ===");
            content.AppendLine("This license file is protected by copyright law and international treaties.");
            content.AppendLine("Unauthorized reproduction or distribution is prohibited.");
            content.AppendLine("This license is valid only for the specified product version and licensed entity.");
            content.AppendLine("=======================================");
            
            return content.ToString();
        }

        /// <summary>
        /// Generate license content in JSON format
        /// </summary>
        private string GenerateLicenseJsonContent(ProductLicense license)
        {
            var licenseData = new
            {
                LicenseInfo = new
                {
                    LicenseId = license.LicenseId,
                    LicenseCode = license.LicenseCode,
                    Status = license.Status.ToString(),
                    GeneratedAt = DateTime.UtcNow,
                    ValidFrom = license.ValidFrom,
                    ValidTo = license.ValidTo,
                    IssuedBy = license.IssuedBy,
                    CreatedAt = license.CreatedAt
                },
                Product = new
                {
                    Name = license.LicenseConsumer.Product.Name,
                    ProductId = license.LicenseConsumer.Product.ProductId,
                    Version = license.LicenseConsumer.Product.Version,
                    Description = license.LicenseConsumer.Product.Description
                },
                Licensee = new
                {
                    CompanyName = license.LicenseConsumer.Consumer.CompanyName,
                    ConsumerId = license.LicenseConsumer.Consumer.ConsumerId,
                    PrimaryContact = new
                    {
                        Name = license.LicenseConsumer.Consumer.PrimaryContact.Name,
                        Email = license.LicenseConsumer.Consumer.PrimaryContact.Email,
                        Phone = license.LicenseConsumer.Consumer.PrimaryContact.Phone
                    },
                    SecondaryContact = license.LicenseConsumer.Consumer.SecondaryContact != null ? new
                    {
                        Name = license.LicenseConsumer.Consumer.SecondaryContact.Name,
                        Email = license.LicenseConsumer.Consumer.SecondaryContact.Email,
                        Phone = license.LicenseConsumer.Consumer.SecondaryContact.Phone
                    } : null
                },
                Features = license.Features.Select(f => new
                {
                    FeatureId = f.FeatureId,
                    Name = f.Name,
                    Description = f.Description,
                    IsEnabled = f.IsEnabled,
                    Code = f.Code
                }),
                Security = new
                {
                    LicenseKey = license.LicenseKey,
                    Signature = license.LicenseSignature,
                    PublicKey = license.PublicKey,
                    Encryption = license.Encryption,
                    SignatureAlgorithm = license.Signature
                },
                Metadata = license.Metadata,
                Notice = new
                {
                    Copyright = "This license file is protected by copyright law and international treaties.",
                    Warning = "Unauthorized reproduction or distribution is prohibited.",
                    Validity = "This license is valid only for the specified product version and licensed entity."
                }
            };

            return System.Text.Json.JsonSerializer.Serialize(licenseData, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });
        }

        /// <summary>
        /// Generate README content for ZIP download
        /// </summary>
        private string GenerateReadmeContent(ProductLicense license)
        {
            var content = new System.Text.StringBuilder();
            content.AppendLine("TechWayFit License Package");
            content.AppendLine("==========================");
            content.AppendLine();
            content.AppendLine($"License ID: {license.LicenseId}");
            content.AppendLine($"License Code: {license.LicenseCode}");
            content.AppendLine($"Product: {license.LicenseConsumer.Product.Name}");
            content.AppendLine($"Licensed To: {license.LicenseConsumer.Consumer.CompanyName}");
            content.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            content.AppendLine();
            content.AppendLine("Package Contents:");
            content.AppendLine("================");
            content.AppendLine();
            content.AppendLine($"1. License_{license.LicenseCode}.lic");
            content.AppendLine("   - Standard license file format");
            content.AppendLine("   - Use this for most software integrations");
            content.AppendLine("   - Human-readable format with all license details");
            content.AppendLine();
            content.AppendLine($"2. License_{license.LicenseCode}.json");
            content.AppendLine("   - JSON format for API integrations");
            content.AppendLine("   - Machine-readable structured data");
            content.AppendLine("   - Ideal for automated license validation");
            content.AppendLine();
            content.AppendLine("3. README.txt (this file)");
            content.AppendLine("   - Package information and usage instructions");
            content.AppendLine();
            content.AppendLine("Usage Instructions:");
            content.AppendLine("==================");
            content.AppendLine();
            content.AppendLine("1. Extract all files to your application directory");
            content.AppendLine("2. Use the appropriate format based on your integration needs:");
            content.AppendLine("   - .lic file: For traditional license file validation");
            content.AppendLine("   - .json file: For REST API or modern application integration");
            content.AppendLine();
            content.AppendLine("3. Implement license validation in your application using the");
            content.AppendLine("   TechWayFit License Validation SDK or your custom validation logic");
            content.AppendLine();
            content.AppendLine("Important Notes:");
            content.AppendLine("===============");
            content.AppendLine();
            content.AppendLine("- Keep these license files secure and do not share them publicly");
            content.AppendLine("- The license is valid only for the specified product and entity");
            content.AppendLine("- Contact support if you need to transfer or modify the license");
            content.AppendLine("- Both files contain the same license information in different formats");
            content.AppendLine();
            content.AppendLine("Support:");
            content.AppendLine("========");
            content.AppendLine();
            content.AppendLine("For technical support or license-related questions:");
            if (!string.IsNullOrEmpty(license.LicenseConsumer.Product.SupportEmail))
            {
                content.AppendLine($"Email: {license.LicenseConsumer.Product.SupportEmail}");
            }
            if (!string.IsNullOrEmpty(license.LicenseConsumer.Product.SupportPhone))
            {
                content.AppendLine($"Phone: {license.LicenseConsumer.Product.SupportPhone}");
            }
            content.AppendLine();
            content.AppendLine("© TechWayFit - All rights reserved");
            content.AppendLine("This license package is protected by copyright law.");
            
            return content.ToString();
        }

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
                    Value = p.ProductId.ConvertToString(),
                    Text = p.Name,
                    Selected = p.ProductId.ConvertToString() == model.ProductId
                }).ToList();

                // Get consumers
                var consumers = await _consumerService.GetConsumerAccountsAsync(pageNumber: 1, pageSize: 100);
                ViewBag.AvailableConsumers = consumers.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.ConsumerId.ConvertToString(),
                    Text = c.CompanyName,
                    Selected = c.ConsumerId.ConvertToString() == model.ConsumerId
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
