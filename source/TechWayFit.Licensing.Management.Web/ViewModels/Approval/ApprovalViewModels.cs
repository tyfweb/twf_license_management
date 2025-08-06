using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Approval;

/// <summary>
/// Dashboard view model for pending approvals
/// </summary>
public class ApprovalDashboardViewModel
{
    public List<PendingApprovalItemViewModel> PendingItems { get; set; } = new();
    public ApprovalStatisticsViewModel Statistics { get; set; } = new();
    public List<WorkflowActionViewModel> RecentActions { get; set; } = new();
    public PaginationViewModel Pagination { get; set; } = new();
    
    // Filter properties
    public string? EntityTypeFilter { get; set; }
    public string? SubmittedByFilter { get; set; }
    public DateTime? SubmittedFromFilter { get; set; }
    public DateTime? SubmittedToFilter { get; set; }
}

/// <summary>
/// Individual pending approval item
/// </summary>
public class PendingApprovalItemViewModel
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
    
    // Quick action buttons
    public string ApprovalUrl { get; set; } = string.Empty;
    public string RejectUrl { get; set; } = string.Empty;
    public string DetailsUrl { get; set; } = string.Empty;
}

/// <summary>
/// Approval action view model
/// </summary>
public class ApprovalActionViewModel
{
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    
    [Required]
    public string Action { get; set; } = string.Empty; // "approve", "reject", "withdraw"
    
    [StringLength(500)]
    [Display(Name = "Comments")]
    public string? Comments { get; set; }
    
    [Required]
    public string ActionBy { get; set; } = string.Empty;
}

/// <summary>
/// Workflow action history view model
/// </summary>
public class WorkflowActionViewModel
{
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityDisplayName { get; set; } = string.Empty;
    public EntityStatus FromStatus { get; set; }
    public EntityStatus ToStatus { get; set; }
    public string ActionBy { get; set; } = string.Empty;
    public DateTime ActionDate { get; set; }
    public string? Comments { get; set; }
    public string ActionDescription { get; set; } = string.Empty;
}

/// <summary>
/// Approval statistics for dashboard
/// </summary>
public class ApprovalStatisticsViewModel
{
    public int TotalPending { get; set; }
    public int TotalApprovedToday { get; set; }
    public int TotalRejectedToday { get; set; }
    public int TotalWithdrawnToday { get; set; }
    public int AverageApprovalTime { get; set; } // in hours
    public int OldestPendingDays { get; set; }
    
    // By entity type
    public Dictionary<string, int> PendingByType { get; set; } = new();
    public Dictionary<string, int> ApprovedByType { get; set; } = new();
    
    // Performance metrics
    public int ItemsApprovedThisWeek { get; set; }
    public int ItemsRejectedThisWeek { get; set; }
    public double ApprovalRate { get; set; }
}

/// <summary>
/// View model for user's own submissions
/// </summary>
public class UserSubmissionsViewModel
{
    public List<UserSubmissionItemViewModel> Submissions { get; set; } = new();
    public PaginationViewModel Pagination { get; set; } = new();
    public EntityStatus? StatusFilter { get; set; }
    public string? EntityTypeFilter { get; set; }
}

/// <summary>
/// Individual user submission item
/// </summary>
public class UserSubmissionItemViewModel
{
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityDisplayName { get; set; } = string.Empty;
    public EntityStatus EntityStatus { get; set; }
    public DateTime SubmittedOn { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedOn { get; set; }
    public string? ReviewComments { get; set; }
    public bool CanWithdraw { get; set; }
    public bool CanEdit { get; set; }
    public string EditUrl { get; set; } = string.Empty;
    public string DetailsUrl { get; set; } = string.Empty;
}

/// <summary>
/// Pagination view model
/// </summary>
public class PaginationViewModel
{
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
}
