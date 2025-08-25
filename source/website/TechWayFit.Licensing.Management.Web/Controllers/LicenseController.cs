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
using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Web.Helpers;
using TechWayFit.Licensing.Management.Core.Models.Enums;

namespace TechWayFit.Licensing.Management.Web.Controllers
{
    /// <summary>
    /// License Management Controller - Step 5 Implementation
    /// Handles license generation, validation, and lifecycle management
    /// </summary>
    [Authorize]
    public class LicenseController : BaseController
    {
        private readonly ILogger<LicenseController> _logger;
        private readonly IProductLicenseService _licenseService;
        private readonly IEnterpriseProductService _productService;
        private readonly IConsumerAccountService _consumerService;
        private readonly IProductActivationService _productActivationService;
        private readonly ILicenseFileService _licenseFileService;
        private readonly IProductTierService _productTierService;

        public LicenseController(
            ILogger<LicenseController> logger,
            IProductLicenseService licenseService,
            IEnterpriseProductService productService,
            IConsumerAccountService consumerService,
            IProductActivationService productActivationService,
            ILicenseFileService licenseFileService,
            IProductTierService productTierService)
        {
            _logger = logger;
            _licenseService = licenseService;
            _productService = productService;
            _consumerService = consumerService;
            _productActivationService = productActivationService;
            _licenseFileService = licenseFileService;
            _productTierService = productTierService;
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
                        LicenseCode = l.LicenseCode ?? "N/A",
                        ConsumerName = l.LicenseConsumer?.Consumer?.CompanyName ?? "Unknown Consumer",
                        ContactEmail = l.LicenseConsumer?.Consumer?.PrimaryContact?.Email ?? "No Email",
                        Tier = LicenseTier.Community, // Default value since ProductLicense doesn't have LicenseTier
                        Status = l.Status,
                        ValidFrom = l.ValidFrom,
                        ValidTo = l.ValidTo,
                        CreatedAt = l.CreatedAt,
                        CreatedBy = l.IssuedBy ?? "System", // Using IssuedBy instead of CreatedBy
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
                return View(new LicenseListViewModel
                {
                    Licenses = new List<LicenseItemViewModel>(),
                    Filter = filter ?? new LicenseFilterViewModel(),
                    Pagination = new PaginationViewModel
                    {
                        CurrentPage = page,
                        TotalPages = 1,
                        TotalItems = 0,
                        PageSize = pageSize
                    }
                });
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

                // Additional validation for required fields
                var validationErrors = await ValidateLicenseCreationRequest(model);
                if (validationErrors.Any())
                {
                    foreach (var error in validationErrors)
                    {
                        ModelState.AddModelError(error.Key, error.Value);
                    }
                    await PopulateCreateLicenseDropdowns(model);
                    return View(model);
                }

                // Map ViewModel to Core request model
                var licenseRequest = new LicenseGenerationRequest
                {
                    ProductId = model.ProductId.ToGuid(),
                    ConsumerId = model.ConsumerId.ToGuid(),
                    TierId = model.ProductTierId.ToGuid(),
                    LicenseModel = model.LicenseModel,
                    ExpiryDate = model.ValidTo, // Use ValidTo as ExpiryDate
                    MaxUsers = model.MaxAllowedUsers ?? (int?)model.MaxApiCallsPerMonth, // Map MaxAllowedUsers first, fallback to API calls
                    MaxDevices = model.MaxConcurrentConnections,
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
                        ["ValidFrom"] = model.ValidFrom.ToString("O"), // Store ValidFrom in metadata
                        ["LicenseModel"] = model.LicenseModel.ToString(),
                        ["MaxAllowedUsers"] = model.MaxAllowedUsers?.ToString() ?? ""
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
                        Consumer = consumer ?? new ConsumerAccount(), // Provide fallback
                        Product = product ?? new EnterpriseProduct() // Provide fallback
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
        /// Get product tiers for a specific product (AJAX endpoint)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProductTiers(string productId)
        {
            try
            {
                if (string.IsNullOrEmpty(productId))
                {
                    return Json(new { success = false, message = "Product ID is required" });
                }

                var productGuid = productId.ToGuid();
                if (productGuid == Guid.Empty)
                {
                    return Json(new { success = false, message = "Invalid Product ID" });
                }

                var tiers = await _productTierService.GetTiersByProductAsync(productGuid);
                var tierOptions = tiers.Select(t => new
                {
                    value = t.TierId.ConvertToString(),
                    text = t.Name
                }).ToList();

                return Json(new { success = true, tiers = tierOptions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product tiers for product {ProductId}", productId);
                return Json(new { success = false, message = "Error loading product tiers" });
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
        /// Download License - Generate and download license file in specified format
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Download(Guid id, string format = "lic")
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

                // Normalize format parameter
                format = format?.ToLowerInvariant() ?? "lic";

                string fileContent;
                string fileName;
                string contentType;

                // Generate content based on format
                switch (format)
                {
                    case "lic":
                        fileContent = await _licenseFileService.GenerateLicenseFileAsync(license);
                        fileName = $"License_{license.LicenseCode}_{DateTime.UtcNow:yyyyMMdd}.lic";
                        contentType = "application/octet-stream";
                        break;

                    case "json":
                        fileContent = await _licenseFileService.GenerateJsonLicenseFileAsync(license);
                        fileName = $"License_{license.LicenseCode}_{DateTime.UtcNow:yyyyMMdd}.json";
                        contentType = "application/json";
                        break;

                    case "xml":
                        fileContent = await _licenseFileService.GenerateXmlLicenseFileAsync(license);
                        fileName = $"License_{license.LicenseCode}_{DateTime.UtcNow:yyyyMMdd}.xml";
                        contentType = "application/xml";
                        break;

                    case "zip":
                        // For ZIP format, we use the existing package generation method
                        var packageBytes = await _licenseFileService.GenerateLicensePackageAsync(license);
                        fileName = $"License_{license.LicenseCode}_{DateTime.UtcNow:yyyyMMdd}.zip";
                        
                        // Track download
                        await _licenseFileService.TrackDownloadAsync(license.LicenseId, User.Identity?.Name ?? "Anonymous", format);
                        _logger.LogInformation("License {LicenseId} downloaded as {Format} by user {User}", id, format.ToUpperInvariant(), User.Identity?.Name);
                        
                        return File(packageBytes, "application/zip", fileName);

                    default:
                        TempData["ErrorMessage"] = $"Unsupported format: {format}. Supported formats are: lic, json, xml, zip.";
                        return RedirectToAction(nameof(Details), new { id });
                }

                // Track download
                await _licenseFileService.TrackDownloadAsync(license.LicenseId, User.Identity?.Name ?? "Anonymous", format);

                // Log the download action
                _logger.LogInformation("License {LicenseId} downloaded as {Format} by user {User}", id, format.ToUpperInvariant(), User.Identity?.Name);

                // Return file for download
                var fileBytes = System.Text.Encoding.UTF8.GetBytes(fileContent);
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading license {LicenseId} in format {Format}", id, format);
                TempData["ErrorMessage"] = "An error occurred while downloading the license.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        /// <summary>
        /// Download Statistics for a license
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DownloadStats(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return Json(new { success = false, message = "License ID is required." });
                }

                var stats = await _licenseFileService.GetDownloadStatsAsync(id);
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting download stats for license {LicenseId}", id);
                return Json(new { success = false, message = "An error occurred while retrieving download statistics." });
            }
        }

        /// <summary>
        /// Bulk export multiple licenses
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BulkExport([FromBody] BulkExportRequest request)
        {
            try
            {
                if (request?.LicenseIds?.Any() != true)
                {
                    return Json(new { success = false, message = "No licenses selected for export." });
                }

                // Get all selected licenses
                var licenses = new List<ProductLicense>();
                foreach (var licenseId in request.LicenseIds)
                {
                    var license = await _licenseService.GetLicenseByIdAsync(licenseId);
                    if (license != null && license.Status == LicenseStatus.Active)
                    {
                        licenses.Add(license);
                    }
                }

                if (!licenses.Any())
                {
                    return Json(new { success = false, message = "No valid licenses found for export." });
                }

                // Generate bulk export
                var exportBytes = await _licenseFileService.GenerateBulkExportAsync(licenses, request.Format ?? "zip");
                var fileName = $"BulkExport_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip";

                _logger.LogInformation("Bulk export of {Count} licenses by user {User}", licenses.Count, User.Identity?.Name);

                return File(exportBytes, "application/zip", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk export");
                return Json(new { success = false, message = "An error occurred during bulk export." });
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
            foreach (var feature in license.LicenseConsumer.Features)
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
                Features = license.LicenseConsumer.Features.Select(f => new
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

                // Get product tiers - if a specific product is selected, get tiers for that product
                var productTiers = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
                if (!string.IsNullOrEmpty(model.ProductId))
                {
                    var productGuid = model.ProductId.ToGuid();
                    if (productGuid != Guid.Empty)
                    {
                        var tiers = await _productTierService.GetTiersByProductAsync(productGuid);
                        productTiers = tiers.Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                        {
                            Value = t.TierId.ConvertToString(),
                            Text = t.Name,
                            Selected = t.TierId.ConvertToString() == model.ProductTierId
                        }).ToList();
                    }
                }

                // Add default option if no tiers found
                if (!productTiers.Any())
                {
                    productTiers.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = "",
                        Text = "Select a Product first",
                        Selected = true
                    });
                }

                ViewBag.AvailableProductTiers = productTiers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error populating create license dropdowns");
                // Initialize empty lists to prevent null reference exceptions
                ViewBag.AvailableProducts = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
                ViewBag.AvailableConsumers = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
                ViewBag.AvailableProductTiers = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
                {
                    new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = "",
                        Text = "Error loading tiers",
                        Selected = true
                    }
                };
            }
        }

        #region ProductKey Management

        /// <summary>
        /// Display ProductKeys for a specific license
        /// </summary>
        public async Task<IActionResult> ProductKeys(string licenseId, int page = 1, int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrEmpty(licenseId))
                {
                    TempData["ErrorMessage"] = "License ID is required.";
                    return RedirectToAction(nameof(Index));
                }

                // Get the license details
                var license = await _licenseService.GetLicenseByIdAsync(Guid.Parse(licenseId));
                if (license == null)
                {
                    TempData["ErrorMessage"] = "License not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Get ProductKeys for this license by querying all activations and filtering
                // Since we don't have a direct method, we'll use a placeholder implementation
                var licenseActivations = new List<ProductActivationDetails>();
                
                // TODO: Implement proper method to get activations by license ID
                // For now, return empty list with proper structure

                var totalItems = licenseActivations.Count;
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                // Convert to ProductKey items
                var productKeyItems = licenseActivations
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new ProductKeyItemViewModel
                    {
                        Id = a.ActivationId,
                        ProductKey = a.ProductKey ?? string.Empty,
                        ClientIdentifier = a.MachineId ?? string.Empty,
                        Status = a.Status,
                        CreatedAt = a.ActivationDate,
                        ActivationDate = a.ActivationDate,
                        ActivationEndDate = a.ActivationEndDate,
                        ActivationSignature = a.ActivationSignature,
                        MachineId = a.MachineId,
                        MachineName = a.MachineName,
                        LastHeartbeat = a.LastHeartbeat,
                        Description = "ProductKey activation"
                    }).ToList();

                // Calculate statistics
                var stats = new ProductKeyStatsViewModel
                {
                    TotalKeys = totalItems,
                    ActiveKeys = licenseActivations.Count(a => a.Status == ProductActivationStatus.Active),
                    PendingKeys = licenseActivations.Count(a => a.Status == ProductActivationStatus.PendingActivation),
                    ExpiredKeys = licenseActivations.Count(a => a.Status == ProductActivationStatus.Expired),
                    RevokedKeys = licenseActivations.Count(a => a.Status == ProductActivationStatus.Revoked)
                };

                var model = new LicenseProductKeysViewModel
                {
                    License = license,
                    ProductKeys = productKeyItems,
                    Stats = stats,
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
                _logger.LogError(ex, "Error loading ProductKeys for license {LicenseId}", licenseId);
                TempData["ErrorMessage"] = "Failed to load ProductKeys. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Generate new ProductKey for a license
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GenerateProductKey(string licenseId, int quantity = 1)
        {
            try
            {
                if (string.IsNullOrEmpty(licenseId))
                {
                    TempData["ErrorMessage"] = "License ID is required.";
                    return RedirectToAction(nameof(Index));
                }

                var license = await _licenseService.GetLicenseByIdAsync(Guid.Parse(licenseId));
                if (license == null)
                {
                    TempData["ErrorMessage"] = "License not found.";
                    return RedirectToAction(nameof(Index));
                }

                // TODO: Implement ProductKey generation
                // For now, show a placeholder message
                TempData["InfoMessage"] = "ProductKey generation feature will be implemented.";

                return RedirectToAction(nameof(ProductKeys), new { licenseId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating ProductKey for license {LicenseId}", licenseId);
                TempData["ErrorMessage"] = "Failed to generate ProductKey. Please try again.";
                return RedirectToAction(nameof(ProductKeys), new { licenseId });
            }
        }

        /// <summary>
        /// Deactivate a ProductKey
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeactivateProductKey(string licenseId, string productKey)
        {
            try
            {
                if (string.IsNullOrEmpty(licenseId) || string.IsNullOrEmpty(productKey))
                {
                    TempData["ErrorMessage"] = "License ID and ProductKey are required.";
                    return RedirectToAction(nameof(ProductKeys), new { licenseId });
                }

                // Get current user for audit
                var currentUser = User.Identity?.Name ?? "System";
                
                var success = await _productActivationService.DeactivateProductKeyAsync(
                    productKey, 
                    currentUser, 
                    "Deactivated by admin");

                if (success)
                {
                    TempData["SuccessMessage"] = "ProductKey deactivated successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to deactivate ProductKey.";
                }

                return RedirectToAction(nameof(ProductKeys), new { licenseId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating ProductKey {ProductKey}", productKey);
                TempData["ErrorMessage"] = "Failed to deactivate ProductKey. Please try again.";
                return RedirectToAction(nameof(ProductKeys), new { licenseId });
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Validates the license creation request for all required fields and business rules
        /// </summary>
        private async Task<Dictionary<string, string>> ValidateLicenseCreationRequest(LicenseGenerationViewModel model)
        {
            var errors = new Dictionary<string, string>();

            try
            {
                // Validate ProductId
                if (string.IsNullOrWhiteSpace(model.ProductId) || !Guid.TryParse(model.ProductId, out var productId) || productId == Guid.Empty)
                {
                    errors.Add("ProductId", "Please select a valid product.");
                }
                else
                {
                    // Check if product exists
                    var product = await _productService.GetProductByIdAsync(productId);
                    if (product == null)
                    {
                        errors.Add("ProductId", "Selected product does not exist.");
                    }
                }

                // Validate ConsumerId
                if (string.IsNullOrWhiteSpace(model.ConsumerId) || !Guid.TryParse(model.ConsumerId, out var consumerId) || consumerId == Guid.Empty)
                {
                    errors.Add("ConsumerId", "Please select a valid consumer.");
                }
                else
                {
                    // Check if consumer exists
                    var consumer = await _consumerService.GetConsumerAccountByIdAsync(consumerId);
                    if (consumer == null)
                    {
                        errors.Add("ConsumerId", "Selected consumer does not exist.");
                    }
                }

                // Validate ProductTierId
                if (string.IsNullOrWhiteSpace(model.ProductTierId) || !Guid.TryParse(model.ProductTierId, out var tierId) || tierId == Guid.Empty)
                {
                    errors.Add("ProductTierId", "Please select a valid product tier.");
                }
                else if (Guid.TryParse(model.ProductId, out var prodId))
                {
                    // Check if tier exists and belongs to the selected product
                    var tiers = await _productTierService.GetTiersByProductAsync(prodId);
                    if (!tiers.Any(t => t.TierId == tierId))
                    {
                        errors.Add("ProductTierId", "Selected tier does not belong to the selected product.");
                    }
                }

                // Validate date ranges
                if (model.ValidFrom >= model.ValidTo)
                {
                    errors.Add("ValidTo", "Valid To date must be after Valid From date.");
                }

                if (model.ValidFrom < DateTime.Now.Date)
                {
                    errors.Add("ValidFrom", "Valid From date cannot be in the past.");
                }

                // Validate required contact information
                if (string.IsNullOrWhiteSpace(model.LicensedTo))
                {
                    errors.Add("LicensedTo", "Licensed To field is required.");
                }

                if (string.IsNullOrWhiteSpace(model.ContactPerson))
                {
                    errors.Add("ContactPerson", "Contact Person field is required.");
                }

                if (string.IsNullOrWhiteSpace(model.ContactEmail))
                {
                    errors.Add("ContactEmail", "Contact Email field is required.");
                }
                else if (!IsValidEmail(model.ContactEmail))
                {
                    errors.Add("ContactEmail", "Please enter a valid email address.");
                }

                // Validate secondary email if provided
                if (!string.IsNullOrWhiteSpace(model.SecondaryContactEmail) && !IsValidEmail(model.SecondaryContactEmail))
                {
                    errors.Add("SecondaryContactEmail", "Please enter a valid secondary email address.");
                }

                // Validate license model specific requirements
                if (model.LicenseModel == LicenseType.VolumetricLicense && (!model.MaxAllowedUsers.HasValue || model.MaxAllowedUsers <= 0))
                {
                    errors.Add("MaxAllowedUsers", "Max Allowed Users must be greater than 0 for Volumetric licenses.");
                }

                // Validate API call limits if provided
                if (model.MaxApiCallsPerMonth.HasValue && model.MaxApiCallsPerMonth <= 0)
                {
                    errors.Add("MaxApiCallsPerMonth", "Max API Calls Per Month must be greater than 0 if specified.");
                }

                // Validate concurrent connections if provided
                if (model.MaxConcurrentConnections.HasValue && model.MaxConcurrentConnections <= 0)
                {
                    errors.Add("MaxConcurrentConnections", "Max Concurrent Connections must be greater than 0 if specified.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during license creation validation");
                errors.Add("General", "An error occurred during validation. Please try again.");
            }

            return errors;
        }

        /// <summary>
        /// Validates email format
        /// </summary>
        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #endregion
    }
}
