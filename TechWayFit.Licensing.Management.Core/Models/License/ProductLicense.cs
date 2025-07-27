using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Management.Core.Models.License;

public class ProductLicense
{
    /// <summary>
    /// Unique identifier for the product license
    /// </summary>
    public string LicenseId { get; set; } = Guid.NewGuid().ToString();
    public string LicenseCode { get; set; } = string.Empty;
    /// <summary>
    /// Product and Consumer relationship
    /// This contains the product and consumer information for the license
    /// </summary>
    public ProductConsumer LicenseConsumer { get; set; } = new();

    /// <summary>
    /// Licensed product details
    /// </summary>
    public EnterpriseProduct LicensedProduct { get; set; } = new EnterpriseProduct();
    /// <summary>
    /// Features included in this license
    /// </summary>
    public IEnumerable<ProductFeature> Features { get; set; } = new List<ProductFeature>();

    /// <summary>
    /// Date when the license becomes valid
    /// </summary>
    public DateTime ValidFrom { get; set; }

    /// <summary>
    /// Date when the license expires
    /// </summary>
    public DateTime ValidTo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Encryption method used for the license
    /// </summary>
    public string Encryption { get; set; } = "AES256";
    /// <summary>
    /// Signature algorithm used for the license
    /// </summary>
    public string Signature { get; set; } = "SHA256";
    /// <summary>
    /// Base64 encoded license key
    /// </summary>
    public string LicenseKey { get; set; } = string.Empty;
    /// <summary>
    /// Public key used for signing the license
    /// </summary>
    public string PublicKey { get; set; } = string.Empty;
    /// <summary>
    /// License Signature
    /// </summary>
    public string LicenseSignature { get; set; } = string.Empty;
    /// <summary>
    /// Date when the license key was generated
    /// </summary>
    public DateTime KeyGeneratedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Status of the license
    /// </summary>
    public LicenseStatus Status { get; set; } = LicenseStatus.Active;
    /// <summary>
    /// User who issued the license
    /// </summary>
    public string IssuedBy { get; set; } = string.Empty;
    /// <summary>
    /// Date when the license was revoked, if applicable
    /// </summary>
    public DateTime? RevokedAt { get; set; }
    /// <summary>
    /// Reason for revocation, if applicable
    /// </summary>
    public string? RevocationReason { get; set; }
    /// <summary>
    /// Additional metadata for the license
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
    /// <summary>
    /// Is the license valid?
    /// </summary>
    public bool IsValid => Status == LicenseStatus.Active &&
                      DateTime.UtcNow >= ValidFrom &&
                      DateTime.UtcNow <= ValidTo;
    /// <summary>
    /// Is the license currently expired?
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ValidTo;
    /// <summary>
    /// Remaining days until the license expires
    /// </summary>
    public int DaysUntilExpiry => Math.Max(0, (ValidTo - DateTime.UtcNow).Days);
}
public enum LicenseStatus
{
    Inactive,
    Active,
    Expired,
    Suspended,
    Revoked,
    Pending,
    Unknown
}
