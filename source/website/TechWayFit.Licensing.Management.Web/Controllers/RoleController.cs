using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.User;
using TechWayFit.Licensing.Management.Core.Models.Tenant;
using TechWayFit.Licensing.Management.Web.Controllers;
using TechWayFit.Licensing.Management.Web.ViewModels.Role;

namespace TechWayFit.Licensing.Management.Web.Controllers;

/// <summary>
/// Controller for role management operations
/// </summary>
[Authorize]
public class RoleController : BaseController
{
    private readonly IUserService _userService;
    private readonly ITenantService _tenantService;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<RoleController> _logger;

    public RoleController(
        IUserService userService,
        ITenantService tenantService,
        IPermissionService permissionService,
        ILogger<RoleController> logger)
    {
        _userService = userService;
        _tenantService = tenantService;
        _permissionService = permissionService;
        _logger = logger;
    }

    /// <summary>
    /// Display roles list
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            // Initialize default roles if none exist 
            
            var roles = await _userService.GetAllRolesAsync();
            
                var viewModel = new RoleListViewModel
                {
                    Roles = roles.Select(r => new RoleViewModel
                    {
                        RoleId = r.RoleId,
                        RoleName = r.RoleName,
                        RoleDescription = r.RoleDescription,
                        IsSystemRole = r.IsAdmin, // Using IsAdmin as system role indicator
                        IsActive = r.Audit.IsActive,
                        CreatedOn = r.Audit.CreatedOn,
                        TenantName = r.Audit?.TenantName ?? "N/A",
                        UsersCount = 0 // TODO: Implement user count for each role
                    }).ToList()
                };
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading roles");
            TempData["ErrorMessage"] = "Failed to load roles. Please try again.";
            return View(new RoleListViewModel());
        }
    }

    /// <summary>
    /// Display create role form
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create()
    {        try
        {
            var tenants = await _tenantService.GetAllTenantsAsync();
            
            var viewModel = new CreateRoleViewModel
            {
                AvailableTenants = tenants.Select(t => new SelectListItem 
                { 
                    Value = t.TenantId.ToString(), 
                    Text = $"{t.TenantName} ({t.TenantCode})" 
                }).ToList(),
                // Initialize permissions with default values (None for all modules)
                Permissions = Enum.GetValues<SystemModule>().ToDictionary(m => m, m => PermissionLevel.None)
            };

            return View(viewModel);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to load tenant information. Please try again.";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// Create new role
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Reload tenants for dropdown
            try
            {
                var tenants = await _tenantService.GetAllTenantsAsync();
                model.AvailableTenants = tenants.Select(t => new SelectListItem 
                { 
                    Value = t.TenantId.ToString(), 
                    Text = $"{t.TenantName} ({t.TenantCode})" 
                }).ToList();
            }
            catch
            {
                model.AvailableTenants = new List<SelectListItem>();
            }

            return View(model);
        }        try
        {
            UserRole newRole = new UserRole
            {
                RoleId = Guid.NewGuid(),
                RoleName = model.RoleName.Trim(),
                RoleDescription = model.RoleDescription?.Trim(),
                TenantId = model.TenantId,
                CreatedBy = CurrentUserName,
                CreatedOn = DateTime.UtcNow,
                IsActive = true
            };
            
            // Create the role using the user service
            var result = await _userService.CreateRoleAsync(newRole); 

            if (result.Success)
            {
                // Parse and save permissions
                var permissions = ParsePermissionsFromForm();
                if (permissions.Any(p => p.Value != PermissionLevel.None))
                {
                    try
                    {
                        await _permissionService.UpdateRolePermissionsAsync(newRole.RoleId, permissions);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error creating permissions for role: {RoleId}", newRole.RoleId);
                        // Role was created but permissions failed - could continue or rollback
                        TempData["WarningMessage"] = $"Role '{model.RoleName}' created but some permissions could not be set.";
                    }
                }

                TempData["SuccessMessage"] = $"Role '{model.RoleName}' created successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error creating role: {RoleName}", model.RoleName);
            ModelState.AddModelError("", "Failed to create role. Please try again.");
        }

        // Reload tenants for dropdown on error
        try
        {
            var tenants = await _tenantService.GetAllTenantsAsync();
            model.AvailableTenants = tenants.Select(t => new SelectListItem 
            { 
                Value = t.TenantId.ToString(), 
                Text = $"{t.TenantName} ({t.TenantCode})" 
            }).ToList();
        }
        catch
        {
            model.AvailableTenants = new List<SelectListItem>();
        }

        return View(model);
    }

    /// <summary>
    /// Display role details
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var roles = await _userService.GetAllRolesAsync();
            var role = roles.FirstOrDefault(r => r.RoleId == id);
            
            if (role == null)
            {
                TempData["ErrorMessage"] = "Role not found.";
                return RedirectToAction(nameof(Index));
            }

            // Get tenant information
            var tenants = await _tenantService.GetAllTenantsAsync();
            var tenant = tenants.FirstOrDefault(t => t.TenantId == role.TenantId);            // Get users assigned to this role
            var allUsers = await _userService.GetAllUsersAsync();
            var assignedUsers = new List<UserProfile>();
            
            // Get role mappings to find users with this role
            foreach (var user in allUsers)
            {
                var userRoles = await _userService.GetUserRolesAsync(user.UserId);
                if (userRoles.Any(r => r.RoleId == id))
                {
                    assignedUsers.Add(user);
                }
            }

            var viewModel = new RoleDetailsViewModel
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                RoleDescription = role.RoleDescription,
                TenantId = role.TenantId,
                TenantName = tenant?.TenantName ?? "Unknown",
                IsSystemRole = role.IsAdmin,
                IsActive = role.Audit.IsActive,
                CreatedOn = role.Audit.CreatedOn,
                CreatedBy = role.Audit.CreatedBy,
                UpdatedOn = role.Audit.UpdatedOn,
                UpdatedBy = role.Audit.UpdatedBy,                UsersCount = assignedUsers.Count,
                AssignedUsers = assignedUsers.Select(u => new UserSummary
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    FullName = u.FullName,
                    Email = u.Email,                    IsActive = u.Audit.IsActive,                    LastLoginDate = u.LastLoginDate ?? DateTime.MinValue
                }).ToList(),
                // Load actual permissions from database
                Permissions = await GetRolePermissionsFromDatabaseAsync(role.RoleId, role)
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading role details for ID: {RoleId}", id);
            TempData["ErrorMessage"] = "An error occurred while loading role details.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Display edit role form
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var roles = await _userService.GetAllRolesAsync();
            var role = roles.FirstOrDefault(r => r.RoleId == id);
            
            if (role == null)
            {
                TempData["ErrorMessage"] = "Role not found.";
                return RedirectToAction(nameof(Index));
            }

            var tenants = await _tenantService.GetAllTenantsAsync();
            var currentTenant = tenants.FirstOrDefault(t => t.TenantId == role.TenantId);

            var viewModel = new EditRoleViewModel
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                RoleDescription = role.RoleDescription,
                IsAdminRole = role.IsAdmin,
                TenantId = role.TenantId,
                TenantName = currentTenant?.TenantName,
                IsActive = role.Audit.IsActive,
                CreatedOn = role.Audit.CreatedOn,
                CreatedBy = role.Audit.CreatedBy,
                UpdatedOn = role.Audit.UpdatedOn,
                UpdatedBy = role.Audit.UpdatedBy,                AvailableTenants = tenants.Select(t => new SelectListItem 
                { 
                    Value = t.TenantId.ToString(), 
                    Text = $"{t.TenantName} ({t.TenantCode})",
                    Selected = t.TenantId == role.TenantId                }).ToList(),
                // Load actual permissions from database
                Permissions = await GetRolePermissionsFromDatabaseAsync(role.RoleId, role)
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading edit role form for ID: {RoleId}", id);
            TempData["ErrorMessage"] = "An error occurred while loading the role.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Handle edit role form submission
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditRoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Reload tenants for dropdown
            try
            {
                var tenants = await _tenantService.GetAllTenantsAsync();
                model.AvailableTenants = tenants.Select(t => new SelectListItem 
                { 
                    Value = t.TenantId.ToString(), 
                    Text = $"{t.TenantName} ({t.TenantCode})",
                    Selected = t.TenantId == model.TenantId
                }).ToList();
            }
            catch
            {
                model.AvailableTenants = new List<SelectListItem>();
            }

            return View(model);
        }

        try
        {
            // Get the existing role
            var roles = await _userService.GetAllRolesAsync();
            var existingRole = roles.FirstOrDefault(r => r.RoleId == model.RoleId);
            
            if (existingRole == null)
            {
                TempData["ErrorMessage"] = "Role not found.";
                return RedirectToAction(nameof(Index));
            }

            // Update the role properties
            existingRole.RoleName = model.RoleName.Trim();
            existingRole.RoleDescription = model.RoleDescription?.Trim();
            existingRole.IsAdmin = model.IsAdminRole;
            existingRole.TenantId = model.TenantId;
            existingRole.Audit.IsActive = model.IsActive;
            existingRole.Audit.UpdatedBy = CurrentUserName;
            existingRole.Audit.UpdatedOn = DateTime.UtcNow;            // Update the role using the user service
            var result = await _userService.UpdateRoleAsync(model.RoleId, existingRole);

            if (result.Success)
            {
                // Parse and save permissions
                var permissions = ParsePermissionsFromForm();
                try
                {
                    await _permissionService.UpdateRolePermissionsAsync(model.RoleId, permissions);
                    TempData["SuccessMessage"] = $"Role '{model.RoleName}' and permissions updated successfully.";
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error updating permissions for role: {RoleId}", model.RoleId);
                    TempData["WarningMessage"] = $"Role '{model.RoleName}' updated but some permissions could not be saved.";
                }
                
                return RedirectToAction(nameof(Details), new { id = model.RoleId });
            }
            else
            {
                ModelState.AddModelError("", result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error updating role: {RoleId}", model.RoleId);
            ModelState.AddModelError("", "Failed to update role. Please try again.");
        }

        // Reload tenants for dropdown on error
        try
        {
            var tenants = await _tenantService.GetAllTenantsAsync();
            model.AvailableTenants = tenants.Select(t => new SelectListItem 
            { 
                Value = t.TenantId.ToString(), 
                Text = $"{t.TenantName} ({t.TenantCode})",
                Selected = t.TenantId == model.TenantId
            }).ToList();
        }
        catch
        {
            model.AvailableTenants = new List<SelectListItem>();
        }

        return View(model);
    }

    /// <summary>
    /// Handle role deletion
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _userService.DeleteRoleAsync(id, CurrentUserName);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error deleting role: {RoleId}", id);
            TempData["ErrorMessage"] = "An error occurred while deleting the role.";
            return RedirectToAction(nameof(Index));
        }
    }    /// <summary>
    /// Get role permissions from database with fallback to defaults
    /// </summary>
    private async Task<Dictionary<SystemModule, PermissionLevel>> GetRolePermissionsFromDatabaseAsync(Guid roleId, UserRole role)
    {
        try
        {
            return await _permissionService.GetRolePermissionsAsync(roleId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading permissions for role: {RoleId}", roleId);
            // Fall back to default permissions if database load fails
            return GetDefaultPermissionsForRole(role);
        }
    }

    /// <summary>
    /// Get default permissions for a role based on its type
    /// </summary>
    private Dictionary<SystemModule, PermissionLevel> GetDefaultPermissionsForRole(UserRole role)
    {
        var permissions = new Dictionary<SystemModule, PermissionLevel>();

        // Initialize all modules with no access by default
        foreach (var module in Enum.GetValues<SystemModule>())
        {
            permissions[module] = PermissionLevel.None;
        }

        // Set permissions based on role type and name
        if (role.IsAdmin || role.RoleName.Equals("Administrator", StringComparison.OrdinalIgnoreCase))
        {
            // Administrators get approver access to all modules
            foreach (var module in Enum.GetValues<SystemModule>())
            {
                permissions[module] = PermissionLevel.Approver;
            }
        }
        else if (role.RoleName.Equals("Manager", StringComparison.OrdinalIgnoreCase))
        {
            // Managers get read/write access to most modules, approver for some
            permissions[SystemModule.Products] = PermissionLevel.ReadWrite;
            permissions[SystemModule.Consumers] = PermissionLevel.ReadWrite;
            permissions[SystemModule.Licenses] = PermissionLevel.Approver;
            permissions[SystemModule.Approvals] = PermissionLevel.Approver;
            permissions[SystemModule.Reports] = PermissionLevel.ReadOnly;
            permissions[SystemModule.Audit] = PermissionLevel.ReadOnly;
        }
        else if (role.RoleName.Equals("User", StringComparison.OrdinalIgnoreCase))
        {
            // Standard users get read-only access to basic modules
            permissions[SystemModule.Products] = PermissionLevel.ReadOnly;
            permissions[SystemModule.Consumers] = PermissionLevel.ReadOnly;
            permissions[SystemModule.Licenses] = PermissionLevel.ReadOnly;
            permissions[SystemModule.Reports] = PermissionLevel.ReadOnly;
        }

        return permissions;
    }

    /// <summary>
    /// Parse permissions from form data
    /// </summary>
    private Dictionary<SystemModule, PermissionLevel> ParsePermissionsFromForm()
    {
        var permissions = new Dictionary<SystemModule, PermissionLevel>();

        foreach (var module in Enum.GetValues<SystemModule>())
        {
            var key = $"Permissions[{(int)module}]";
            if (Request.Form.ContainsKey(key) && 
                Enum.TryParse<PermissionLevel>(Request.Form[key], out var level))
            {
                permissions[module] = level;
            }
            else
            {
                permissions[module] = PermissionLevel.None;
            }
        }

        return permissions;
    }
}
