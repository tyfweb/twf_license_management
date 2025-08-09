using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Tenants;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.User;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Settings;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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

    public DebugController(IUnitOfWork unitOfWork, EfCoreLicensingDbContext context, IWebHostEnvironment environment)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _environment = environment;
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
            var tenants = await _context.Set<TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Tenants.TenantEntity>()
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
            var roles = await _context.Set<UserRoleEntity>()
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
            var settings = await _context.Set<SettingEntity>()
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
            var profiles = await _context.Set<UserProfileEntity>()
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
                    .AsNoTracking().ToListAsync(),
                UserRoles = await _context.Set<UserRoleEntity>()
                    .AsNoTracking().ToListAsync(),
                Settings = await _context.Set<SettingEntity>()
                    .AsNoTracking().ToListAsync(),
                UserProfiles = await _context.Set<UserProfileEntity>()
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
                ["Tenants"] = await _context.Set<TenantEntity>().CountAsync(),
                ["UserRoles"] = await _context.Set<UserRoleEntity>().CountAsync(),
                ["Settings"] = await _context.Set<SettingEntity>().CountAsync(),
                ["UserProfiles"] = await _context.Set<UserProfileEntity>().CountAsync()
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
            // Remove entities in reverse dependency order
            var userProfiles = await _context.Set<UserProfileEntity>().ToListAsync();
            _context.Set<UserProfileEntity>().RemoveRange(userProfiles);

            var settings = await _context.Set<SettingEntity>().ToListAsync();
            _context.Set<SettingEntity>().RemoveRange(settings);

            var userRoles = await _context.Set<UserRoleEntity>().ToListAsync();
            _context.Set<UserRoleEntity>().RemoveRange(userRoles);

            var tenants = await _context.Set<TenantEntity>().ToListAsync();
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
}
