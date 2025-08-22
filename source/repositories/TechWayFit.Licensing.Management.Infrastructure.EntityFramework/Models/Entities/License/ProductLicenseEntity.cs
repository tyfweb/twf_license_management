using TechWayFit.Licensing.Management.Infrastructure.Data.Entities.Consumer;
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Core.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Products;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Common;
using System.Text.Json;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.License;

/// <summary>
/// Database entity for ProductLicense
/// </summary>
[Table("product_licenses")]
public class ProductLicenseEntity : AuditWorkflowEntity, IEntityMapper<ProductLicense, ProductLicenseEntity>
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
    /// Base64 encoded license key
    /// </summary>
    public string LicenseKey { get; set; } = string.Empty;

    /// <summary>
    /// Public key used for signing the license
    /// </summary>
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>
    /// Date when the license key was generated
    /// </summary>
    public DateTime KeyGeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Status of the license
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Type of license (Product Key, Product License, Volumetric License)
    /// </summary>
    public string LicenseModel { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of users allowed for this license (for Volumetric licenses)
    /// </summary>
    public int? MaxAllowedUsers { get; set; }

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

    #region LicenseTypeSpecificProperties
    /// <summary>
    /// Type of license - determines which specific properties are relevant
    /// </summary>
    public LicenseType LicenseType { get; set; } = LicenseType.ProductLicenseFile;
    #endregion
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
    /// Navigation property to License Activities - tracks all usage and changes
    /// </summary>
    public virtual ICollection<LicenseActivityEntity> Activities { get; set; } = new List<LicenseActivityEntity>();

    /// <summary>
    /// Navigation property to Product Activations (for ProductKey type)
    /// </summary>
    public virtual ICollection<ProductActivationEntity> Activations { get; set; } = new List<ProductActivationEntity>();

    /// <summary>
    /// Navigation property to License Files (for ProductLicenseFile type)
    /// </summary>
    public virtual ICollection<LicenseFileEntity> LicenseFiles { get; set; } = new List<LicenseFileEntity>();

    /// <summary>
    /// Navigation property to Volumetric License (for VolumetricLicense type)
    /// </summary>
    public virtual VolumetricLicenseEntity? VolumetricLicense { get; set; }

   
    #endregion

    #region IEntityMapper Implementation
    public ProductLicenseEntity Map(ProductLicense model)
    {
        if (model == null) return null!;

        Id = model.LicenseId;
        TenantId = model.TenantId;
        LicenseCode = model.LicenseCode;
        ProductId = model.ProductId;
        ConsumerId = model.ConsumerId;
        ProductTierId = model.ProductTierId;
        ValidFrom = model.ValidFrom;
        ValidTo = model.ValidTo;
        ValidProductVersionFrom = model.ValidProductVersionFrom;
        ValidProductVersionTo = model.ValidProductVersionTo;
        LicenseKey = model.LicenseKey;
        PublicKey = model.PublicKey;
        KeyGeneratedAt = model.KeyGeneratedAt;
        Status = model.Status.ToString();
        LicenseModel = model.LicenseModel.ToString();
        MaxAllowedUsers = model.MaxAllowedUsers;
        IssuedBy = model.IssuedBy;
        RevokedAt = model.RevokedAt;
        RevocationReason = model.RevocationReason;
        MetadataJson = model.Metadata != null ? JsonSerializer.Serialize(model.Metadata) : "{}";
        IsActive = model.IsActive;
        IsDeleted = model.IsDeleted;
        CreatedBy = model.CreatedBy;
        CreatedOn = model.CreatedOn;
        UpdatedBy = model.UpdatedBy;
        UpdatedOn = model.UpdatedOn;
        DeletedBy = model.DeletedBy;
        DeletedOn = model.DeletedOn;
        EntityStatus = (int)model.EntityStatus;
        SubmittedBy = model.SubmittedBy;
        SubmittedOn = model.SubmittedOn;
        ReviewedBy = model.ReviewedBy;
        ReviewedOn = model.ReviewedOn;
        ReviewComments = model.ReviewComments;
        RowVersion = model.RowVersion;

        return this;
    }

    public ProductLicense Map()
    { 

        return new ProductLicense
        {
            LicenseId = this.Id,
            TenantId = this.TenantId,
            LicenseCode = this.LicenseCode,
            ProductId = this.ProductId,
            ConsumerId = this.ConsumerId,
            ProductTierId = this.ProductTierId,
            ValidFrom = this.ValidFrom,
            ValidTo = this.ValidTo,
            ValidProductVersionFrom = this.ValidProductVersionFrom,
            ValidProductVersionTo = this.ValidProductVersionTo,
            LicenseKey = this.LicenseKey,
            PublicKey = this.PublicKey,
            KeyGeneratedAt = this.KeyGeneratedAt,
            Status = Enum.TryParse<LicenseStatus>(this.Status, out var status) ? status : LicenseStatus.Active,
            LicenseModel = Enum.TryParse<LicenseType>(this.LicenseModel, out var licenseType) ? licenseType : LicenseType.ProductKey,
            MaxAllowedUsers = this.MaxAllowedUsers,
            IssuedBy = this.IssuedBy,
            RevokedAt = this.RevokedAt,
            RevocationReason = this.RevocationReason,
            Metadata = !string.IsNullOrEmpty(this.MetadataJson)
                        ? JsonSerializer.Deserialize<Dictionary<string, string>>(this.MetadataJson)?.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) ?? new Dictionary<string, object>()
                        : new Dictionary<string, object>(),
            // Map the navigation properties to LicenseConsumer
            LicenseConsumer = new Core.Models.Consumer.ProductConsumer
            {
                TenantId = this.TenantId,
                Product = this.Product?.Map() ?? new Core.Models.Product.EnterpriseProduct(),
                Consumer = this.Consumer?.Map() ?? new Core.Models.Consumer.ConsumerAccount(),
                ProductTier = this.ProductTier?.Map() ?? new Core.Models.Product.ProductTier()
            },
            IsActive = this.IsActive,
            IsDeleted = this.IsDeleted,
            CreatedBy = this.CreatedBy,
            CreatedOn = this.CreatedOn,
            UpdatedBy = this.UpdatedBy,
            UpdatedOn = this.UpdatedOn,
            DeletedBy = this.DeletedBy,
            DeletedOn = this.DeletedOn,
            EntityStatus = (Core.Models.Common.EntityStatus)this.EntityStatus,
            SubmittedBy = this.SubmittedBy,
            SubmittedOn = this.SubmittedOn,
            ReviewedBy = this.ReviewedBy,
            ReviewedOn = this.ReviewedOn,
            ReviewComments = this.ReviewComments,
            RowVersion = this.RowVersion
        };
    }
    #endregion

}
