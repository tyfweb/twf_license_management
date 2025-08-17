using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TechWayFit.Licensing.Management.Web.Models.Api;
using TechWayFit.Licensing.Management.Web.Models.Api.User;
using TechWayFit.Licensing.Management.Web.Controllers;

namespace TechWayFit.Licensing.Management.Web.Controllers.Api;

/// <summary>
/// API Controller for User Management
/// Provides REST API endpoints for user operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class UserApiController : BaseController
{
    private readonly ILogger<UserApiController> _logger;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserApiController(
        ILogger<UserApiController> logger,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
    }

    /// <summary>
    /// Get all users with filtering and pagination
    /// </summary>
    /// <param name="request">Filter parameters</param>
    /// <returns>Paginated list of users</returns>
    [HttpGet]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> GetUsers([FromQuery] GetUsersRequest request)
    {
        try
        {
            _logger.LogInformation("Getting users with filters: SearchTerm={SearchTerm}, IsActive={IsActive}", 
                request.SearchTerm, request.IsActive);

            var users = _userManager.Users.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                users = users.Where(u => u.UserName.Contains(request.SearchTerm) || 
                                        u.Email.Contains(request.SearchTerm));
            }

            if (request.CreatedAfter.HasValue)
            {
                users = users.Where(u => u.Id != null); // Basic date filtering would need custom implementation
            }

            var totalCount = users.Count();
            var pagedUsers = users.Skip((request.PageNumber - 1) * request.PageSize)
                                 .Take(request.PageSize)
                                 .ToList();

            var userResponses = new List<UserResponse>();
            foreach (var user in pagedUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userResponses.Add(MapToUserResponse(user, roles.ToList()));
            }

            var response = new GetUsersResponse
            {
                Users = userResponses,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };

            return Ok(JsonResponse.OK(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return StatusCode(500, JsonResponse.Error("Failed to retrieve users"));
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> GetUser(string id)
    {
        try
        {
            _logger.LogInformation("Getting user with ID: {UserId}", id);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(JsonResponse.Error($"User with ID {id} not found"));
            }

            var roles = await _userManager.GetRolesAsync(user);
            var response = MapToUserResponse(user, roles.ToList());

            return Ok(JsonResponse.OK(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", id);
            return StatusCode(500, JsonResponse.Error("Failed to retrieve user"));
        }
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="request">User creation parameters</param>
    /// <returns>Created user</returns>
    [HttpPost]
    [ProducesResponseType(typeof(JsonResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(JsonResponse.Error("Invalid request data", 
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            _logger.LogInformation("Creating user: {UserName}", request.UserName);

            var user = new IdentityUser
            {
                UserName = request.UserName,
                Email = request.Email,
                EmailConfirmed = true // For API creation, auto-confirm
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(JsonResponse.Error("Failed to create user", 
                    result.Errors.Select(e => e.Description).ToList()));
            }

            // Assign roles if provided
            if (request.Roles != null && request.Roles.Any())
            {
                var roleResult = await _userManager.AddToRolesAsync(user, request.Roles);
                if (!roleResult.Succeeded)
                {
                    _logger.LogWarning("Failed to assign roles to user {UserId}: {Errors}", 
                        user.Id, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }

            var roles = await _userManager.GetRolesAsync(user);
            var response = MapToUserResponse(user, roles.ToList());

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, 
                JsonResponse.OK(response, "User created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, JsonResponse.Error("Failed to create user"));
        }
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">User update parameters</param>
    /// <returns>Updated user</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> UpdateUser(string id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(JsonResponse.Error("Invalid request data"));
            }

            _logger.LogInformation("Updating user {UserId}", id);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(JsonResponse.Error($"User with ID {id} not found"));
            }

            // Update only provided fields
            bool userModified = false;
            
            if (!string.IsNullOrEmpty(request.UserName) && request.UserName != user.UserName)
            {
                user.UserName = request.UserName;
                userModified = true;
            }
            
            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
            {
                user.Email = request.Email;
                userModified = true;
            }

            if (userModified)
            {
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    return BadRequest(JsonResponse.Error("Failed to update user", 
                        updateResult.Errors.Select(e => e.Description).ToList()));
                }
            }

            // Update roles if provided
            if (request.Roles != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                
                if (removeResult.Succeeded && request.Roles.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(user, request.Roles);
                    if (!addResult.Succeeded)
                    {
                        _logger.LogWarning("Failed to update roles for user {UserId}: {Errors}", 
                            user.Id, string.Join(", ", addResult.Errors.Select(e => e.Description)));
                    }
                }
            }

            var roles = await _userManager.GetRolesAsync(user);
            var response = MapToUserResponse(user, roles.ToList());

            return Ok(JsonResponse.OK(response, "User updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, JsonResponse.Error("Failed to update user"));
        }
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> DeleteUser(string id)
    {
        try
        {
            _logger.LogInformation("Deleting user {UserId}", id);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(JsonResponse.Error($"User with ID {id} not found"));
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(JsonResponse.Error("Failed to delete user", 
                    result.Errors.Select(e => e.Description).ToList()));
            }

            return Ok(JsonResponse.OK(null, "User deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, JsonResponse.Error("Failed to delete user"));
        }
    }

    /// <summary>
    /// Change user password
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">Password change parameters</param>
    /// <returns>Success status</returns>
    [HttpPost("{id}/change-password")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> ChangePassword(string id, [FromBody] ChangePasswordRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(JsonResponse.Error("Invalid request data"));
            }

            _logger.LogInformation("Changing password for user {UserId}", id);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(JsonResponse.Error($"User with ID {id} not found"));
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(JsonResponse.Error("Failed to change password", 
                    result.Errors.Select(e => e.Description).ToList()));
            }

            return Ok(JsonResponse.OK(null, "Password changed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", id);
            return StatusCode(500, JsonResponse.Error("Failed to change password"));
        }
    }

    private UserResponse MapToUserResponse(IdentityUser user, List<string> roles)
    {
        return new UserResponse
        {
            Id = Guid.Parse(user.Id),
            UserName = user.UserName ?? "",
            Email = user.Email ?? "",
            FirstName = "", // Would need to extend IdentityUser or get from additional table
            LastName = "", // Would need to extend IdentityUser or get from additional table
            IsActive = !user.LockoutEnabled || (user.LockoutEnd == null || user.LockoutEnd < DateTimeOffset.UtcNow),
            CreatedDate = DateTime.UtcNow, // Would need to extend IdentityUser
            Roles = roles,
            Metadata = new Dictionary<string, string>()
        };
    }
}
