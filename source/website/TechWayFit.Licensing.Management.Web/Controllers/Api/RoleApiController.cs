using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Management.Web.Models.Api;
using TechWayFit.Licensing.Management.Web.Models.Api.Role;
using TechWayFit.Licensing.Management.Web.Controllers;

namespace TechWayFit.Licensing.Management.Web.Controllers.Api;

/// <summary>
/// API Controller for Role Management
/// Provides REST API endpoints for role and permission operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class RoleApiController : BaseController
{
    private readonly ILogger<RoleApiController> _logger;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;

    public RoleApiController(
        ILogger<RoleApiController> logger,
        RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    /// <summary>
    /// Get all roles with filtering and pagination
    /// </summary>
    /// <param name="request">Filter parameters</param>
    /// <returns>Paginated list of roles</returns>
    [HttpGet]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> GetRoles([FromQuery] GetRolesRequest request)
    {
        try
        {
            _logger.LogInformation("Getting roles with filters: SearchTerm={SearchTerm}, IsActive={IsActive}", 
                request.SearchTerm, request.IsActive);

            var roles = _roleManager.Roles.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                roles = roles.Where(r => r.Name.Contains(request.SearchTerm));
            }

            var totalCount = roles.Count();
            var pagedRoles = roles.Skip((request.PageNumber - 1) * request.PageSize)
                                 .Take(request.PageSize)
                                 .ToList();

            var roleResponses = new List<RoleResponse>();
            foreach (var role in pagedRoles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                roleResponses.Add(MapToRoleResponse(role, usersInRole.Count));
            }

            var response = new GetRolesResponse
            {
                Roles = roleResponses,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };

            return Ok(JsonResponse.OK(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles");
            return StatusCode(500, JsonResponse.Error("Failed to retrieve roles"));
        }
    }

    /// <summary>
    /// Get role by ID
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <returns>Role details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> GetRole(string id)
    {
        try
        {
            _logger.LogInformation("Getting role with ID: {RoleId}", id);

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound(JsonResponse.Error($"Role with ID {id} not found"));
            }

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            var response = MapToRoleResponse(role, usersInRole.Count);

            return Ok(JsonResponse.OK(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role {RoleId}", id);
            return StatusCode(500, JsonResponse.Error("Failed to retrieve role"));
        }
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    /// <param name="request">Role creation parameters</param>
    /// <returns>Created role</returns>
    [HttpPost]
    [ProducesResponseType(typeof(JsonResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> CreateRole([FromBody] CreateRoleRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(JsonResponse.Error("Invalid request data", 
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            _logger.LogInformation("Creating role: {RoleName}", request.Name);

            var role = new IdentityRole
            {
                Name = request.Name,
                NormalizedName = request.Name.ToUpperInvariant()
            };

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(JsonResponse.Error("Failed to create role", 
                    result.Errors.Select(e => e.Description).ToList()));
            }

            var response = MapToRoleResponse(role, 0);

            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, 
                JsonResponse.OK(response, "Role created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return StatusCode(500, JsonResponse.Error("Failed to create role"));
        }
    }

    /// <summary>
    /// Update an existing role
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <param name="request">Role update parameters</param>
    /// <returns>Updated role</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> UpdateRole(string id, [FromBody] UpdateRoleRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(JsonResponse.Error("Invalid request data"));
            }

            _logger.LogInformation("Updating role {RoleId}", id);

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound(JsonResponse.Error($"Role with ID {id} not found"));
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(request.Name) && request.Name != role.Name)
            {
                role.Name = request.Name;
                role.NormalizedName = request.Name.ToUpperInvariant();
            }

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(JsonResponse.Error("Failed to update role", 
                    result.Errors.Select(e => e.Description).ToList()));
            }

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            var response = MapToRoleResponse(role, usersInRole.Count);

            return Ok(JsonResponse.OK(response, "Role updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role {RoleId}", id);
            return StatusCode(500, JsonResponse.Error("Failed to update role"));
        }
    }

    /// <summary>
    /// Delete a role
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> DeleteRole(string id)
    {
        try
        {
            _logger.LogInformation("Deleting role {RoleId}", id);

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound(JsonResponse.Error($"Role with ID {id} not found"));
            }

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(JsonResponse.Error("Failed to delete role", 
                    result.Errors.Select(e => e.Description).ToList()));
            }

            return Ok(JsonResponse.OK(null, "Role deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role {RoleId}", id);
            return StatusCode(500, JsonResponse.Error("Failed to delete role"));
        }
    }

    /// <summary>
    /// Get all available permissions
    /// </summary>
    /// <returns>List of permissions</returns>
    [HttpGet("permissions")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> GetPermissions()
    {
        try
        {
            _logger.LogInformation("Getting all permissions");

            // Placeholder implementation - replace with actual permissions from your system
            var permissions = new List<PermissionResponse>
            {
                new PermissionResponse { Name = "license.create", Description = "Create licenses", Category = "License Management" },
                new PermissionResponse { Name = "license.read", Description = "View licenses", Category = "License Management" },
                new PermissionResponse { Name = "license.update", Description = "Update licenses", Category = "License Management" },
                new PermissionResponse { Name = "license.delete", Description = "Delete licenses", Category = "License Management" },
                new PermissionResponse { Name = "product.create", Description = "Create products", Category = "Product Management" },
                new PermissionResponse { Name = "product.read", Description = "View products", Category = "Product Management" },
                new PermissionResponse { Name = "product.update", Description = "Update products", Category = "Product Management" },
                new PermissionResponse { Name = "product.delete", Description = "Delete products", Category = "Product Management" },
                new PermissionResponse { Name = "consumer.create", Description = "Create consumers", Category = "Consumer Management" },
                new PermissionResponse { Name = "consumer.read", Description = "View consumers", Category = "Consumer Management" },
                new PermissionResponse { Name = "consumer.update", Description = "Update consumers", Category = "Consumer Management" },
                new PermissionResponse { Name = "consumer.delete", Description = "Delete consumers", Category = "Consumer Management" },
                new PermissionResponse { Name = "user.create", Description = "Create users", Category = "User Management" },
                new PermissionResponse { Name = "user.read", Description = "View users", Category = "User Management" },
                new PermissionResponse { Name = "user.update", Description = "Update users", Category = "User Management" },
                new PermissionResponse { Name = "user.delete", Description = "Delete users", Category = "User Management" },
                new PermissionResponse { Name = "audit.read", Description = "View audit logs", Category = "Audit Management" },
                new PermissionResponse { Name = "settings.read", Description = "View settings", Category = "System Management" },
                new PermissionResponse { Name = "settings.update", Description = "Update settings", Category = "System Management" }
            };

            var response = new GetPermissionsResponse
            {
                Permissions = permissions,
                PermissionsByCategory = permissions.GroupBy(p => p.Category)
                    .ToDictionary(g => g.Key, g => g.ToList())
            };

            return Ok(JsonResponse.OK(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions");
            return StatusCode(500, JsonResponse.Error("Failed to retrieve permissions"));
        }
    }

    /// <summary>
    /// Assign roles to a user
    /// </summary>
    /// <param name="request">Role assignment parameters</param>
    /// <returns>Updated role assignments</returns>
    [HttpPost("assign")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> AssignRolesToUser([FromBody] AssignRoleToUserRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(JsonResponse.Error("Invalid request data"));
            }

            _logger.LogInformation("Assigning roles to user {UserId}", request.UserId);

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                return NotFound(JsonResponse.Error($"User with ID {request.UserId} not found"));
            }

            // Get role names from IDs
            var roleNames = new List<string>();
            foreach (var roleId in request.RoleIds)
            {
                var role = await _roleManager.FindByIdAsync(roleId.ToString());
                if (role != null)
                {
                    roleNames.Add(role.Name);
                }
            }

            // Remove existing roles and add new ones
            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            
            if (removeResult.Succeeded && roleNames.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, roleNames);
                if (!addResult.Succeeded)
                {
                    return BadRequest(JsonResponse.Error("Failed to assign roles", 
                        addResult.Errors.Select(e => e.Description).ToList()));
                }
            }

            // Get updated roles
            var updatedRoles = await _userManager.GetRolesAsync(user);
            var roles = new List<RoleResponse>();
            foreach (var roleName in updatedRoles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                    roles.Add(MapToRoleResponse(role, usersInRole.Count));
                }
            }

            var response = new RoleAssignmentResponse
            {
                UserId = request.UserId,
                UserName = user.UserName ?? "",
                Roles = roles
            };

            return Ok(JsonResponse.OK(response, "Roles assigned successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning roles to user");
            return StatusCode(500, JsonResponse.Error("Failed to assign roles"));
        }
    }

    /// <summary>
    /// Get user role assignments
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User role assignments</returns>
    [HttpGet("user/{userId}/assignments")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> GetUserRoleAssignments(Guid userId)
    {
        try
        {
            _logger.LogInformation("Getting role assignments for user {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound(JsonResponse.Error($"User with ID {userId} not found"));
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var roles = new List<RoleResponse>();
            
            foreach (var roleName in userRoles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                    roles.Add(MapToRoleResponse(role, usersInRole.Count));
                }
            }

            var response = new RoleAssignmentResponse
            {
                UserId = userId,
                UserName = user.UserName ?? "",
                Roles = roles
            };

            return Ok(JsonResponse.OK(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role assignments for user {UserId}", userId);
            return StatusCode(500, JsonResponse.Error("Failed to retrieve user role assignments"));
        }
    }

    private RoleResponse MapToRoleResponse(IdentityRole role, int userCount)
    {
        return new RoleResponse
        {
            Id = Guid.Parse(role.Id),
            Name = role.Name ?? "",
            Description = null, // Would need to extend IdentityRole to include description
            IsActive = true, // Would need to extend IdentityRole to include IsActive
            CreatedDate = DateTime.UtcNow, // Would need to extend IdentityRole
            UserCount = userCount,
            Permissions = new List<string>() // Would need to implement permission system
        };
    }
}
