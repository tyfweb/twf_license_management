namespace TechWayFit.Licensing.Management.Core.Models.Common;

/// <summary>
/// Composition object for workflow information
/// </summary>
public class WorkflowInfo
{
    /// <summary>
    /// Current status in the workflow
    /// </summary>
    public EntityStatus Status { get; set; } = EntityStatus.Draft;

    /// <summary>
    /// User who submitted the record for approval
    /// </summary>
    public string? SubmittedBy { get; set; }

    /// <summary>
    /// Timestamp when the record was submitted for approval
    /// </summary>
    public DateTime? SubmittedOn { get; set; }

    /// <summary>
    /// User who reviewed/approved/rejected the record
    /// </summary>
    public string? ReviewedBy { get; set; }

    /// <summary>
    /// Timestamp when the record was reviewed
    /// </summary>
    public DateTime? ReviewedOn { get; set; }

    /// <summary>
    /// Comments from the reviewer
    /// </summary>
    public string? ReviewComments { get; set; }
}
