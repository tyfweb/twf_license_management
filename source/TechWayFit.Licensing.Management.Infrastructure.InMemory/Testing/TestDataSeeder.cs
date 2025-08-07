using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Products;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.User;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.License;

namespace TechWayFit.Licensing.Management.Infrastructure.InMemory.Testing;

/// <summary>
/// Test data seeder for InMemory database testing
/// </summary>
public static class TestDataSeeder
{
    /// <summary>
    /// Seed basic test data for product features testing
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="tenantId">Tenant ID for the test data</param>
    /// <returns>Task</returns>
    public static async Task SeedProductFeaturesTestDataAsync(
        EfCoreLicensingDbContext context, 
        Guid? tenantId = null)
    {
        var testTenantId = tenantId ?? Guid.Parse("00000000-0000-0000-0000-000000000001");
        var now = DateTime.UtcNow;

        // Create test product
        var product = new ProductEntity
        {
            ProductId = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Product Description",
            TenantId = testTenantId,
            CreatedOn = now,
            CreatedBy = "test-seeder"
        };

        context.Products.Add(product);

        // Create test product tier
        var tier = new ProductTierEntity
        {
            TierId = Guid.NewGuid(),
            ProductId = product.ProductId,
            Name = "Basic Tier",
            Description = "Basic tier for testing",
            TenantId = testTenantId,
            CreatedOn = now,
            CreatedBy = "test-seeder"
        };

        context.ProductTiers.Add(tier);

        // Create test product features
        var features = new[]
        {
            new ProductFeatureEntity
            {
                FeatureId = Guid.NewGuid(),
                ProductId = product.ProductId,
                TierId = tier.TierId,
                Name = "Feature 1",
                Description = "First test feature",
                Code = "FEAT_001",
                IsEnabled = true,
                DisplayOrder = 1,
                TenantId = testTenantId,
                CreatedOn = now,
                CreatedBy = "test-seeder"
            },
            new ProductFeatureEntity
            {
                FeatureId = Guid.NewGuid(),
                ProductId = product.ProductId,
                TierId = tier.TierId,
                Name = "Feature 2",
                Description = "Second test feature",
                Code = "FEAT_002",
                IsEnabled = true,
                DisplayOrder = 2,
                TenantId = testTenantId,
                CreatedOn = now,
                CreatedBy = "test-seeder"
            },
            new ProductFeatureEntity
            {
                FeatureId = Guid.NewGuid(),
                ProductId = product.ProductId,
                TierId = tier.TierId,
                Name = "Feature 3",
                Description = "Third test feature",
                Code = "FEAT_003",
                IsEnabled = false,
                DisplayOrder = 3,
                TenantId = testTenantId,
                CreatedOn = now,
                CreatedBy = "test-seeder"
            }
        };

        context.ProductFeatures.AddRange(features);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seed comprehensive test data for all entities
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="tenantId">Tenant ID for the test data</param>
    /// <returns>Task</returns>
    public static async Task SeedComprehensiveTestDataAsync(
        EfCoreLicensingDbContext context, 
        Guid? tenantId = null)
    {
        var testTenantId = tenantId ?? Guid.Parse("00000000-0000-0000-0000-000000000001");
        var now = DateTime.UtcNow;

        // Create test users
        var userRole = new UserRoleEntity
        {
            RoleId = Guid.NewGuid(),
            Name = "TestUser",
            Description = "Test user role",
            TenantId = testTenantId,
            CreatedOn = now,
            CreatedBy = "test-seeder"
        };

        context.UserRoles.Add(userRole);

        var userProfile = new UserProfileEntity
        {
            UserId = Guid.NewGuid(),
            TenantId = testTenantId,
            CreatedOn = now,
            CreatedBy = "test-seeder"
        };

        context.UserProfiles.Add(userProfile);

        // Create test consumer account
        var consumerAccount = new ConsumerAccountEntity
        {
            ConsumerId = Guid.NewGuid(),
            TenantId = testTenantId,
            CreatedOn = now,
            CreatedBy = "test-seeder"
        };

        context.ConsumerAccounts.Add(consumerAccount);

        // Seed product features
        await SeedProductFeaturesTestDataAsync(context, testTenantId);
    }

    /// <summary>
    /// Clear all test data from the context
    /// </summary>
    /// <param name="context">Database context</param>
    /// <returns>Task</returns>
    public static async Task ClearAllTestDataAsync(EfCoreLicensingDbContext context)
    {
        // Remove all entities in dependency order
        context.ProductFeatures.RemoveRange(context.ProductFeatures);
        context.ProductTiers.RemoveRange(context.ProductTiers);
        context.Products.RemoveRange(context.Products);
        context.ConsumerAccounts.RemoveRange(context.ConsumerAccounts);
        context.UserProfiles.RemoveRange(context.UserProfiles);
        context.UserRoles.RemoveRange(context.UserRoles);

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Get test tenant ID
    /// </summary>
    /// <returns>Default test tenant ID</returns>
    public static Guid GetTestTenantId() => Guid.Parse("00000000-0000-0000-0000-000000000001");

    /// <summary>
    /// Get alternative test tenant ID for multi-tenant testing
    /// </summary>
    /// <returns>Alternative test tenant ID</returns>
    public static Guid GetAlternativeTestTenantId() => Guid.Parse("00000000-0000-0000-0000-000000000002");
}
