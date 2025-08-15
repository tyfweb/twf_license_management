using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Tenant;
using TechWayFit.Licensing.Management.Web.ViewModels.TenantSelector;

namespace TechWayFit.Licensing.Management.Web.ViewComponents;

/// <summary>
/// ViewComponent for rendering the tenant selector dropdown in the top bar
/// </summary>
public class TenantSelectorViewComponent : ViewComponent
{
    private readonly ITenantService _tenantService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TenantSelectorViewComponent> _logger;
    
    private const string TENANT_CACHE_KEY = "tenant_list_cache";
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(10);

    public TenantSelectorViewComponent(
        ITenantService tenantService,
        IMemoryCache cache,
        ILogger<TenantSelectorViewComponent> logger)
    {
        _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Renders the tenant selector if the user is an administrator
    /// </summary>
    /// <returns>ViewComponent result or empty result if user is not admin</returns>
    public async Task<IViewComponentResult> InvokeAsync()
    {
        try
        {
            // Check if user is authenticated and is an administrator
            if (!User.Identity?.IsAuthenticated == true || !User.IsInRole("Administrator"))
            {
                return Content(string.Empty); // Return empty content for non-admin users
            }

            var viewModel = await GetTenantSelectorViewModelAsync();
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering tenant selector for user {Username}", User.Identity?.Name);
            
            // Return a minimal view model to prevent UI breakage
            var fallbackViewModel = new TenantSelectorViewModel
            {
                IsAdministrator = User.IsInRole("Administrator"),
                Tenants = new List<TenantOption>(),
                CurrentTenantId = null,
                HasError = true,
                ErrorMessage = "Unable to load tenants"
            };
            
            return View(fallbackViewModel);
        }
    }

    /// <summary>
    /// Gets the tenant selector view model with cached data
    /// </summary>
    private async Task<TenantSelectorViewModel> GetTenantSelectorViewModelAsync()
    {
        var viewModel = new TenantSelectorViewModel
        {
            IsAdministrator = User.IsInRole("Administrator")
        };

        if (!viewModel.IsAdministrator)
        {
            return viewModel;
        }

        // Try to get tenants from cache first
        var tenants = await GetTenantsFromCacheAsync();
        
        viewModel.Tenants = tenants.Select(t => new TenantOption
        {
            Id = t.TenantId,
            Name = t.TenantName ?? $"Tenant {t.TenantId}",
            Code = t.TenantCode ?? "N/A",
            DisplayText = $"{t.TenantName ?? $"Tenant {t.TenantId}"} ({t.TenantCode ?? "N/A"})"
        }).ToList();

        // Get currently selected tenant from session
        var currentTenantIdString = HttpContext.Session.GetString("AdminSelectedTenantId");
        if (Guid.TryParse(currentTenantIdString, out var currentTenantId))
        {
            viewModel.CurrentTenantId = currentTenantId;
        }

        _logger.LogDebug("Loaded {Count} tenants for user {Username}, current tenant: {CurrentTenant}", 
            viewModel.Tenants.Count, User.Identity?.Name, viewModel.CurrentTenantId);

        return viewModel;
    }

    /// <summary>
    /// Gets tenant data from cache, with fallback to service call
    /// </summary>
    private async Task<IEnumerable<Tenant>> GetTenantsFromCacheAsync()
    {
        // Check cache first
        if (_cache.TryGetValue(TENANT_CACHE_KEY, out IEnumerable<Tenant>? cachedTenants) && cachedTenants != null)
        {
            _logger.LogDebug("Retrieved {Count} tenants from cache", cachedTenants.Count());
            return cachedTenants;
        }

        // Cache miss - fetch from service
        _logger.LogDebug("Cache miss - fetching tenants from service");
        
        try
        {
            var tenants = await _tenantService.GetAllTenantsAsync();
            var tenantList = tenants.ToList();

            // Cache the results
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CACHE_DURATION,
                SlidingExpiration = TimeSpan.FromMinutes(5),
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(TENANT_CACHE_KEY, tenantList, cacheOptions);
            
            _logger.LogInformation("Cached {Count} tenants for {Duration} minutes", 
                tenantList.Count, CACHE_DURATION.TotalMinutes);

            return tenantList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load tenants from service");
            
            // Return empty list on error to prevent UI breakage
            return new List<Tenant>();
        }
    }

    /// <summary>
    /// Invalidates the tenant cache (called when tenants are modified)
    /// </summary>
    public static void InvalidateTenantCache(IMemoryCache cache, ILogger logger)
    {
        cache.Remove(TENANT_CACHE_KEY);
        logger.LogInformation("Tenant cache invalidated");
    }
}
