using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Web.Controllers;
using TechWayFit.Licensing.Management.Web.ViewModels.Tenant;

namespace TechWayFit.Licensing.Management.Web.Controllers;

/// <summary>
/// Controller for managing tenants
/// </summary>
[Authorize]
public class TenantController : BaseController
{
    private readonly ITenantService _tenantService;
    private readonly IUserService _userService;

    public TenantController(
        ITenantService tenantService,
        IUserService userService)
    {
        _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    /// <summary>
    /// Display tenants list
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            var tenants = await _tenantService.GetAllTenantsAsync();
            
            var viewModel = new TenantListViewModel
            {
                Tenants = tenants.Select(t => new TenantViewModel
                {
                    TenantId = t.TenantId,
                    TenantName = t.TenantName,
                    TenantCode = t.TenantCode ?? string.Empty,
                    Description = t.Description ?? string.Empty,
                    Website = t.Website,
                    IsActive = true, // TODO: Add IsActive property to Tenant model
                    CreatedOn = DateTime.Now, // TODO: Add CreatedOn property to Tenant model
                    UserCount = 0, // TODO: Implement user count calculation
                    LicenseCount = 0 // TODO: Implement license count calculation
                }).ToList()
            };

            return View(viewModel);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to load tenants. Please try again.";
            return View(new TenantListViewModel());
        }
    }

    /// <summary>
    /// Display create tenant form
    /// </summary>
    [HttpGet]
    public IActionResult Create()
    {
        try
        {
            var viewModel = new CreateTenantViewModel();
            return View(viewModel);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to load tenant creation form. Please try again.";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// Create new tenant
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTenantViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Check if tenant code is unique if provided
            if (!string.IsNullOrWhiteSpace(model.TenantCode))
            {
                var isUnique = await _tenantService.IsTenantCodeUniqueAsync(model.TenantCode);
                if (!isUnique)
                {
                    ModelState.AddModelError(nameof(model.TenantCode), "Tenant code already exists. Please choose a different code.");
                    return View(model);
                }
            }

            var createdTenant = await _tenantService.CreateTenantAsync(
                model.TenantName,
                model.TenantCode,
                model.Description,
                model.Website);

            TempData["SuccessMessage"] = $"Tenant '{createdTenant.TenantName}' created successfully with code '{createdTenant.TenantCode}'.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Failed to create tenant: {ex.Message}");
            return View(model);
        }
    }
}
