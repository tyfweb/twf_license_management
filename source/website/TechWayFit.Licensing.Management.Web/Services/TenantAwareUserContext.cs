using System.Security.Claims;
using TechWayFit.Licensing.Management.Core.Contracts;
using TechWayFit.Licensing.Management.Core.Constants;

namespace TechWayFit.Licensing.Management.Web.Services;

/// <summary>
/// Tenant-aware user context that respects tenant scope overrides
/// </summary>
public class TenantAwareUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITenantScope _tenantScope;

    public TenantAwareUserContext(IHttpContextAccessor httpContextAccessor, ITenantScope tenantScope)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _tenantScope = tenantScope ?? throw new ArgumentNullException(nameof(tenantScope));
    }

    private ClaimsPrincipal? CurrentUser => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId => GetClaimValueAsGuid(ClaimTypes.NameIdentifier) ?? GetClaimValueAsGuid("sub");

    /// <summary>
    /// Gets the tenant ID, respecting tenant scope overrides and admin tenant selection
    /// </summary>
    public Guid? TenantId 
    { 
        get 
        {
            // First check if tenant scope is overridden
            if (_tenantScope.CurrentTenantId.HasValue)
                return _tenantScope.CurrentTenantId;

            // Check if admin has selected a specific tenant to view
            if (IsAdministrator && _httpContextAccessor.HttpContext?.Session != null)
            {
                var session = _httpContextAccessor.HttpContext.Session;
                var adminSelectedTenantId = session.GetString("AdminSelectedTenantId");
                if (!string.IsNullOrEmpty(adminSelectedTenantId) && Guid.TryParse(adminSelectedTenantId, out var selectedTenantId))
                {
                    return selectedTenantId;
                }
            }

            // Fall back to user's original tenant ID from claims
            return GetClaimValueAsGuid(TenantClaims.TenantId) ?? GetClaimValueAsGuid(TenantClaims.TenantIdAlternative);
        }
    }

    /// <summary>
    /// Gets the original tenant ID from claims (ignoring scope)
    /// </summary>
    public Guid? OriginalTenantId => GetClaimValueAsGuid(TenantClaims.TenantId) ?? GetClaimValueAsGuid(TenantClaims.TenantIdAlternative);

    public string? UserEmail => GetClaimValue(ClaimTypes.Email);

    public string? UserName 
    { 
        get 
        {
            var scopedUsername = _tenantScope.CurrentUsername;
            var claimUsername = GetClaimValue(ClaimTypes.Name) ?? GetClaimValue("preferred_username");
            
            var result = scopedUsername ?? claimUsername;
            
            return result;
        }
    }

    public IEnumerable<string> UserRoles => CurrentUser?.FindAll(ClaimTypes.Role)?.Select(c => c.Value) ?? Enumerable.Empty<string>();

    public bool IsAuthenticated => CurrentUser?.Identity?.IsAuthenticated ?? false;
    
    public bool IsAdmin => GetClaimValue(ClaimTypes.Role) == "Admin" || _tenantScope.IsSystemScope;

    /// <summary>
    /// Check if current user is an Administrator (case-insensitive)
    /// </summary>
    public bool IsAdministrator => UserRoles.Any(role => string.Equals(role, "Administrator", StringComparison.OrdinalIgnoreCase)) || _tenantScope.IsSystemScope;

    public bool HasRole(string role)
    {
        if (string.IsNullOrEmpty(role))
            return false;

        // System scope has all roles
        if (_tenantScope.IsSystemScope)
            return true;

        return CurrentUser?.IsInRole(role) ?? false;
    }

    public string? GetClaimValue(string claimType)
    {
        if (string.IsNullOrEmpty(claimType))
            return null;

        return CurrentUser?.FindFirst(claimType)?.Value;
    }
    
    public Guid? GetClaimValueAsGuid(string claimType)
    {
        var value = GetClaimValue(claimType);
        return Guid.TryParse(value, out var guid) ? guid : null;
    }
}
