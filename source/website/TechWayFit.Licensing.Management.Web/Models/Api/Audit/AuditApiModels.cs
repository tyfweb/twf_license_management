using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Web.Models.Api.Audit;

public class GetAuditLogsRequest
{
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? Action { get; set; }
    public string? UserId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class AuditLogResponse
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public Dictionary<string, object>? OldValues { get; set; }
    public Dictionary<string, object>? NewValues { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

public class GetAuditLogsResponse
{
    public List<AuditLogResponse> AuditLogs { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class CreateAuditLogRequest
{
    [Required]
    public string EntityType { get; set; } = string.Empty;
    
    [Required]
    public Guid EntityId { get; set; }
    
    [Required]
    public string Action { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public Dictionary<string, object>? OldValues { get; set; }
    
    public Dictionary<string, object>? NewValues { get; set; }
    
    public Dictionary<string, string>? Metadata { get; set; }
}

public class AuditStatsResponse
{
    public int TotalLogs { get; set; }
    public int LogsToday { get; set; }
    public int LogsThisWeek { get; set; }
    public int LogsThisMonth { get; set; }
    public Dictionary<string, int> ActionCounts { get; set; } = new();
    public Dictionary<string, int> EntityTypeCounts { get; set; } = new();
    public List<TopUserActivity> TopUsers { get; set; } = new();
    
    public class TopUserActivity
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int ActivityCount { get; set; }
    }
}

public class EntityAuditTrailResponse
{
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public List<AuditLogResponse> AuditLogs { get; set; } = new();
    public DateTime FirstActivity { get; set; }
    public DateTime LastActivity { get; set; }
    public int TotalChanges { get; set; }
}

public class ExportAuditLogsRequest
{
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? Action { get; set; }
    public string? UserId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Format { get; set; } = "csv"; // csv, json, excel
}
