using TechWayFit.Licensing.Management.Core.Contracts;
using TechWayFit.Licensing.Management.Core.Helpers;

namespace TechWayFit.Licensing.Management.Web.Services;

/// <summary>
/// Service for managing tenant scope during operations
/// </summary>
public class TenantScope : ITenantScope, IDisposable
{
    private readonly ThreadLocal<Guid?> _scopedTenantId = new();
    private readonly ThreadLocal<bool> _isSystemScope = new();
    private readonly ThreadLocal<string?> _scopedUsername = new();

    public TenantScope()
    {
    }

    public Guid? CurrentTenantId => _scopedTenantId.Value;

    public string? CurrentUsername => _scopedUsername.Value;

    public bool IsSystemScope => _isSystemScope.Value;

    public IDisposable CreateSystemScope()
    {
        var result = CreateTenantScope(IdConstants.SystemTenantId, "System", isSystem: true);
        return result;
    }

    public IDisposable CreateTenantScope(Guid tenantId)
    {
        return CreateTenantScope(tenantId, null, isSystem: false);
    }

    private IDisposable CreateTenantScope(Guid tenantId, string? username, bool isSystem)
    {
        return new TenantScopeDisposable(this, tenantId, username, isSystem);
    }

    private void SetScope(Guid? tenantId, string? username, bool isSystem)
    {
        _scopedTenantId.Value = tenantId;
        _scopedUsername.Value = username;
        _isSystemScope.Value = isSystem;
    }

    private void ClearScope()
    {
        _scopedTenantId.Value = null;
        _scopedUsername.Value = null;
        _isSystemScope.Value = false;
    }

    private class TenantScopeDisposable : IDisposable
    {
        private readonly TenantScope _tenantScope;
        private readonly Guid? _previousTenantId;
        private readonly string? _previousUsername;
        private readonly bool _previousIsSystemScope;
        private bool _disposed = false;

        public TenantScopeDisposable(TenantScope tenantScope, Guid tenantId, string? username, bool isSystem)
        {
            _tenantScope = tenantScope;
            _previousTenantId = tenantScope._scopedTenantId.Value;
            _previousUsername = tenantScope._scopedUsername.Value;
            _previousIsSystemScope = tenantScope._isSystemScope.Value;
            
            // Set the new scope
            tenantScope.SetScope(tenantId, username, isSystem);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                // Restore previous scope
                if (_previousTenantId.HasValue)
                {
                    _tenantScope.SetScope(_previousTenantId.Value, _previousUsername, _previousIsSystemScope);
                }
                else
                {
                    _tenantScope.ClearScope();
                }
                
                _disposed = true;
            }
        }
    }

    public void Dispose()
    {
        _scopedTenantId?.Dispose();
        _scopedUsername?.Dispose();
        _isSystemScope?.Dispose();
    }
}
