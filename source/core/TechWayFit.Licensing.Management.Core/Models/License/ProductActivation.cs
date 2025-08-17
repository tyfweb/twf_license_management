using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.Enums;

namespace TechWayFit.Licensing.Management.Core.Models.License;

/// <summary>
/// Core model representing a product key activation
/// </summary>
public class ProductActivation : BaseAuditModel
{
    #region LicenseConfiguration (Immutable)
    /// <summary>
    /// Foreign key to the license that this activation belongs to
    /// </summary>
    public Guid LicenseId { get; set; }

    /// <summary>
    /// Formatted product key in XXXX-XXXX-XXXX-XXXX format - immutable after creation
    /// </summary>
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
    public string ProductKey { get; set; } = string.Empty;

    /// <summary>
    /// Unique identifier for the machine/device
    /// </summary>
    public string MachineId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the machine/device
    /// </summary>
    public string? MachineName { get; set; }

    /// <summary>
    /// Hardware fingerprint of the machine
    /// </summary>
    public string? MachineFingerprint { get; set; }

    /// <summary>
    /// IP address from which the activation was made
    /// </summary>
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
    public string? DeactivationReason { get; set; }

    /// <summary>
    /// User who deactivated this activation
    /// </summary>
    public string? DeactivatedBy { get; set; }
    #endregion

    #region NavigationProperties
    /// <summary>
    /// Navigation property to the parent license
    /// </summary>
    public virtual ProductLicense? License { get; set; }
    #endregion
}
