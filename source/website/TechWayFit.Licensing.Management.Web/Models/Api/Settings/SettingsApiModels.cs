using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Web.Models.Api.Settings;

public class GetSettingsRequest
{
    public string? Category { get; set; }
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class SettingResponse
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public string DataType { get; set; } = "string";
    public bool IsReadOnly { get; set; }
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}

public class GetSettingsResponse
{
    public List<SettingResponse> Settings { get; set; } = new();
    public Dictionary<string, List<SettingResponse>> SettingsByCategory { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class UpdateSettingRequest
{
    [Required]
    public string Value { get; set; } = string.Empty;
}

public class CreateSettingRequest
{
    [Required]
    [StringLength(100)]
    public string Key { get; set; } = string.Empty;
    
    [Required]
    public string Value { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? Category { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public string DataType { get; set; } = "string";
    
    public bool IsReadOnly { get; set; }
    
    public bool IsRequired { get; set; }
    
    public string? DefaultValue { get; set; }
}

public class SettingCategoryResponse
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SettingCount { get; set; }
}

public class SystemHealthResponse
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, HealthCheckResult> HealthChecks { get; set; } = new();
    public SystemMetrics Metrics { get; set; } = new();
    
    public class HealthCheckResult
    {
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TimeSpan Duration { get; set; }
        public Dictionary<string, object>? Data { get; set; }
    }
    
    public class SystemMetrics
    {
        public double CpuUsage { get; set; }
        public long MemoryUsage { get; set; }
        public long DiskUsage { get; set; }
        public int ActiveConnections { get; set; }
        public int RequestsPerSecond { get; set; }
    }
}
