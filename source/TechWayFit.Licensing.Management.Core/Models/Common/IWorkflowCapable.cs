namespace TechWayFit.Licensing.Management.Core.Models.Common;

/// <summary>
/// Interface for models that can participate in workflow operations
/// </summary>
public interface IWorkflowCapable
{
    /// <summary>
    /// Unique identifier for the model
    /// </summary>
    Guid Id { get; set; }

    /// <summary>
    /// Audit information
    /// </summary>
    AuditInfo Audit { get; set; }

    /// <summary>
    /// Workflow information (if applicable)
    /// </summary>
    WorkflowInfo Workflow { get; set; }
}
