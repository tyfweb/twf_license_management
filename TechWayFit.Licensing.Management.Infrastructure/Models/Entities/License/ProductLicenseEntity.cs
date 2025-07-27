using TechWayFit.Licensing.Infrastructure.Data.Entities.Consumer;
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Models.Consumer;
using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Infrastructure.Models.Entities.Products;

namespace TechWayFit.Licensing.Infrastructure.Models.Entities.License;

/// <summary>
/// Database entity for ProductLicense
/// </summary>
[Table("product_licenses")]
public class ProductLicenseEntity : BaseAuditEntity
{
    #region BaseEntityProperties
    /// <summary>
    /// Unique identifier for the product license
    /// </summary>
    public string LicenseId { get; set; } = string.Empty;
    /// <summary>
    /// License code used for validation and activation
    /// </summary>
    public string LicenseCode { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to Product
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to Consumer
    /// </summary>
    public string ConsumerId { get; set; } = string.Empty;
 
    /// <summary>
    /// Date when the license becomes valid
    /// </summary>
    public DateTime ValidFrom { get; set; }

    /// <summary>
    /// Date when the license expires
    /// </summary>
    public DateTime ValidTo { get; set; }

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
    /// Navigation property to Tier
    /// </summary>
    public virtual ICollection<ProductFeatureEntity> Features { get; set; } = new List<ProductFeatureEntity>();

    #endregion
    #region MappingMethods
    public static ProductLicenseEntity FromModel(ProductLicense model)
    {
        return new ProductLicenseEntity
        {
            LicenseId = model.LicenseId,
            LicenseCode = model.LicenseCode,
            ProductId = model.LicenseConsumer.Product.ProductId,
            ConsumerId = model.LicenseConsumer.Consumer.ConsumerId, 
            ValidFrom = model.ValidFrom,
            ValidTo = model.ValidTo,
            Encryption = model.Encryption,
            Signature = model.Signature,
            LicenseKey = model.LicenseKey,
            PublicKey = model.PublicKey,
            LicenseSignature = model.LicenseSignature,
            KeyGeneratedAt = model.KeyGeneratedAt,
            Status = ToStringEnum(model.Status),
            IssuedBy = model.IssuedBy,
            RevokedAt = model.RevokedAt,
            RevocationReason = model.RevocationReason,
            MetadataJson = ToJson(model.Metadata)
        };
    }
    public ProductLicense ToModel()
    {
        return new ProductLicense
        {
            LicenseId = LicenseId,
            LicenseConsumer = new ProductConsumer
            {
                Product = new EnterpriseProduct { ProductId = ProductId },
                Consumer = new ConsumerAccount { ConsumerId = ConsumerId }
            },
            ValidFrom = ValidFrom,
            LicenseCode = LicenseCode,
            ValidTo = ValidTo,
            Encryption = Encryption,
            Signature = Signature,
            LicenseKey = LicenseKey,
            PublicKey = PublicKey,
            LicenseSignature = LicenseSignature,
            KeyGeneratedAt = KeyGeneratedAt,
            Status = ToEnum<LicenseStatus>(Status),
            IssuedBy = IssuedBy,
            RevokedAt = RevokedAt,
            RevocationReason = RevocationReason,
            Metadata = FromJson<Dictionary<string, string>>(MetadataJson),
            Features = Features.Select(f => f.ToModel()).ToList()
        };
    }
    #endregion

}
