using System;
using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Core.Models.Workflow;

public class WorkflowHistoryEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public EntityStatus FromStatus { get; set; }
    public EntityStatus ToStatus { get; set; }
    public string ActionBy { get; set; } = string.Empty;
    public DateTime ActionDate { get; set; } = DateTime.UtcNow;
    public string? Comments { get; set; }
    public string? Metadata { get; set; }
}
