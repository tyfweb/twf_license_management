using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Management.Core.Models.Audit;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Models.Enums;

namespace TechWayFit.Licensing.Management.Core.Models.License;

public class ProductLicense : BaseAuditModel
{
    /// <summary>
    /// Unique identifier for the product license
    /// </summary>
    public Guid LicenseId { 
        get => Id; 
        set => Id = value; 
    }
    /// <summary>
    /// License code used for validation and activation (user-friendly format)
    /// Format: XXXX-YYYY-ZZZZ-AAAA-BBBB
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

    public DateTime CreatedAt { 
        get => CreatedOn; 
        set => CreatedOn = value; 
    }
    /// <summary>
    /// Encryption method used for the license
    /// </summary>
    public string Encryption { get; set; } = "AES256";
    /// <summary>
    /// Signature algorithm used for the license
    /// </summary>
    public string Signature { get; set; } = "SHA256";
    /// <summary>
    /// Base64 encoded license key (technical/cryptographic identifier for internal use)
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
    /// Type of license (Product Key, Product License, Volumetric License)
    /// </summary>
    public LicenseType LicenseModel { get; set; } = LicenseType.ProductKey;

    /// <summary>
    /// Maximum number of users allowed for this license (for Volumetric licenses)
    /// </summary>
    public int? MaxAllowedUsers { get; set; }
    
    /// <summary>
    /// Device activations for this license
    /// </summary>
    public virtual ICollection<ProductActivation> DeviceActivations { get; set; } = new List<ProductActivation>();
    
    /// <summary>
    /// Number of currently active device activations
    /// </summary>
    public int ActiveActivationsCount => DeviceActivations?.Count(a => a.Status == ProductActivationStatus.Active) ?? 0;
    
    /// <summary>
    /// Whether this license can accept more device activations
    /// </summary>
    public bool CanAcceptMoreActivations => MaxAllowedUsers == null || 
                                          ActiveActivationsCount < MaxAllowedUsers.Value;

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
    public Dictionary<string, object> Metadata { get; set; } = new();
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
        Metadata ??= new Dictionary<string, object>();
        Metadata.TryAdd("ProductTier", LicenseConsumer?.ProductTier?.Name ?? "Not Found");
        var licenseModel = new TechWayFit.Licensing.Core.Models.License
        {
            LicenseId = LicenseId.ToString(),    
            ValidFrom = ValidFrom,
            ValidTo = ValidTo,
            CreatedAt = CreatedAt,
            Status = Status,
            RevokedAt = RevokedAt,
            RevocationReason = RevocationReason,
            Metadata = Metadata?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? string.Empty) ?? new Dictionary<string, string>(),
            FeaturesIncluded = [],
            CreatedBy = IssuedBy,
            IssuedAt = KeyGeneratedAt,
            Issuer = IssuedBy,
            ProductVersion = SemanticVersion.Parse(ValidProductVersionFrom).ToVersion(),
            MaxSupportedVersion = SemanticVersion.Parse(ValidProductVersionTo).ToVersion(),
            Tier = LicenseTier.Custom
        };
        if (LicenseConsumer != null)
        {
            licenseModel.ProductId = LicenseConsumer.Product.ProductId.ToString();
            licenseModel.ConsumerId = LicenseConsumer.Consumer.ConsumerId.ToString();
            licenseModel.LicensedTo = LicenseConsumer.Consumer.CompanyName;
            licenseModel.ContactEmail = LicenseConsumer.Consumer.PrimaryContact.Email;
            licenseModel.ContactPerson = LicenseConsumer.Consumer.PrimaryContact.Name;
            licenseModel.SecondaryContactEmail = LicenseConsumer.Consumer.SecondaryContact?.Email;
            licenseModel.SecondaryContactPerson = LicenseConsumer.Consumer.SecondaryContact?.Name;
            licenseModel.Version = LicenseConsumer.Product.Version;
            licenseModel.Tier = LicenseConsumer.ProductTier?.Name switch
            {
                "Enterprise" => LicenseTier.Enterprise,
                "Professional" => LicenseTier.Professional,
                "Community" => LicenseTier.Community,
                _ => LicenseTier.Custom
            };
            licenseModel.FeaturesIncluded = LicenseConsumer.Features.Select(f => new LicenseFeature
            {
                Id = f.FeatureId.ToString(),
                Name = f.Name,
                Description = f.Description,
                IsCurrentlyValid = f.IsEnabled
            }).ToList();
        }


        return licenseModel;
    }
}
