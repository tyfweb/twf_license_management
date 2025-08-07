using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechWayFit.Licensing.Management.Core.Contracts;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.InMemory.Extensions;

namespace TechWayFit.Licensing.Management.Infrastructure.InMemory.Testing;

/// <summary>
/// Test database context factory for creating isolated InMemory databases for testing
/// </summary>
public static class InMemoryTestContextFactory
{
    /// <summary>
    /// Create a test service provider with InMemory database
    /// </summary>
    /// <param name="testName">Optional test name for unique database naming</param>
    /// <param name="configureServices">Optional action to configure additional services</param>
    /// <returns>Service provider for testing</returns>
    public static IServiceProvider CreateTestServiceProvider(
        string? testName = null,
        Action<IServiceCollection>? configureServices = null)
    {
        var services = new ServiceCollection();
        
        // Add InMemory infrastructure
        services.AddInMemoryInfrastructureForTesting(testName);
        
        // Add mock user context for testing
        services.AddScoped<IUserContext, MockUserContext>();
        
        // Configure additional services if provided
        configureServices?.Invoke(services);
        
        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Create a test database context with seeded data
    /// </summary>
    /// <param name="seedAction">Action to seed test data</param>
    /// <param name="testName">Optional test name for unique database naming</param>
    /// <returns>Tuple of service provider and database context</returns>
    public static async Task<(IServiceProvider ServiceProvider, EfCoreLicensingDbContext Context)> 
        CreateTestContextWithDataAsync(
            Func<EfCoreLicensingDbContext, Task> seedAction,
            string? testName = null)
    {
        var serviceProvider = CreateTestServiceProvider(testName);
        var context = serviceProvider.GetRequiredService<EfCoreLicensingDbContext>();
        
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        
        // Seed data
        await seedAction(context);
        await context.SaveChangesAsync();
        
        return (serviceProvider, context);
    }

    /// <summary>
    /// Create a clean test database context
    /// </summary>
    /// <param name="testName">Optional test name for unique database naming</param>
    /// <returns>Tuple of service provider and database context</returns>
    public static async Task<(IServiceProvider ServiceProvider, EfCoreLicensingDbContext Context)> 
        CreateCleanTestContextAsync(string? testName = null)
    {
        var serviceProvider = CreateTestServiceProvider(testName);
        var context = serviceProvider.GetRequiredService<EfCoreLicensingDbContext>();
        
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        
        return (serviceProvider, context);
    }
}

/// <summary>
/// Mock user context implementation for testing
/// </summary>
public class MockUserContext : IUserContext
{
    public string UserId { get; set; } = "test-user-id";
    public string? Email { get; set; } = "test@example.com";
    public string? FirstName { get; set; } = "Test";
    public string? LastName { get; set; } = "User";
    public Guid TenantId { get; set; } = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public bool IsAuthenticated { get; set; } = true;
    public IEnumerable<string> Roles { get; set; } = new[] { "User" };
    public IEnumerable<string> Permissions { get; set; } = new[] { "Read", "Write" };

    /// <summary>
    /// Create a mock user context for testing with custom values
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="roles">User roles</param>
    /// <returns>Mock user context</returns>
    public static MockUserContext Create(
        string userId = "test-user-id",
        Guid? tenantId = null,
        IEnumerable<string>? roles = null)
    {
        return new MockUserContext
        {
            UserId = userId,
            TenantId = tenantId ?? Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Roles = roles ?? new[] { "User" }
        };
    }

    /// <summary>
    /// Create a mock admin user context for testing
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Mock admin user context</returns>
    public static MockUserContext CreateAdmin(
        string userId = "admin-user-id",
        Guid? tenantId = null)
    {
        return new MockUserContext
        {
            UserId = userId,
            TenantId = tenantId ?? Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Roles = new[] { "Admin", "User" },
            Permissions = new[] { "Read", "Write", "Admin" }
        };
    }
}
