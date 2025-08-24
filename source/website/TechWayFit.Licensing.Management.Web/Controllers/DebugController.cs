using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Tenants;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.User;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Settings;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TechWayFit.Licensing.Management.Core.Contracts;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Enums;

namespace TechWayFit.Licensing.Management.Web.Controllers;

/// <summary>
/// Debug controller for viewing InMemory database data
/// Only available in Development environment
/// </summary>
public class DebugController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly EfCoreLicensingDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ITenantScope _tenantScope;
    private readonly ILicenseActivationService _activationService;
    private readonly IProductLicenseService _licenseService;

    public DebugController(IUnitOfWork unitOfWork,
    EfCoreLicensingDbContext context,
    IWebHostEnvironment environment,
    ITenantScope tenantScope,
    ILicenseActivationService activationService,
    IProductLicenseService licenseService)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _environment = environment;
        _tenantScope = tenantScope;
        _activationService = activationService;
        _licenseService = licenseService;
    }

    /// <summary>
    /// Main debug page with links to all data views
    /// </summary>
    public IActionResult Index()
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        return View();
    }

    /// <summary>
    /// View all tenants in the InMemory database
    /// </summary>
    public async Task<IActionResult> Tenants()
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        try
        {
            var tenants = await _context.Set<TenantEntity>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .ToListAsync();

            ViewBag.DataType = "Tenants";
            ViewBag.Count = tenants.Count;
            return View("DataView", tenants);
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            return View("Error");
        }
    }

    /// <summary>
    /// View all user roles in the InMemory database
    /// </summary>
    public async Task<IActionResult> UserRoles()
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        try
        {
            // Use the correct EF Core IgnoreQueryFilters() method on the queryable
            var roles = await _context.Set<UserRoleEntity>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .ToListAsync();

            ViewBag.DataType = "User Roles";
            ViewBag.Count = roles.Count;
            return View("DataView", roles);
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            return View("Error");
        }
    }

    /// <summary>
    /// View all settings in the InMemory database
    /// </summary>
    public async Task<IActionResult> Settings()
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        try
        {
            // Use the correct EF Core IgnoreQueryFilters() method on the queryable
            var settings = await _context.Set<SettingEntity>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .ToListAsync();

            ViewBag.DataType = "Settings";
            ViewBag.Count = settings.Count;
            return View("DataView", settings);
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            return View("Error");
        }
    }

    /// <summary>
    /// View all user profiles in the InMemory database
    /// </summary>
    public async Task<IActionResult> UserProfiles()
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        try
        {
            // Use the correct EF Core IgnoreQueryFilters() method on the queryable
            var profiles = await _context.Set<UserProfileEntity>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .ToListAsync();

            ViewBag.DataType = "User Profiles";
            ViewBag.Count = profiles.Count;
            return View("DataView", profiles);
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            return View("Error");
        }
    }

    /// <summary>
    /// View all entities in a compact JSON format
    /// </summary>
    public async Task<IActionResult> AllData()
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        try
        {
            var data = new
            {
                Tenants = await _context.Set<TenantEntity>()
                    .IgnoreQueryFilters()
                    .AsNoTracking().ToListAsync(),
                UserRoles = await _context.Set<UserRoleEntity>()
                    .IgnoreQueryFilters()
                    .AsNoTracking().ToListAsync(),
                Settings = await _context.Set<SettingEntity>()
                    .IgnoreQueryFilters()
                    .AsNoTracking().ToListAsync(),
                UserProfiles = await _context.Set<UserProfileEntity>()
                    .IgnoreQueryFilters()
                    .AsNoTracking().ToListAsync()
            };

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            ViewBag.JsonData = json;
            return View("JsonView");
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            return View("Error");
        }
    }

    /// <summary>
    /// Get raw database statistics
    /// </summary>
    public async Task<IActionResult> Stats()
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        try
        {
            var stats = new Dictionary<string, int>
            {
                ["Tenants"] = await _context.Set<TenantEntity>().IgnoreQueryFilters().CountAsync(),
                ["UserRoles"] = await _context.Set<UserRoleEntity>().IgnoreQueryFilters().CountAsync(),
                ["Settings"] = await _context.Set<SettingEntity>().IgnoreQueryFilters().CountAsync(),
                ["UserProfiles"] = await _context.Set<UserProfileEntity>().IgnoreQueryFilters().CountAsync()
            };

            ViewBag.Stats = stats;
            return View("StatsView");
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            return View("Error");
        }
    }

    /// <summary>
    /// Clear all data from InMemory database (Development only)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ClearData()
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        try
        {
            // Remove entities in reverse dependency order - use bypass methods for cross-tenant deletion
            var userProfiles = await _context.Set<UserProfileEntity>().IgnoreQueryFilters().ToListAsync();
            _context.Set<UserProfileEntity>().RemoveRange(userProfiles);

            var settings = await _context.Set<SettingEntity>().IgnoreQueryFilters().ToListAsync();
            _context.Set<SettingEntity>().RemoveRange(settings);

            var userRoles = await _context.Set<UserRoleEntity>().IgnoreQueryFilters().ToListAsync();
            _context.Set<UserRoleEntity>().RemoveRange(userRoles);

            var tenants = await _context.Set<TenantEntity>().IgnoreQueryFilters().ToListAsync();
            _context.Set<TenantEntity>().RemoveRange(tenants);

            await _context.SaveChangesAsync();
            
            TempData["Success"] = "All data cleared from InMemory database";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to clear data: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// Test page for license activation
    /// </summary>
    public async Task<IActionResult> LicenseActivationTest()
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        try
        {
            // Get available licenses for testing
            var licenses = await _licenseService.GetLicensesAsync(pageSize: 10);
            ViewBag.AvailableLicenses = licenses.ToList();

            return View();
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to load test data: {ex.Message}";
            return View();
        }
    }

    /// <summary>
    /// Process license activation test
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> LicenseActivationTest(string activationKey, string deviceId, string deviceInfo)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        try
        {
            if (string.IsNullOrWhiteSpace(activationKey))
            {
                TempData["Error"] = "Activation key is required";
                return RedirectToAction("LicenseActivationTest");
            }

            if (string.IsNullOrWhiteSpace(deviceId))
            {
                deviceId = $"TEST-DEVICE-{Guid.NewGuid():N}"[..8];
            }

            if (string.IsNullOrWhiteSpace(deviceInfo))
            {
                deviceInfo = "Debug Test Device - Development Environment";
            }

            // Attempt to activate the license
            var result = await _activationService.ActivateLicenseAsync(activationKey, deviceId, deviceInfo);

            if (result.IsSuccessful)
            {
                TempData["Success"] = $"License activated successfully! License ID: {result.LicenseId}, Token: {result.ActivationToken}";
                TempData["ActivationDetails"] = JsonSerializer.Serialize(new
                {
                    LicenseId = result.LicenseId,
                    ActivationToken = result.ActivationToken,
                    ActivatedAt = result.ActivatedAt,
                    ExpiresAt = result.ExpiresAt,
                    DeviceId = deviceId,
                    DeviceInfo = deviceInfo,
                    Metadata = result.Metadata
                }, new JsonSerializerOptions { WriteIndented = true });
            }
            else
            {
                TempData["Error"] = $"License activation failed: {result.Message}";
                TempData["FailureDetails"] = JsonSerializer.Serialize(new
                {
                    ActivationKey = activationKey,
                    DeviceId = deviceId,
                    DeviceInfo = deviceInfo,
                    Error = result.Message,
                    ErrorCode = result.ErrorCode
                }, new JsonSerializerOptions { WriteIndented = true });
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Exception during activation: {ex.Message}";
            TempData["Exception"] = ex.ToString();
        }

        return RedirectToAction("LicenseActivationTest");
    }

    /// <summary>
    /// Test active devices for a license
    /// </summary>
    public async Task<IActionResult> TestActiveDevices(Guid? licenseId)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        try
        {
            if (!licenseId.HasValue)
            {
                // Get first available license for testing
                var licenses = await _licenseService.GetLicensesAsync(pageSize: 1);
                var firstLicense = licenses.FirstOrDefault();
                if (firstLicense != null)
                {
                    licenseId = firstLicense.LicenseId;
                }
                else
                {
                    TempData["Error"] = "No licenses found. Create a license first.";
                    return RedirectToAction("LicenseActivationTest");
                }
            }

            var activeDevices = await _activationService.GetActiveDevicesAsync(licenseId.Value);
            
            TempData["Success"] = $"Found {activeDevices.Count} active devices for license {licenseId}";
            TempData["DeviceList"] = JsonSerializer.Serialize(activeDevices, new JsonSerializerOptions { WriteIndented = true });

            return RedirectToAction("LicenseActivationTest");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error testing active devices: {ex.Message}";
            return RedirectToAction("LicenseActivationTest");
        }
    }

    /// <summary>
    /// Test license activation
    /// </summary>
    public async Task<IActionResult> TestActivateLicense(Guid? licenseId)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        try
        {
            if (!licenseId.HasValue)
            {
                TempData["Error"] = "License ID is required for activation test.";
                return RedirectToAction("LicenseActivationTest");
            }

            var activationInfo = new ActivationInfo
            {
                ActivatedBy = "debug-user",
                ActivationDate = DateTime.UtcNow,
                MachineId = Environment.MachineName,
                ActivationMetadata = new Dictionary<string, object>
                {
                    { "TestMode", true },
                    { "ActivationSource", "DebugController" },
                    { "UserAgent", Request.Headers.UserAgent.ToString() }
                }
            };

            var result = await _licenseService.ActivateLicenseAsync(licenseId.Value, activationInfo);
            
            if (result)
            {
                TempData["Success"] = $"Successfully activated license {licenseId}";
            }
            else
            {
                TempData["Error"] = $"Failed to activate license {licenseId}";
            }

            return RedirectToAction("LicenseActivationTest");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error activating license: {ex.Message}";
            return RedirectToAction("LicenseActivationTest");
        }
    }

    /// <summary>
    /// Test license deactivation
    /// </summary>
    public async Task<IActionResult> TestDeactivateLicense(Guid? licenseId)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        try
        {
            if (!licenseId.HasValue)
            {
                TempData["Error"] = "License ID is required for deactivation test.";
                return RedirectToAction("LicenseActivationTest");
            }

            var result = await _licenseService.DeactivateLicenseAsync(licenseId.Value, "debug-user", "Testing deactivation from debug controller");
            
            if (result)
            {
                TempData["Success"] = $"Successfully deactivated license {licenseId}";
            }
            else
            {
                TempData["Error"] = $"Failed to deactivate license {licenseId}";
            }

            return RedirectToAction("LicenseActivationTest");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error deactivating license: {ex.Message}";
            return RedirectToAction("LicenseActivationTest");
        }
    }

    /// <summary>
    /// Test license suspension
    /// </summary>
    public async Task<IActionResult> TestSuspendLicense(Guid? licenseId)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        try
        {
            if (!licenseId.HasValue)
            {
                TempData["Error"] = "License ID is required for suspension test.";
                return RedirectToAction("LicenseActivationTest");
            }

            var suspendUntil = DateTime.UtcNow.AddDays(7); // Suspend for 7 days
            var result = await _licenseService.SuspendLicenseAsync(licenseId.Value, "debug-user", 
                "Testing suspension from debug controller", suspendUntil);
            
            if (result)
            {
                TempData["Success"] = $"Successfully suspended license {licenseId} until {suspendUntil:yyyy-MM-dd}";
            }
            else
            {
                TempData["Error"] = $"Failed to suspend license {licenseId}";
            }

            return RedirectToAction("LicenseActivationTest");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error suspending license: {ex.Message}";
            return RedirectToAction("LicenseActivationTest");
        }
    }

    /// <summary>
    /// Test license revocation
    /// </summary>
    public async Task<IActionResult> TestRevokeLicense(Guid? licenseId)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        try
        {
            if (!licenseId.HasValue)
            {
                TempData["Error"] = "License ID is required for revocation test.";
                return RedirectToAction("LicenseActivationTest");
            }

            var result = await _licenseService.RevokeLicenseAsync(licenseId.Value, "debug-user", 
                "Testing revocation from debug controller - this is a permanent action");
            
            if (result)
            {
                TempData["Success"] = $"Successfully revoked license {licenseId} - THIS IS PERMANENT!";
            }
            else
            {
                TempData["Error"] = $"Failed to revoke license {licenseId}";
            }

            return RedirectToAction("LicenseActivationTest");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error revoking license: {ex.Message}";
            return RedirectToAction("LicenseActivationTest");
        }
    }
}
