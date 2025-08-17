using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Common;
using TechWayFit.Licensing.Management.Core.Models.Enums;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.License;

/// <summary>
/// Database entity for tracking product key activations
/// </summary>
[Table("product_activations")]
public class ProductActivationEntity : AuditWorkflowEntity
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
    /// Last heartbeat from the activated machine
    /// </summary>
    public DateTime? LastHeartbeat { get; set; }

    /// <summary>
    /// Current status of the activation
    /// </summary>
    public ProductActivationStatus Status { get; set; } = ProductActivationStatus.Active;

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
}
