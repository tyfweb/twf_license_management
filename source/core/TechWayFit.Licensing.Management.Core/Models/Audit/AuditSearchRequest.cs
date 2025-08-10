using System;

namespace TechWayFit.Licensing.Management.Core.Models.Audit;

public class AuditSearchRequest
{
    /// <summary>
    /// Entity type to filter by
    /// </summary>
    public AuditEntityType? EntityType { get; set; }

    /// <summary>
    /// Entity ID to filter by
    /// </summary>
    public string? EntityId { get; set; }

    /// <summary>
    /// Action type to filter by
    /// </summary>
    public AuditActionType? ActionType { get; set; }

    /// <summary>
    /// Start date for filtering
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// End date for filtering
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Page number for pagination
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size for pagination
    /// </summary>
    public int PageSize { get; set; } = 20;
}
