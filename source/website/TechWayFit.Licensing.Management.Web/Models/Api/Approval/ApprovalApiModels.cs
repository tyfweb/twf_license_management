using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Web.Models.Api.Approval;

/// <summary>
/// API request for approving an entity
/// </summary>
public class ApproveEntityRequest
{
    [Required]
    public Guid EntityId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string EntityType { get; set; } = string.Empty; // "Consumer", "Product", "License"
    
    [StringLength(500)]
    public string? Comments { get; set; }
}

/// <summary>
/// API request for rejecting an entity
/// </summary>
public class RejectEntityRequest
{
    [Required]
    public Guid EntityId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string EntityType { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// API request for withdrawing an entity
/// </summary>
public class WithdrawEntityRequest
{
    [Required]
    public Guid EntityId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string EntityType { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Reason { get; set; }
}

/// <summary>
/// API request for submitting an entity for approval
/// </summary>
public class SubmitForApprovalRequest
{
    [Required]
    public Guid EntityId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string EntityType { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Comments { get; set; }
}

/// <summary>
/// API response for workflow operations
/// </summary>
public class WorkflowActionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public EntityStatus NewStatus { get; set; }
    public DateTime ActionTimestamp { get; set; }
    public string? EntityType { get; set; }
    public string? EntityDisplayName { get; set; }
}

/// <summary>
/// API response for workflow statistics
/// </summary>
public class WorkflowStatisticsResponse
{
    public int TotalPending { get; set; }
    public int TotalApprovedToday { get; set; }
    public int TotalRejectedToday { get; set; }
    public int TotalWithdrawnToday { get; set; }
    public int AverageApprovalTimeHours { get; set; }
    public int OldestPendingDays { get; set; }
    public Dictionary<string, int> PendingByType { get; set; } = new();
    public Dictionary<string, int> ApprovedByType { get; set; } = new();
}

/// <summary>
/// API response for pending approval items
/// </summary>
public class PendingApprovalItemResponse
{
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityDisplayName { get; set; } = string.Empty;
    public string EntityDescription { get; set; } = string.Empty;
    public EntityStatus EntityStatus { get; set; }
    public string SubmittedBy { get; set; } = string.Empty;
    public DateTime SubmittedOn { get; set; }
    public string? CurrentAssignee { get; set; }
    public int DaysInQueue { get; set; }
    public string Priority { get; set; } = "Normal";
    public bool CanApprove { get; set; }
    public bool CanReject { get; set; }
    public bool CanWithdraw { get; set; }
}

/// <summary>
/// API response for workflow history
/// </summary>
public class WorkflowHistoryResponse
{
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityDisplayName { get; set; } = string.Empty;
    public List<WorkflowActionHistoryItem> History { get; set; } = new();
}

/// <summary>
/// Individual workflow action history item
/// </summary>
public class WorkflowActionHistoryItem
{
    public Guid Id { get; set; }
    public EntityStatus FromStatus { get; set; }
    public EntityStatus ToStatus { get; set; }
    public string ActionBy { get; set; } = string.Empty;
    public DateTime ActionDate { get; set; }
    public string? Comments { get; set; }
    public string ActionDescription { get; set; } = string.Empty;
}

/// <summary>
/// API request for getting pending approvals with filters
/// </summary>
public class GetPendingApprovalsRequest
{
    public string? EntityTypeFilter { get; set; }
    public string? SubmittedByFilter { get; set; }
    public DateTime? SubmittedFromFilter { get; set; }
    public DateTime? SubmittedToFilter { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } // "SubmittedOn", "EntityType", "SubmittedBy", "DaysInQueue"
    public bool SortDescending { get; set; } = false;
}

/// <summary>
/// API response for pending approvals list
/// </summary>
public class GetPendingApprovalsResponse
{
    public List<PendingApprovalItemResponse> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}
