namespace TechWayFit.Licensing.Management.Core.Constants;

/// <summary>
/// Constants for tenant-related claims used in multi-tenant authentication
/// </summary>
public static class TenantClaims
{
    /// <summary>
    /// Primary tenant ID claim type
    /// </summary>
    public const string TenantId = "tenant_id";

    /// <summary>
    /// Alternative tenant ID claim type for compatibility
    /// </summary>
    public const string TenantIdAlternative = "tenantId";

    /// <summary>
    /// Tenant name claim type
    /// </summary>
    public const string TenantName = "tenant_name";

    /// <summary>
    /// Tenant admin role claim type
    /// </summary>
    public const string TenantAdmin = "tenant_admin";

    /// <summary>
    /// System admin role claim type (cross-tenant access)
    /// </summary>
    public const string SystemAdmin = "system_admin";
}
