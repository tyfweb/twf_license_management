using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Common;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Products;

/// <summary>
/// Database entity for Product encryption keys
/// </summary>
[Table("product_keys")]
public class ProductKeysEntity : AuditWorkflowEntity, IEntityMapper<ProductKeys, ProductKeysEntity>
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
    public virtual ProductEntity Product { get; set; } = null!;

    #region IEntityMapper Implementation
    public ProductKeysEntity Map(ProductKeys model)
    {
        if (model == null) return null!;

        return new ProductKeysEntity
        {
            Id = model.Id,
            ProductId = model.ProductId,
            PrivateKeyEncrypted = model.PrivateKeyEncrypted,
            PublicKey = model.PublicKey,
            KeySize = model.KeySize,
            Algorithm = model.Algorithm,
            IsActive = model.IsActive,
            KeyGeneratedAt = model.KeyGeneratedAt,
            ExpiresAt = model.ExpiresAt,
            KeyVersion = model.KeyVersion,
            MetadataJson = model.MetadataJson,
            TenantId = model.TenantId,
            CreatedBy = model.CreatedBy,
            CreatedOn = model.CreatedOn,
            UpdatedBy = model.UpdatedBy,
            UpdatedOn = model.UpdatedOn,
            IsDeleted = model.IsDeleted,
            DeletedBy = model.DeletedBy,
            DeletedOn = model.DeletedOn
        };
    }

    public ProductKeys Map()
    {
        return new ProductKeys
        {
            Id = Id,
            ProductId = ProductId,
            PrivateKeyEncrypted = PrivateKeyEncrypted,
            PublicKey = PublicKey,
            KeySize = KeySize,
            Algorithm = Algorithm,
            IsActive = IsActive,
            KeyGeneratedAt = KeyGeneratedAt,
            ExpiresAt = ExpiresAt,
            KeyVersion = KeyVersion,
            MetadataJson = MetadataJson,
            TenantId = TenantId,
            CreatedBy = CreatedBy,
            CreatedOn = CreatedOn,
            UpdatedBy = UpdatedBy,
            UpdatedOn = UpdatedOn,
            IsDeleted = IsDeleted,
            DeletedBy = DeletedBy,
            DeletedOn = DeletedOn
        };
    }
    #endregion
}
