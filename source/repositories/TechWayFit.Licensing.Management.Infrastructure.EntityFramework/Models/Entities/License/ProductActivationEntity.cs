using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Common;
using TechWayFit.Licensing.Management.Core.Models.Enums;
using TechWayFit.Licensing.Management.Core.Models.License;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.License;

/// <summary>
/// Database entity for tracking product key activations
/// </summary>
[Table("product_activations")]
public class ProductActivationEntity : AuditWorkflowEntity, IEntityMapper<ProductActivation, ProductActivationEntity>
{
    #region LicenseConfiguration (Immutable)
    /// <summary>
    /// Foreign key to the license that this activation belongs to
    /// </summary>
    public Guid LicenseId { get; set; }

    /// <summary>
    /// Formatted product key in XXXX-XXXX-XXXX-XXXX format - immutable after creation
    /// </summary>
    [Required]
    [MaxLength(19)]
    public string FormattedProductKey { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of activations allowed - immutable after creation
    /// </summary>
    public int MaxActivations { get; set; }
    #endregion

    #region ActivationTracking (Mutable)
    /// <summary>
    /// The product key that was activated (XXXX-XXXX-XXXX-XXXX format)
    /// </summary>
    [Required]
    [MaxLength(19)]
    public string ProductKey { get; set; } = string.Empty;

    /// <summary>
    /// Unique identifier for the machine/device
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string MachineId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the machine/device
    /// </summary>
    [MaxLength(255)]
    public string? MachineName { get; set; }

    /// <summary>
    /// Hardware fingerprint of the machine
    /// </summary>
    [MaxLength(500)]
    public string? MachineFingerprint { get; set; }

    /// <summary>
    /// IP address from which the activation was made
    /// </summary>
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// Date when the activation was created
    /// </summary>
    public DateTime ActivationDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the activation expires (calculated from license validity)
    /// </summary>
    public DateTime? ActivationEndDate { get; set; }

    /// <summary>
    /// Unique activation signature/token for this activation
    /// </summary>
    [MaxLength(500)]
    public string? ActivationSignature { get; set; }

    /// <summary>
    /// Last heartbeat from the activated machine
    /// </summary>
    public DateTime? LastHeartbeat { get; set; }

    /// <summary>
    /// Current status of the activation
    /// </summary>
    public ProductActivationStatus Status { get; set; } = ProductActivationStatus.PendingActivation;

    /// <summary>
    /// Additional activation data in JSON format
    /// </summary>
    public string ActivationData { get; set; } = "{}";

    /// <summary>
    /// Date when the activation was deactivated
    /// </summary>
    public DateTime? DeactivationDate { get; set; }

    /// <summary>
    /// Reason for deactivation
    /// </summary>
    [MaxLength(500)]
    public string? DeactivationReason { get; set; }

    /// <summary>
    /// User who deactivated this activation
    /// </summary>
    [MaxLength(100)]
    public string? DeactivatedBy { get; set; }
    #endregion
    #region NavigationProperties
    /// <summary>
    /// Navigation property to the parent license
    /// </summary>
    public virtual ProductLicenseEntity? License { get; set; }
    #endregion

    #region IEntityMapper Implementation
    /// <summary>
    /// Converts ProductActivationEntity to ProductActivation core model
    /// </summary>
    public ProductActivation Map()
    {
        return new ProductActivation
        {
            Id = this.Id,
            TenantId = this.TenantId,
            LicenseId = this.LicenseId,
            FormattedProductKey = this.FormattedProductKey,
            MaxActivations = this.MaxActivations,
            ProductKey = this.ProductKey,
            MachineId = this.MachineId,
            MachineName = this.MachineName,
            MachineFingerprint = this.MachineFingerprint,
            IpAddress = this.IpAddress,
            ActivationDate = this.ActivationDate,
            ActivationEndDate = this.ActivationEndDate,
            ActivationSignature = this.ActivationSignature,
            LastHeartbeat = this.LastHeartbeat,
            Status = this.Status,
            ActivationData = this.ActivationData,
            DeactivationDate = this.DeactivationDate,
            DeactivationReason = this.DeactivationReason,
            DeactivatedBy = this.DeactivatedBy,
            IsActive = this.IsActive,
            IsDeleted = this.IsDeleted,
            CreatedBy = this.CreatedBy,
            CreatedOn = this.CreatedOn,
            UpdatedBy = this.UpdatedBy,
            UpdatedOn = this.UpdatedOn
        };
    }

    /// <summary>
    /// Maps ProductActivation core model to ProductActivationEntity
    /// </summary>
    public ProductActivationEntity Map(ProductActivation model)
    {
        if (model == null) return null!;

        this.Id = model.Id;
        this.TenantId = model.TenantId;
        this.LicenseId = model.LicenseId;
        this.FormattedProductKey = model.FormattedProductKey;
        this.MaxActivations = model.MaxActivations;
        this.ProductKey = model.ProductKey;
        this.MachineId = model.MachineId;
        this.MachineName = model.MachineName;
        this.MachineFingerprint = model.MachineFingerprint;
        this.IpAddress = model.IpAddress;
        this.ActivationDate = model.ActivationDate;
        this.ActivationEndDate = model.ActivationEndDate;
        this.ActivationSignature = model.ActivationSignature;
        this.LastHeartbeat = model.LastHeartbeat;
        this.Status = model.Status;
        this.ActivationData = model.ActivationData;
        this.DeactivationDate = model.DeactivationDate;
        this.DeactivationReason = model.DeactivationReason;
        this.DeactivatedBy = model.DeactivatedBy;
        this.IsActive = model.IsActive;
        this.IsDeleted = model.IsDeleted;
        this.CreatedBy = model.CreatedBy;
        this.CreatedOn = model.CreatedOn;
        this.UpdatedBy = model.UpdatedBy;
        this.UpdatedOn = model.UpdatedOn;

        return this;
    }
    #endregion
}
