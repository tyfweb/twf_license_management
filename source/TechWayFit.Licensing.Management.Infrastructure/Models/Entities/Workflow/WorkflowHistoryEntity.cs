using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Core.Contracts.Services.Workflow;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Workflow;

/// <summary>
/// Database entity for workflow history tracking
/// </summary>
[Table("workflow_history")]
public class WorkflowHistoryEntity : AuditEntity
{
    /// <summary>
    /// ID of the entity this history entry relates to
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// Type name of the entity (e.g., "ProductLicense", "ConsumerAccount")
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Status the entity was in before the action
    /// </summary>
    public int FromStatus { get; set; }

    /// <summary>
    /// Status the entity moved to after the action
    /// </summary>
    public int ToStatus { get; set; }

    /// <summary>
    /// User who performed the action
    /// </summary>
    public string ActionBy { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the action was performed
    /// </summary>
    public DateTime ActionDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Comments or notes about the action
    /// </summary>
    public string? Comments { get; set; }

    /// <summary>
    /// Additional metadata as JSON
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Convert entity to core model
    /// </summary>
    /// <returns>WorkflowHistoryEntry core model</returns>
    public WorkflowHistoryEntry ToModel()
    {
        return new WorkflowHistoryEntry
        {
            Id = Id,
            EntityId = EntityId,
            EntityType = EntityType,
            FromStatus = (EntityStatus)FromStatus,
            ToStatus = (EntityStatus)ToStatus,
            ActionBy = ActionBy,
            ActionDate = ActionDate,
            Comments = Comments,
            Metadata = Metadata
        };
    }

    /// <summary>
    /// Create entity from core model
    /// </summary>
    /// <param name="model">WorkflowHistoryEntry core model</param>
    /// <returns>WorkflowHistoryEntity</returns>
    public static WorkflowHistoryEntity FromModel(WorkflowHistoryEntry model)
    {
        return new WorkflowHistoryEntity
        {
            Id = model.Id,
            EntityId = model.EntityId,
            EntityType = model.EntityType,
            FromStatus = (int)model.FromStatus,
            ToStatus = (int)model.ToStatus,
            ActionBy = model.ActionBy,
            ActionDate = model.ActionDate,
            Comments = model.Comments,
            Metadata = model.Metadata,
            CreatedBy = model.ActionBy,
            CreatedOn = model.ActionDate,
            IsActive = true
        };
    }
}
