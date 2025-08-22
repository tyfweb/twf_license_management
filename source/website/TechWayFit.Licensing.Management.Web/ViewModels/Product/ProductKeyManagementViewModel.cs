using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Product;

/// <summary>
/// View model for product key management interface
/// </summary>
public class ProductKeyManagementViewModel
{
    /// <summary>
    /// Product identifier
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Product name for display
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Product description for context
    /// </summary>
    public string ProductDescription { get; set; } = string.Empty;

    /// <summary>
    /// Whether the product has active keys
    /// </summary>
    public bool HasKeys { get; set; }

    /// <summary>
    /// Current public key (PEM format)
    /// </summary>
    public string? PublicKey { get; set; }

    /// <summary>
    /// When the current keys were generated
    /// </summary>
    public DateTime? KeyGeneratedAt { get; set; }

    /// <summary>
    /// Key size in bits (2048, 4096)
    /// </summary>
    public int KeySize { get; set; } = 2048;

    /// <summary>
    /// Whether new keys can be generated
    /// </summary>
    public bool CanGenerateKeys { get; set; } = true;

    /// <summary>
    /// Whether existing keys can be rotated
    /// </summary>
    public bool CanRotateKeys { get; set; }

    /// <summary>
    /// Key version for tracking rotations
    /// </summary>
    public int KeyVersion { get; set; } = 1;

    /// <summary>
    /// Status message for operations
    /// </summary>
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Success indicator for operations
    /// </summary>
    public bool IsOperationSuccess { get; set; }
}

/// <summary>
/// View model for key generation/rotation operations
/// </summary>
public class KeyOperationViewModel
{
    /// <summary>
    /// Product identifier
    /// </summary>
    [Required]
    public Guid ProductId { get; set; }

    /// <summary>
    /// Key size in bits
    /// </summary>
    [Required]
    [Range(2048, 4096, ErrorMessage = "Key size must be 2048 or 4096 bits")]
    public int KeySize { get; set; } = 2048;

    /// <summary>
    /// Operation type (generate, rotate)
    /// </summary>
    [Required]
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Confirmation text for destructive operations
    /// </summary>
    public string? ConfirmationText { get; set; }
}

/// <summary>
/// View model for displaying key information in product views
/// </summary>
public class ProductKeyInfoViewModel
{
    /// <summary>
    /// Product identifier
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Whether the product has active keys
    /// </summary>
    public bool HasKeys { get; set; }

    /// <summary>
    /// Key fingerprint for identification (SHA256 hash of public key)
    /// </summary>
    public string? KeyFingerprint { get; set; }

    /// <summary>
    /// Key generation date
    /// </summary>
    public DateTime? KeyGeneratedAt { get; set; }

    /// <summary>
    /// Key size in bits
    /// </summary>
    public int KeySize { get; set; }

    /// <summary>
    /// Key version number
    /// </summary>
    public int KeyVersion { get; set; }

    /// <summary>
    /// Quick status for display
    /// </summary>
    public string Status => HasKeys ? "Active" : "No Keys";

    /// <summary>
    /// CSS class for status display
    /// </summary>
    public string StatusCssClass => HasKeys ? "text-success" : "text-warning";
}
