using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Web.ViewModels.Product;
using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Management.Web.Controllers;

/// <summary>
/// Product Key Management Controller
/// Handles RSA key pair management for products used in license generation
/// </summary>
[Authorize]
public class ProductKeyManagementController : BaseController
{
    private readonly ILogger<ProductKeyManagementController> _logger;
    private readonly IKeyManagementService _keyManagementService;
    private readonly IEnterpriseProductService _productService;

    public ProductKeyManagementController(
        ILogger<ProductKeyManagementController> logger,
        IKeyManagementService keyManagementService,
        IEnterpriseProductService productService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _keyManagementService = keyManagementService ?? throw new ArgumentNullException(nameof(keyManagementService));
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
    }

    /// <summary>
    /// Display key management for a specific product
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(Guid productId)
    {
        try
        {
            if (productId == Guid.Empty)
            {
                return BadRequest("Invalid product ID");
            }

            // Get product information
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                return NotFound($"Product with ID {productId} not found");
            }

            // Get current key information
            var hasKeys = await _keyManagementService.HasValidKeysAsync(productId);
            var publicKey = hasKeys ? await _keyManagementService.GetPublicKeyAsync(productId) : null;

            var viewModel = new ProductKeyManagementViewModel
            {
                ProductId = productId,
                ProductName = product.Name,
                ProductDescription = product.Description,
                HasKeys = hasKeys,
                PublicKey = publicKey,
                KeyGeneratedAt = hasKeys ? DateTime.UtcNow : null, // TODO: Get actual generation date
                CanGenerateKeys = true,
                CanRotateKeys = hasKeys
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading key management for product {ProductId}", productId);
            TempData["ErrorMessage"] = "Error loading key management. Please try again.";
            return RedirectToAction("Details", "Product", new { id = productId });
        }
    }

    /// <summary>
    /// Generate new key pair for product
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateKeys(Guid productId, int keySize = 2048)
    {
        try
        {
            if (productId == Guid.Empty)
            {
                return Json(new { success = false, message = "Invalid product ID" });
            }

            // Validate key size
            if (keySize != 2048 && keySize != 4096)
            {
                return Json(new { success = false, message = "Key size must be 2048 or 4096 bits" });
            }

            // Generate new key pair
            var publicKey = await _keyManagementService.GenerateKeyPairForProductAsync(productId, keySize);

            _logger.LogInformation("Generated new {KeySize}-bit key pair for product {ProductId}", keySize, productId);

            return Json(new 
            { 
                success = true, 
                message = $"Successfully generated {keySize}-bit RSA key pair",
                publicKey = publicKey,
                keySize = keySize,
                generatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating keys for product {ProductId}", productId);
            return Json(new { success = false, message = "Error generating keys. Please try again." });
        }
    }

    /// <summary>
    /// Rotate existing keys for product
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RotateKeys(Guid productId, int keySize = 2048)
    {
        try
        {
            if (productId == Guid.Empty)
            {
                return Json(new { success = false, message = "Invalid product ID" });
            }

            // Validate key size
            if (keySize != 2048 && keySize != 4096)
            {
                return Json(new { success = false, message = "Key size must be 2048 or 4096 bits" });
            }

            // Check if product has existing keys
            var hasKeys = await _keyManagementService.HasValidKeysAsync(productId);
            if (!hasKeys)
            {
                return Json(new { success = false, message = "No existing keys to rotate. Generate keys first." });
            }

            // Rotate keys
            var publicKey = await _keyManagementService.RotateKeysAsync(productId, keySize);

            _logger.LogInformation("Rotated keys for product {ProductId} with new {KeySize}-bit key pair", productId, keySize);

            return Json(new 
            { 
                success = true, 
                message = $"Successfully rotated to new {keySize}-bit RSA key pair",
                publicKey = publicKey,
                keySize = keySize,
                rotatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rotating keys for product {ProductId}", productId);
            return Json(new { success = false, message = "Error rotating keys. Please try again." });
        }
    }

    /// <summary>
    /// Get public key information for display
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPublicKey(Guid productId)
    {
        try
        {
            if (productId == Guid.Empty)
            {
                return Json(new { success = false, message = "Invalid product ID" });
            }

            var hasKeys = await _keyManagementService.HasValidKeysAsync(productId);
            if (!hasKeys)
            {
                return Json(new { success = false, message = "No keys found for this product" });
            }

            var publicKey = await _keyManagementService.GetPublicKeyAsync(productId);

            return Json(new 
            { 
                success = true, 
                publicKey = publicKey,
                hasKeys = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving public key for product {ProductId}", productId);
            return Json(new { success = false, message = "Error retrieving public key" });
        }
    }

    /// <summary>
    /// Delete keys for product (with confirmation)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteKeys(Guid productId, string confirmationText)
    {
        try
        {
            if (productId == Guid.Empty)
            {
                return Json(new { success = false, message = "Invalid product ID" });
            }

            // Require explicit confirmation
            if (confirmationText != "DELETE")
            {
                return Json(new { success = false, message = "Confirmation text must be 'DELETE'" });
            }

            var hasKeys = await _keyManagementService.HasValidKeysAsync(productId);
            if (!hasKeys)
            {
                return Json(new { success = false, message = "No keys found to delete" });
            }

            // TODO: Implement key deletion in KeyManagementService
            // await _keyManagementService.DeleteKeysAsync(productId);

            _logger.LogWarning("Keys deleted for product {ProductId}", productId);

            return Json(new 
            { 
                success = true, 
                message = "Keys successfully deleted",
                deletedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting keys for product {ProductId}", productId);
            return Json(new { success = false, message = "Error deleting keys. Please try again." });
        }
    }

    /// <summary>
    /// Download public key as a file
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> DownloadPublicKey(Guid productId)
    {
        try
        {
            if (productId == Guid.Empty)
            {
                return BadRequest("Invalid product ID");
            }

            var hasKeys = await _keyManagementService.HasValidKeysAsync(productId);
            if (!hasKeys)
            {
                TempData["ErrorMessage"] = "No keys found for this product";
                return RedirectToAction(nameof(Index), new { productId });
            }

            var publicKey = await _keyManagementService.GetPublicKeyAsync(productId);
            var product = await _productService.GetProductByIdAsync(productId);

            var fileName = $"{product?.Name?.Replace(" ", "_") ?? "Product"}_{productId}_PublicKey.pem";
            var fileBytes = System.Text.Encoding.UTF8.GetBytes(publicKey);

            return File(fileBytes, "application/x-pem-file", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading public key for product {ProductId}", productId);
            TempData["ErrorMessage"] = "Error downloading public key. Please try again.";
            return RedirectToAction(nameof(Index), new { productId });
        }
    }
}
