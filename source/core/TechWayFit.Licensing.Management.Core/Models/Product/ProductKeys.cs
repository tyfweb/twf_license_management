using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Core.Models.Product;

/// <summary>
/// Product encryption keys model
/// </summary>
public class ProductKeys : BaseAuditModel
{
    /// <summary>
    /// Foreign key to Product
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// RSA Private Key in PEM format (encrypted for storage)
    /// </summary>
    public string PrivateKeyEncrypted { get; set; } = string.Empty;

    /// <summary>
    /// RSA Public Key in PEM format
    /// </summary>
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>
    /// Key size in bits (e.g., 2048, 4096)
    /// </summary>
    public int KeySize { get; set; } = 2048;

    /// <summary>
    /// Algorithm used for key generation
    /// </summary>
    public string Algorithm { get; set; } = "RSA";

    /// <summary>
    /// Date when the key was generated
    /// </summary>
    public DateTime KeyGeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional expiry date for key rotation
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Version/identifier for key rotation
    /// </summary>
    public int KeyVersion { get; set; } = 1;

    /// <summary>
    /// Additional metadata for the keys (JSON)
    /// </summary>
    public string MetadataJson { get; set; } = "{}";

    /// <summary>
    /// Navigation property to Product
    /// </summary>
    public EnterpriseProduct? Product { get; set; }
}
