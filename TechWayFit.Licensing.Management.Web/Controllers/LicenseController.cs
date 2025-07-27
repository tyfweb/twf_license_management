using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Core.Contracts;
using TechWayFit.Licensing.Generator.Services;
using TechWayFit.Licensing.WebUI.Models;
using TechWayFit.Licensing.WebUI.ViewModels.License;
using TechWayFit.Licensing.WebUI.ViewModels.Dashboard;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;

namespace TechWayFit.Licensing.WebUI.Controllers
{
    /// <summary>
    /// Controller for license generation and management with full lifecycle support
    /// </summary>
    [Authorize]
    public class LicenseController : Controller
    {
        private readonly LicenseGenerator _licenseGenerator;
        private readonly ILicenseLifecycleService _lifecycleService;
        private readonly ILicenseValidationService _validationService;
        private readonly IProductService _productService;
        private readonly IConsumerService _consumerService;
        private readonly ILogger<LicenseController> _logger;

        public LicenseController(
            LicenseGenerator licenseGenerator,
            ILicenseLifecycleService lifecycleService,
            ILicenseValidationService validationService,
            IProductService productService,
            IConsumerService consumerService,
            ILogger<LicenseController> logger)
        {
            _licenseGenerator = licenseGenerator;
            _lifecycleService = lifecycleService;
            _validationService = validationService;
            _productService = productService;
            _consumerService = consumerService;
            _logger = logger;
        }

        /// <summary>
        /// Display the main dashboard with products and license management
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                // Get all products for display
                var products = await _productService.GetAllProductsAsync();
                
                var dashboardModel = new DashboardViewModel
                {
                    Products = products.Select(p => new ProductSummaryViewModel
                    {
                        ProductId = p.ProductId,
                        Name = p.ProductName, // Use ProductName instead of Name
                        Description = p.ProductType.ToString(), // Use ProductType as description for now
                        IsActive = p.IsActive,
                        CreatedAt = p.CreatedAt,
                        ConsumerCount = 0 // Will be populated by service later
                    }).ToList(),
                    
                    LicenseGenerationModel = new Models.LicenseGenerationViewModel
                    {
                        // Use local time instead of UTC for better datetime-local input handling
                        ValidFrom = DateTime.Now,
                        ValidTo = DateTime.Now.AddYears(1),
                        Tier = LicenseTier.Community,
                        SelectedFeatures = new List<string>
                        {
                            "BasicApiGateway",
                            "BasicAuthentication",
                            "BasicRateLimiting"
                        }
                    }
                };

                return View(dashboardModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                TempData["Error"] = "Failed to load dashboard data";
                
                // Return empty model on error
                return View(new DashboardViewModel
                {
                    Products = new List<ProductSummaryViewModel>(),
                    LicenseGenerationModel = new Models.LicenseGenerationViewModel
                    {
                        ValidFrom = DateTime.Now,
                        ValidTo = DateTime.Now.AddYears(1),
                        Tier = LicenseTier.Community
                    }
                });
            }
        }

        /// <summary>
        /// List all licenses with lifecycle management options
        /// </summary>
        public IActionResult List(int page = 1, int pageSize = 10, string search = "")
        {
            try
            {
                // For now, use a placeholder until the service is fully implemented
                var viewModel = new LicenseListViewModel
                {
                    Licenses = new List<LicenseItemViewModel>()
                };

                TempData["Info"] = "License listing with lifecycle management is being implemented";
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading license list");
                TempData["Error"] = "Failed to load licenses";
                return View(new LicenseListViewModel());
            }
        }

        /// <summary>
        /// Show license details with lifecycle options
        /// </summary>
        public IActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                // For now, use a placeholder until the service is fully implemented
                TempData["Info"] = "License details with lifecycle management is being implemented";
                return View(new LicenseDetailViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading license details {LicenseId}", id);
                return NotFound();
            }
        }

        /// <summary>
        /// Show license renewal form
        /// </summary>
        public IActionResult Renew(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            TempData["Info"] = "License renewal functionality is being implemented";
            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>
        /// Show license suspension form
        /// </summary>
        public IActionResult Suspend(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            TempData["Info"] = "License suspension functionality is being implemented";
            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>
        /// Show license creation form for a specific product
        /// </summary>
        public async Task<IActionResult> Create(string productId)
        {
            try
            {
                if (string.IsNullOrEmpty(productId))
                {
                    TempData["Error"] = "Product ID is required";
                    return RedirectToAction("Index", "Product");
                }

                // Get the product to validate it exists
                var product = await _productService.GetProductAsync(productId);
                if (product == null)
                {
                    TempData["Error"] = "Product not found";
                    return RedirectToAction("Index", "Product");
                }

                // Get consumers for this product for the dropdown
                var consumers = await _consumerService.GetConsumersByProductAsync(productId);

                var model = new Models.LicenseGenerationViewModel
                {
                    ValidFrom = DateTime.Now,
                    ValidTo = DateTime.Now.AddYears(1),
                    Tier = LicenseTier.Community,
                    SelectedFeatures = new List<string>
                    {
                        "BasicApiGateway",
                        "BasicAuthentication",
                        "BasicRateLimiting"
                    }
                };

                // Store productId and consumers for the view
                ViewBag.ProductId = productId;
                ViewBag.ProductName = product.ProductName;
                ViewBag.AvailableConsumers = consumers.Select(c => new
                {
                    Value = c.ConsumerId,
                    Text = $"{c.OrganizationName} ({c.ContactPerson})"
                }).ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading license creation form for product {ProductId}", productId);
                TempData["Error"] = "Failed to load license creation form";
                return RedirectToAction("Index", "Product");
            }
        }

        /// <summary>
        /// Reactivate a suspended license
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reactivate(string id, string reason)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(reason))
                return BadRequest();

            TempData["Info"] = "License reactivation functionality is being implemented";
            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>
        /// Revoke a license
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Revoke(string id, string reason)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(reason))
                return BadRequest();

            TempData["Info"] = "License revocation functionality is being implemented";
            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>
        /// Generate a license based on the form data
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Generate(Models.LicenseGenerationViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("Index", model);
                }
                // Validate dates if they're still default values
                if (model.ValidFrom == default(DateTime) || model.ValidFrom.Year < 2000)
                {
                    _logger.LogWarning("ValidFrom date is invalid: {ValidFrom}", model.ValidFrom);
                    model.ValidFrom = DateTime.Now;
                }

                if (model.ValidTo == default(DateTime) || model.ValidTo.Year < 2000)
                {
                    _logger.LogWarning("ValidTo date is invalid: {ValidTo}", model.ValidTo);
                    model.ValidTo = DateTime.Now.AddYears(1);
                }

                // Ensure ValidTo is after ValidFrom
                if (model.ValidTo <= model.ValidFrom)
                {
                    model.ValidTo = model.ValidFrom.AddYears(1);
                }

                // Create license generation request
                var request = new LicenseGenerationRequest
                {
                    LicensedTo = model.LicensedTo,
                    ContactPerson = model.ContactPerson,
                    ContactEmail = model.ContactEmail,
                    SecondaryContactPerson = model.SecondaryContactPerson,
                    SecondaryContactEmail = model.SecondaryContactEmail,
                    ValidFrom = model.ValidFrom,
                    ValidTo = model.ValidTo,
                    Tier = model.Tier,
                    CustomFeatures = CreateFeatures(model.SelectedFeatures, model.Tier),
                    Metadata = new Dictionary<string, string>()
                };

                // Generate signed license
                var signedLicense = await _licenseGenerator.GenerateLicenseAsync(request);

                // Decode the license data to get the license object for display
                var licenseJson = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(signedLicense.LicenseData));

                // Fix the deserialization with proper JsonSerializerOptions
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                    // Add date time converter to handle the ISO format properly
                    Converters = { new JsonStringEnumConverter() }
                };

                // Log the raw JSON for debugging
                _logger.LogInformation("Raw license JSON: {LicenseJson}", licenseJson);

                var license = JsonSerializer.Deserialize<License>(licenseJson, jsonOptions);

                // Fallback: If dates are still empty after deserialization, use the original request dates
                if (license != null)
                {
                    _logger.LogInformation("Deserialized dates - ValidFrom: {ValidFrom}, ValidTo: {ValidTo}",
                        license.ValidFrom, license.ValidTo);

                    if (license.ValidFrom == default(DateTime))
                    {
                        _logger.LogWarning("ValidFrom was not deserialized properly, using request date");
                        license.ValidFrom = request.ValidFrom;
                    }

                    if (license.ValidTo == default(DateTime))
                    {
                        _logger.LogWarning("ValidTo was not deserialized properly, using request date");
                        license.ValidTo = request.ValidTo;
                    }
                }

                // Create result model
                var result = new Models.LicenseGenerationResultViewModel
                {
                    Success = true,
                    License = license,
                    SignedLicense = signedLicense,
                    LicenseJson = JsonSerializer.Serialize(signedLicense, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }),
                    FileName = $"license_{license?.LicensedTo?.Replace(" ", "_") ?? "unknown"}_{DateTime.UtcNow:yyyyMMdd}.json"
                };

                _logger.LogInformation("License generated successfully for {LicensedTo}", license?.LicensedTo ?? "unknown");

                return View("Result", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating license for {LicensedTo}", model.LicensedTo);

                var result = new Models.LicenseGenerationResultViewModel
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };

                return View("Result", result);
            }
        }

        /// <summary>
        /// Download the generated license as a JSON file
        /// </summary>
        [HttpGet]
        public IActionResult Download(string licenseJson, string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(licenseJson))
                {
                    return BadRequest("License data is required");
                }

                var jsonBytes = System.Text.Encoding.UTF8.GetBytes(licenseJson);
                return File(jsonBytes, "application/json", fileName ?? "license.json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading license file {FileName}", fileName);
                return BadRequest("Error generating download file");
            }
        }

        /// <summary>
        /// Validate an existing license
        /// </summary>
        public IActionResult Validate()
        {
            return View(new Models.LicenseValidationViewModel());
        }

        /// <summary>
        /// Process license validation
        /// </summary>
        [HttpPost]
        public IActionResult ValidateLicense(Models.LicenseValidationViewModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.LicenseJson))
                {
                    ModelState.AddModelError("LicenseJson", "License JSON is required");
                    return View("Validate", model);
                }

                var signedLicense = JsonSerializer.Deserialize<SignedLicense>(model.LicenseJson);
                if (signedLicense == null)
                {
                    throw new ArgumentException("Invalid license format");
                }

                // Decode the license data to get the license object
                var licenseJson = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(signedLicense.LicenseData));

                // Fix: Use proper JsonSerializerOptions for deserialization
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                    Converters = { new JsonStringEnumConverter() }
                };

                _logger.LogInformation("Validating license JSON: {LicenseJson}", licenseJson);

                var license = JsonSerializer.Deserialize<License>(licenseJson, jsonOptions);

                // Fix: Handle date deserialization issues
                if (license != null && (license.ValidFrom == default(DateTime) || license.ValidTo == default(DateTime)))
                {
                    try
                    {
                        var jsonDoc = JsonDocument.Parse(licenseJson);

                        if (jsonDoc.RootElement.TryGetProperty("validFrom", out var validFromElement))
                        {
                            if (DateTime.TryParse(validFromElement.GetString(), out var validFromParsed))
                            {
                                license.ValidFrom = validFromParsed;
                                _logger.LogInformation("Manually parsed ValidFrom: {ValidFrom}", validFromParsed);
                            }
                        }

                        if (jsonDoc.RootElement.TryGetProperty("validTo", out var validToElement))
                        {
                            if (DateTime.TryParse(validToElement.GetString(), out var validToParsed))
                            {
                                license.ValidTo = validToParsed;
                                _logger.LogInformation("Manually parsed ValidTo: {ValidTo}", validToParsed);
                            }
                        }
                    }
                    catch (Exception parseEx)
                    {
                        _logger.LogError(parseEx, "Error manually parsing dates from license JSON");
                    }
                }

                // Enhanced validation logic
                bool isValid = false;
                string validationMessage = "License is invalid";

                if (license != null)
                {
                    var now = DateTime.UtcNow;

                    _logger.LogInformation("License validation - Current: {Now}, ValidFrom: {ValidFrom}, ValidTo: {ValidTo}",
                        now, license.ValidFrom, license.ValidTo);

                    // Check if license dates are valid (not default values)
                    if (license.ValidFrom == default(DateTime) || license.ValidTo == default(DateTime))
                    {
                        validationMessage = "License has invalid date information";
                    }
                    // Check if license is within valid date range
                    else if (license.ValidFrom > now)
                    {
                        validationMessage = $"License is not yet valid. Valid from: {license.ValidFrom:yyyy-MM-dd HH:mm:ss}";
                    }
                    else if (license.ValidTo < now)
                    {
                        validationMessage = $"License has expired. Valid until: {license.ValidTo:yyyy-MM-dd HH:mm:ss}";
                    }
                    // Check if license has required fields
                    else if (string.IsNullOrEmpty(license.LicensedTo))
                    {
                        validationMessage = "License is missing required information (LicensedTo)";
                    }
                    else
                    {
                        isValid = true;
                        var remainingDays = (license.ValidTo - now).Days;
                        if (remainingDays > 30)
                        {
                            validationMessage = $"License is valid until {license.ValidTo:yyyy-MM-dd HH:mm:ss} ({remainingDays} days remaining)";
                        }
                        else if (remainingDays > 0)
                        {
                            validationMessage = $"License is valid but expires soon: {license.ValidTo:yyyy-MM-dd HH:mm:ss} ({remainingDays} days remaining)";
                        }
                        else
                        {
                            validationMessage = $"License expires today: {license.ValidTo:yyyy-MM-dd HH:mm:ss}";
                        }
                    }
                }

                model.IsValid = isValid;
                model.ValidationMessage = validationMessage;
                model.License = license;

                _logger.LogInformation("License validation completed: {IsValid} - {Message}", isValid, validationMessage);

                return View("Validate", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating license");
                model.IsValid = false;
                model.ValidationMessage = $"Error validating license: {ex.Message}";
                return View("Validate", model);
            }
        }

        /// <summary>
        /// Create feature list based on selected features and tier
        /// </summary>
        private List<LicenseFeature> CreateFeatures(List<string> selectedFeatures, LicenseTier tier)
        {
            var features = new List<LicenseFeature>();

            if (selectedFeatures.Contains("BasicApiGateway"))
            {
                features.Add(new LicenseFeature
                {
                    Name = "BasicApiGateway",
                    Description = "Basic API Gateway functionality",
                    IsEnabled = true,
                    Category = FeatureCategory.Core,
                    Limits = new FeatureLimits
                    {
                        MaxUsagePerMonth = tier == LicenseTier.Community ? 10000 : null,
                        MaxConcurrentUsage = tier == LicenseTier.Community ? 10 :
                                           tier == LicenseTier.Professional ? 50 : 100
                    },
                    MinimumTier = LicenseTier.Community,
                    AddedAt = DateTime.UtcNow
                });
            }

            if (selectedFeatures.Contains("BasicAuthentication"))
            {
                features.Add(new LicenseFeature
                {
                    Name = "BasicAuthentication",
                    Description = "JWT and API Key authentication",
                    IsEnabled = true,
                    Category = FeatureCategory.Security,
                    MinimumTier = LicenseTier.Community,
                    AddedAt = DateTime.UtcNow
                });
            }

            if (selectedFeatures.Contains("BasicRateLimiting"))
            {
                features.Add(new LicenseFeature
                {
                    Name = "BasicRateLimiting",
                    Description = "Simple rate limiting",
                    IsEnabled = true,
                    Category = FeatureCategory.Performance,
                    MinimumTier = LicenseTier.Community,
                    AddedAt = DateTime.UtcNow
                });
            }

            if (selectedFeatures.Contains("AdvancedRateLimiting") && tier >= LicenseTier.Professional)
            {
                features.Add(new LicenseFeature
                {
                    Name = "AdvancedRateLimiting",
                    Description = "Advanced rate limiting with custom policies",
                    IsEnabled = true,
                    Category = FeatureCategory.Performance,
                    MinimumTier = LicenseTier.Professional,
                    AddedAt = DateTime.UtcNow
                });
            }

            if (selectedFeatures.Contains("LoadBalancing") && tier >= LicenseTier.Professional)
            {
                features.Add(new LicenseFeature
                {
                    Name = "LoadBalancing",
                    Description = "Load balancing across multiple targets",
                    IsEnabled = true,
                    Category = FeatureCategory.Performance,
                    MinimumTier = LicenseTier.Professional,
                    AddedAt = DateTime.UtcNow
                });
            }

            if (selectedFeatures.Contains("Monitoring") && tier >= LicenseTier.Enterprise)
            {
                features.Add(new LicenseFeature
                {
                    Name = "Monitoring",
                    Description = "Advanced monitoring and analytics",
                    IsEnabled = true,
                    Category = FeatureCategory.Monitoring,
                    MinimumTier = LicenseTier.Enterprise,
                    AddedAt = DateTime.UtcNow
                });
            }

            if (selectedFeatures.Contains("EnterpriseSupport") && tier == LicenseTier.Enterprise)
            {
                features.Add(new LicenseFeature
                {
                    Name = "EnterpriseSupport",
                    Description = "24/7 enterprise support",
                    IsEnabled = true,
                    Category = FeatureCategory.Management,
                    MinimumTier = LicenseTier.Enterprise,
                    AddedAt = DateTime.UtcNow
                });
            }

            return features;
        }

        #region Key Management

        /// <summary>
        /// Display the key management page
        /// </summary>
        public IActionResult KeyManagement()
        {
            var model = new Models.KeyManagementViewModel
            {
                HasCurrentKeys = true, // LicenseGenerator creates keys by default
                CurrentPublicKey = _licenseGenerator.ExportPublicKey()
            };
            return View(model);
        }

        /// <summary>
        /// Generate a new key pair
        /// </summary>
        [HttpPost]
        public IActionResult GenerateNewKeyPair()
        {
            try
            {
                // Generate and save new key pair
                _licenseGenerator.GenerateAndSaveNewKeyPair();
                var publicKey = _licenseGenerator.ExportPublicKey();
                
                TempData["SuccessMessage"] = "New key pair generated and saved successfully!";
                return Json(new { success = true, publicKey = publicKey });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate new key pair");
                return Json(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Export the current public key
        /// </summary>
        [HttpPost]
        public IActionResult ExportPublicKey()
        {
            try
            {
                var publicKey = _licenseGenerator.ExportPublicKey();
                var fileName = $"public_key_{DateTime.Now:yyyyMMdd_HHmmss}.pem";
                
                return File(Encoding.UTF8.GetBytes(publicKey), "application/x-pem-file", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export public key");
                return Json(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Save the current private key
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SavePrivateKey([FromBody] SavePrivateKeyRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.FileName))
                {
                    return Json(new { success = false, error = "File name is required" });
                }

                // Ensure the file has .pem extension
                var fileName = request.FileName;
                if (!fileName.EndsWith(".pem", StringComparison.OrdinalIgnoreCase))
                {
                    fileName += ".pem";
                }

                var tempPath = Path.Combine(Path.GetTempPath(), fileName);
                await _licenseGenerator.SavePrivateKeyAsync(tempPath, request.Password);

                // Read the file and return it as download
                var privateKeyContent = await System.IO.File.ReadAllTextAsync(tempPath);
                
                // Clean up temp file
                System.IO.File.Delete(tempPath);

                return File(Encoding.UTF8.GetBytes(privateKeyContent), "application/x-pem-file", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save private key");
                return Json(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Load a private key from uploaded file
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadPrivateKey(IFormFile keyFile, string? password = null)
        {
            try
            {
                if (keyFile == null || keyFile.Length == 0)
                {
                    return Json(new { success = false, error = "Please select a private key file" });
                }

                if (!keyFile.FileName.EndsWith(".pem", StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new { success = false, error = "Only .pem files are supported" });
                }

                // Save uploaded file temporarily
                var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".pem");
                using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    await keyFile.CopyToAsync(stream);
                }

                // Load the private key
                await _licenseGenerator.LoadPrivateKeyAsync(tempPath, password);

                // Clean up temp file
                System.IO.File.Delete(tempPath);

                // Get the new public key
                var publicKey = _licenseGenerator.ExportPublicKey();

                TempData["SuccessMessage"] = "Private key loaded successfully!";
                return Json(new { success = true, publicKey = publicKey });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load private key");
                return Json(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Get the current public key as JSON
        /// </summary>
        [HttpGet]
        public IActionResult GetPublicKey()
        {
            try
            {
                var publicKey = _licenseGenerator.ExportPublicKey();
                return Json(new { success = true, publicKey = publicKey });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get public key");
                return Json(new { success = false, error = ex.Message });
            }
        }

        #endregion
    }

    /// <summary>
    /// Request model for license download
    /// </summary>
    public class DownloadLicenseRequest
    {
        public string LicenseJson { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for saving private key
    /// </summary>
    public class SavePrivateKeyRequest
    {
        public string FileName { get; set; } = string.Empty;
        public string? Password { get; set; }
    }
}
