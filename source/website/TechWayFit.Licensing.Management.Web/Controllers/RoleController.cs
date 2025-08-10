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
    private readonly ILogger<RoleController> _logger;

    public RoleController(
        IUserService userService,
        ITenantService tenantService,
        ILogger<RoleController> logger)
    {
        _userService = userService;
        _tenantService = tenantService;
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
            await _userService.InitializeDefaultRolesAsync();
            
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
                        UsersCount = 0 // TODO: Implement user count for each role
                    }).ToList()
                };            return View(viewModel);
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
    {
        try
        {
            var tenants = await _tenantService.GetAllTenantsAsync();
            
            var viewModel = new CreateRoleViewModel
            {
                AvailableTenants = tenants.Select(t => new SelectListItem 
                { 
                    Value = t.TenantId.ToString(), 
                    Text = $"{t.TenantName} ({t.TenantCode})" 
                }).ToList()
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
        }

        try
        {
            // Create the role using the user service
            var result = await _userService.CreateRoleAsync(
                model.RoleName,
                model.RoleDescription,
                model.IsAdminRole,
                "System" // TODO: Get current user from context
            );

            if (result.Success)
            {
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
}
