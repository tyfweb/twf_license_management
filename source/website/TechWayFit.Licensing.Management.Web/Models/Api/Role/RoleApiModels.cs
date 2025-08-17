using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Web.Models.Api.Role;

public class CreateRoleRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public List<string>? Permissions { get; set; }
}

public class UpdateRoleRequest
{
    [StringLength(100)]
    public string? Name { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool? IsActive { get; set; }
    
    public List<string>? Permissions { get; set; }
}

public class RoleResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public int UserCount { get; set; }
    public List<string> Permissions { get; set; } = new();
}

public class GetRolesRequest
{
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetRolesResponse
{
    public List<RoleResponse> Roles { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class PermissionResponse
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
}

public class GetPermissionsResponse
{
    public List<PermissionResponse> Permissions { get; set; } = new();
    public Dictionary<string, List<PermissionResponse>> PermissionsByCategory { get; set; } = new();
}

public class AssignRoleToUserRequest
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public List<Guid> RoleIds { get; set; } = new();
}

public class RoleAssignmentResponse
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public List<RoleResponse> Roles { get; set; } = new();
}
