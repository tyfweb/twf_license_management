using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Common;

/// <summary>
/// Base entity for entities that require workflow capabilities - inherits audit fields and adds workflow fields
/// </summary>
public abstract class AuditWorkflowEntity : AuditEntity
{
    /// <summary>
    /// Current status in the workflow (Draft, PendingApproval, Approved, Rejected, Withdrawn, Archived)
    /// </summary>
    public int EntityStatus { get; set; } = 1; // Draft

    /// <summary>
    /// User who submitted the record for approval
    /// </summary>
    [MaxLength(100)]
    public string? SubmittedBy { get; set; }

    /// <summary>
    /// Timestamp when the record was submitted for approval
    /// </summary>
    public DateTime? SubmittedOn { get; set; }

    /// <summary>
    /// User who reviewed/approved/rejected the record
    /// </summary>
    [MaxLength(100)]
    public string? ReviewedBy { get; set; }

    /// <summary>
    /// Timestamp when the record was reviewed
    /// </summary>
    public DateTime? ReviewedOn { get; set; }

    /// <summary>
    /// Comments from the reviewer
    /// </summary>
    [MaxLength(1000)]
    public string? ReviewComments { get; set; }
}
