using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Audit
{
    /// <summary>
    /// ViewModel for AuditLog operations
    /// </summary>
    public class AuditLogViewModel
    {
        public Guid AuditId { get; set; }
        public Guid TenantId { get; set; }

        [Display(Name = "Entity Type")]
        public string EntityType { get; set; } = string.Empty;

        [Display(Name = "Entity ID")]
        public Guid EntityId { get; set; }

        [Display(Name = "Action")]
        public string Action { get; set; } = string.Empty;

        [Display(Name = "User ID")]
        public Guid? UserId { get; set; }

        [Display(Name = "User Name")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "Timestamp")]
        public DateTime Timestamp { get; set; }

        [Display(Name = "Old Values")]
        public string? OldValues { get; set; }

        [Display(Name = "New Values")]
        public string? NewValues { get; set; }

        [Display(Name = "IP Address")]
        public string? IpAddress { get; set; }

        [Display(Name = "User Agent")]
        public string? UserAgent { get; set; }

        [Display(Name = "Session ID")]
        public string? SessionId { get; set; }

        [Display(Name = "Duration (ms)")]
        public long? DurationMs { get; set; }

        [Display(Name = "Success")]
        public bool IsSuccess { get; set; } = true;

        [Display(Name = "Error Message")]
        public string? ErrorMessage { get; set; }

        [Display(Name = "Stack Trace")]
        public string? StackTrace { get; set; }

        // Computed properties
        [Display(Name = "Action Type")]
        public string ActionType => Action switch
        {
            "CREATE" => "Create",
            "UPDATE" => "Update",
            "DELETE" => "Delete",
            "READ" => "Read",
            "LOGIN" => "Login",
            "LOGOUT" => "Logout",
            _ => Action
        };

        [Display(Name = "Entity Display Name")]
        public string EntityDisplayName => EntityType switch
        {
            "ConsumerAccount" => "Consumer Account",
            "EnterpriseProduct" => "Product",
            "ProductLicense" => "License",
            "UserProfile" => "User",
            "Setting" => "Setting",
            _ => EntityType
        };

        [Display(Name = "Has Changes")]
        public bool HasChanges => !string.IsNullOrEmpty(OldValues) || !string.IsNullOrEmpty(NewValues);
    }
}
