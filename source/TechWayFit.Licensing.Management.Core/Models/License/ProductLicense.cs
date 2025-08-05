using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Management.Core.Models.Audit;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Management.Core.Models.License;

public class ProductLicense
{
    /// <summary>
    /// Unique identifier for the product license
    /// </summary>
    public Guid LicenseId { get; set; } = Guid.NewGuid();
    public string LicenseCode { get; set; } = string.Empty;
    /// <summary>
    /// Product and Consumer relationship
    /// This contains the product and consumer information for the license
    /// </summary>
    public ProductConsumer LicenseConsumer { get; set; } = new();


    /// <summary>
    /// Minimum product version that this license supports
    /// </summary>
    public string ValidProductVersionFrom { get; set; } = "1.0.0";

    /// <summary>
    /// Maximum product version that this license supports (optional)
    /// </summary>
    public string? ValidProductVersionTo { get; set; }


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


    public TechWayFit.Licensing.Core.Models.License ToLicenseModel()
    {
        Metadata ??= new Dictionary<string, string>();
        Metadata.TryAdd("ProductTier", LicenseConsumer?.ProductTier?.Name?? "Not Found");
        return new TechWayFit.Licensing.Core.Models.License
        {
            LicenseId = LicenseId.ToString(),
            ProductId = LicenseConsumer.Product.ProductId.ToString(),
            // Update this line to use the correct property from LicenseConsumer, e.g. LicenseConsumer.Consumer.ConsumerId if Consumer is a property of ProductConsumer
            ConsumerId = LicenseConsumer.Consumer.ConsumerId.ToString(),
            ValidFrom = ValidFrom,
            ValidTo = ValidTo,
            CreatedAt = CreatedAt,
            Status = Status,
            RevokedAt = RevokedAt,
            RevocationReason = RevocationReason,
            Metadata = Metadata,
            FeaturesIncluded = LicenseConsumer.Features.Select(f => new LicenseFeature
            {
                Id = f.FeatureId.ToString(),
                Name = f.Name,
                Description = f.Description,
                IsCurrentlyValid = f.IsEnabled
            }).ToList(),
            LicensedTo = LicenseConsumer.Consumer.CompanyName,
            ContactEmail = LicenseConsumer.Consumer.PrimaryContact.Email,
            ContactPerson = LicenseConsumer.Consumer.PrimaryContact.Name,
            CreatedBy = IssuedBy,
            IssuedAt = KeyGeneratedAt,
            Issuer = IssuedBy,
            SecondaryContactEmail = LicenseConsumer.Consumer.SecondaryContact?.Email,
            SecondaryContactPerson = LicenseConsumer.Consumer.SecondaryContact?.Name,
            Version = LicenseConsumer.Product.Version,
            ProductVersion = SemanticVersion.Parse(ValidProductVersionFrom).ToVersion(),
            MaxSupportedVersion = SemanticVersion.Parse(ValidProductVersionTo).ToVersion(),
            Tier = LicenseTier.Custom
        };
    }
}
