# Tenant Scope Implementation

## Overview

The tenant scope functionality provides a way to explicitly control which tenant context is used for operations, particularly important for system operations like seeding that should always use the system tenant regardless of the current user's tenant.

## Problem Solved

Previously, the system automatically used `userContext.TenantId` from the HTTP context, which could lead to:
- Unexpected tenant creation during system operations
- Seeding data being created in user tenants instead of the system tenant
- Difficulty performing operations across tenant boundaries

## Components

### 1. ITenantScope Interface

Located: `TechWayFit.Licensing.Management.Core/Contracts/ITenantScope.cs`

Provides methods to:
- Get the current effective tenant ID
- Create system tenant scopes
- Create specific tenant scopes
- Check if currently in system scope

### 2. TenantScope Service

Located: `TechWayFit.Licensing.Management.Web/Services/TenantScope.cs`

Thread-safe implementation of tenant scoping using ThreadLocal storage.

### 3. TenantAwareUserContext

Located: `TechWayFit.Licensing.Management.Web/Services/TenantAwareUserContext.cs`

Enhanced UserContext that respects tenant scope overrides:
- Uses scoped tenant ID when available
- Falls back to user claims when no scope is active
- Provides system admin privileges when in system scope

### 4. TenantScopeExtensions

Located: `TechWayFit.Licensing.Management.Core/Extensions/TenantScopeExtensions.cs`

Extension methods for convenient tenant scope operations.

## Usage Examples

### 1. System Operations (like seeding)

```csharp
public class TenantSeeder : BaseDataSeeder
{
    public async Task<bool> SeedAsync(CancellationToken cancellationToken = default)
    {
        // Automatically uses system tenant scope
        using var systemScope = _tenantScope.CreateSystemScope();
        
        // All operations here will use system tenant
        await _unitOfWork.Tenants.AddAsync(systemTenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return true;
    }
}
```

### 2. Cross-Tenant Operations

```csharp
public class AdminService
{
    private readonly ITenantScope _tenantScope;
    private readonly IUnitOfWork _unitOfWork;

    public async Task PerformCrossTenantOperation(Guid targetTenantId)
    {
        // Temporarily switch to specific tenant
        using var tenantScope = _tenantScope.CreateTenantScope(targetTenantId);
        
        // Operations here will use targetTenantId
        var tenantData = await _unitOfWork.SomeRepository.GetAllAsync();
    }
}
```

### 3. Using Extension Methods

```csharp
public class SomeService
{
    private readonly ITenantScope _tenantScope;

    public async Task<T> DoSystemOperation()
    {
        // Convenient extension method
        return await _tenantScope.ExecuteInSystemScopeAsync(async () =>
        {
            // System operation code here
            return await SomeOperation();
        });
    }
}
```

## Configuration

The tenant scope services are registered in Program.cs:

```csharp
// Register Tenant Scope and User Context services
builder.Services.AddScoped<ITenantScope, TenantScope>();
builder.Services.AddScoped<IUserContext, TenantAwareUserContext>();
```

## Benefits

1. **Explicit Control**: Clear intention when operations need specific tenant context
2. **System Operations**: Ensures seeding and other system operations always use system tenant
3. **Flexibility**: Allows for cross-tenant operations when needed
4. **Safety**: Scoped operations are automatically restored when scope is disposed
5. **Thread-Safe**: Uses ThreadLocal storage to prevent scope bleeding between requests

## Migration from Previous Implementation

### Before
```csharp
// Tenant was always determined by user context
var currentTenantId = _userContext.TenantId; // Could be any user's tenant
```

### After
```csharp
// Explicit control over tenant context
using var systemScope = _tenantScope.CreateSystemScope();
var currentTenantId = _userContext.TenantId; // Always system tenant within scope
```

## System Tenant ID

The system tenant ID is defined as a constant:
```csharp
public static Guid SystemTenantId = Guid.Parse("{3872beda-b291-4485-bb23-1b7a30a61d1a}");
```

This ensures all system operations use a consistent, known tenant ID.

## Best Practices

1. **Always use system scope for seeding**: Ensures data is created in the correct tenant
2. **Dispose scopes properly**: Use `using` statements to ensure automatic cleanup
3. **Minimize scope duration**: Keep scoped operations as short as possible
4. **Document tenant scope usage**: Make it clear when operations change tenant context
5. **Test with different tenants**: Verify operations work correctly across tenant boundaries
