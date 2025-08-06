using TechWayFit.Licensing.Management.Infrastructure.Data.Entities.Consumer;
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Models.Consumer;
using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;
using TechWayFit.Licensing.Core.Models;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.License;

/// <summary>
/// Database entity for ProductLicense
/// </summary>
[Table("product_licenses")]
public class ProductLicenseEntity : BaseDbEntity
{
    #region BaseEntityProperties 
    /// <summary>
    /// License code used for validation and activation
    /// </summary>
    public string LicenseCode { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to Product
    /// </summary>
    public Guid ProductId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key to Consumer
    /// </summary>
    public Guid ConsumerId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key to Product Tier (optional - for tier-based licensing)
    /// </summary>
    public Guid? ProductTierId { get; set; }
 
    /// <summary>
    /// Date when the license becomes valid
    /// </summary>
    public DateTime ValidFrom { get; set; }

    /// <summary>
    /// Date when the license expires
    /// </summary>
    public DateTime ValidTo { get; set; }

    /// <summary>
    /// Minimum product version that this license supports
    /// </summary>
    public string ValidProductVersionFrom { get; set; } = "1.0.0";

    /// <summary>
    /// Maximum product version that this license supports (optional)
    /// </summary>
    public string? ValidProductVersionTo { get; set; }

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
    public string Status { get; set; } = string.Empty;

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
    /// Additional metadata for the license (JSON)
    /// </summary>
    public string MetadataJson { get; set; } = "{}";
    #endregion

    #region NavigationProperties
    /// <summary>
    /// Navigation property to Product
    /// </summary>
    public virtual ProductEntity? Product { get; set; }

    /// <summary>
    /// Navigation property to Consumer
    /// </summary>
    public virtual ConsumerAccountEntity? Consumer { get; set; }

    /// <summary>
    /// Navigation property to Product Tier
    /// </summary>
    public virtual ProductTierEntity? ProductTier { get; set; }

    /// <summary>
    /// Navigation property to Features (kept for backward compatibility but tier-based features are preferred)
    /// </summary>
    public virtual ICollection<ProductFeatureEntity> Features { get; set; } = new List<ProductFeatureEntity>();

    #endregion
}
