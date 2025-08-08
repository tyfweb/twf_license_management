using System.Security.Claims;
using TechWayFit.Licensing.Management.Core.Contracts;
using TechWayFit.Licensing.Management.Core.Constants;

public class TwfUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TwfUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    private ClaimsPrincipal? CurrentUser => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId => GetClaimValueAsGuid(ClaimTypes.NameIdentifier) ?? GetClaimValueAsGuid("sub");

    public Guid? TenantId => GetClaimValueAsGuid(TenantClaims.TenantId) ?? GetClaimValueAsGuid(TenantClaims.TenantIdAlternative);

    public string? UserEmail => GetClaimValue(ClaimTypes.Email);

    public string? UserName => GetClaimValue(ClaimTypes.Name) ?? GetClaimValue("preferred_username");

    public IEnumerable<string> UserRoles => CurrentUser?.FindAll(ClaimTypes.Role)?.Select(c => c.Value) ?? Enumerable.Empty<string>();

    public bool IsAuthenticated => CurrentUser?.Identity?.IsAuthenticated ?? false;
    public bool IsAdmin => GetClaimValue(ClaimTypes.Role) == "Admin";

    public bool HasRole(string role)
    {
        if (string.IsNullOrEmpty(role))
            return false;

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