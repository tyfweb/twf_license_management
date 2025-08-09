using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Audit
{
    /// <summary>
    /// ViewModel for WorkflowHistory operations
    /// </summary>
    public class WorkflowHistoryViewModel
    {
        public Guid HistoryId { get; set; }
        public Guid TenantId { get; set; }

        [Display(Name = "Entity Type")]
        public string EntityType { get; set; } = string.Empty;

        [Display(Name = "Entity ID")]
        public Guid EntityId { get; set; }

        [Display(Name = "Previous Status")]
        public EntityStatus PreviousStatus { get; set; }

        [Display(Name = "New Status")]
        public EntityStatus NewStatus { get; set; }

        [Display(Name = "Action By")]
        public string ActionBy { get; set; } = string.Empty;

        [Display(Name = "Action Date")]
        public DateTime ActionDate { get; set; }

        [Display(Name = "Comments")]
        public string? Comments { get; set; }

        [Display(Name = "Reason")]
        public string? Reason { get; set; }

        [Display(Name = "IP Address")]
        public string? IpAddress { get; set; }

        // Computed properties
        [Display(Name = "Previous Status Display")]
        public string PreviousStatusDisplay => PreviousStatus.ToString();

        [Display(Name = "New Status Display")]
        public string NewStatusDisplay => NewStatus.ToString();

        [Display(Name = "Entity Display Name")]
        public string EntityDisplayName => EntityType switch
        {
            "ConsumerAccount" => "Consumer Account",
            "EnterpriseProduct" => "Product",
            "ProductLicense" => "License",
            "UserProfile" => "User",
            _ => EntityType
        };

        [Display(Name = "Is Approval")]
        public bool IsApproval => NewStatus == EntityStatus.Approved;

        [Display(Name = "Is Rejection")]
        public bool IsRejection => NewStatus == EntityStatus.Rejected;

        [Display(Name = "Is Submission")]
        public bool IsSubmission => NewStatus == EntityStatus.PendingApproval;
    }
}
