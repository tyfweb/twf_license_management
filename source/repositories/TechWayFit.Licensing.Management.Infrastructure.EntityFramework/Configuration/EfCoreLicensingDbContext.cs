using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Data.Entities.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Notification;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Products;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.License;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Settings;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.User;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Common;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Audit;
using TechWayFit.Licensing.Management.Core.Contracts;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Tenants;
using System.Reflection.Emit;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Seeding;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration.EntityConfigurations;
using TechWayFit.Licensing.Management.Core.Helpers;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;

/// <summary>
/// Entity Framework DbContext for the licensing management system
/// This is the base DbContext that can be extended for specific database providers
/// </summary>
public partial class EfCoreLicensingDbContext : DbContext
{
    private readonly IUserContext _userContext;
    public EfCoreLicensingDbContext(
        DbContextOptions<EfCoreLicensingDbContext> options, IUserContext userContext) : base(options)
    {
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
    }

    #region DbSets

    // Product related entities
    public DbSet<ProductEntity> Products { get; set; }
    public DbSet<ProductVersionEntity> ProductVersions { get; set; }
    public DbSet<ProductTierEntity> ProductTiers { get; set; }
    public DbSet<ProductTierPriceEntity> ProductTierPrices { get; set; }
    public DbSet<ProductFeatureEntity> ProductFeatures { get; set; }
    public DbSet<ProductFeatureTierMappingEntity> ProductFeatureTierMappings { get; set; }
    public DbSet<ProductConsumerEntity> ProductConsumers { get; set; }
    public DbSet<ProductKeysEntity> ProductKeys { get; set; }

    // License related entities
    public DbSet<ProductLicenseEntity> ProductLicenses { get; set; }
    public DbSet<ProductActivationEntity> ProductActivations { get; set; }

    // Consumer related entities
    public DbSet<ConsumerAccountEntity> ConsumerAccounts { get; set; }
    public DbSet<ConsumerContactEntity> ConsumerContacts { get; set; }

    // Audit related entities
    public DbSet<AuditEntryEntity> AuditEntries { get; set; }

    // Notification related entities
    public DbSet<NotificationTemplateEntity> NotificationTemplates { get; set; }
    public DbSet<NotificationHistoryEntity> NotificationHistory { get; set; }

    // Settings related entities    public DbSet<SettingEntity> Settings { get; set; }

    // User related entities
    public DbSet<UserProfileEntity> UserProfiles { get; set; }
    public DbSet<UserRoleEntity> UserRoles { get; set; }
    public DbSet<UserRoleMappingEntity> UserRoleMappings { get; set; }
    public DbSet<RolePermissionEntity> RolePermissions { get; set; }
    
    public DbSet<TenantEntity> Tenants { get; set; }
    public DbSet<SeedingHistoryEntity> SeedingHistories { get; set; }

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply modular entity configurations
        // Product-related entities
        modelBuilder.ApplyConfiguration(new ProductEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ProductVersionEntityConfiguration());        modelBuilder.ApplyConfiguration(new ProductTierEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ProductTierPriceEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ProductFeatureEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ProductFeatureTierMappingEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ProductKeysEntityConfiguration());

        // User-related entities
        modelBuilder.ApplyConfiguration(new UserProfileEntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleEntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleMappingEntityConfiguration());
        modelBuilder.ApplyConfiguration(new RolePermissionEntityConfiguration());

        // Consumer-related entities
        modelBuilder.ApplyConfiguration(new ConsumerAccountEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ConsumerContactEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConsumerEntityConfiguration());

        // License-related entities
        modelBuilder.ApplyConfiguration(new ProductLicenseEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ProductActivationEntityConfiguration());

        // Notification-related entities
        modelBuilder.ApplyConfiguration(new NotificationTemplateEntityConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationHistoryEntityConfiguration());

        // Settings-related entities
        modelBuilder.ApplyConfiguration(new SettingEntityConfiguration());
        // Audit-related entities
        modelBuilder.ApplyConfiguration(new AuditEntryEntityConfiguration());
        // Seeding history entities
        modelBuilder.ApplyConfiguration(new SeedingHistoryEntityConfiguration());
        // Tenant-related entities
        modelBuilder.ApplyConfiguration(new TenantEntityConfiguration());

        // Configure indexes
        ConfigureIndexes(modelBuilder);

        // Configure global query filters for multi-tenancy
        ConfigureGlobalQueryFilters(modelBuilder);
    }
 
    /// <summary>
    /// Configure additional indexes for performance
    /// </summary>
    private static void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // Additional performance indexes can be added here
        // These are indexes that span multiple entities or are for specific query patterns
        
        // Add TenantId indexes for all entities to improve multi-tenant query performance
        ConfigureTenantIndexes(modelBuilder);
    }

    /// <summary>
    /// Configure TenantId indexes for all entities for optimal multi-tenant performance
    /// </summary>
    private static void ConfigureTenantIndexes(ModelBuilder modelBuilder)
    {
        // Product related entities
        modelBuilder.Entity<ProductEntity>().HasIndex(e => e.TenantId);
        modelBuilder.Entity<ProductVersionEntity>().HasIndex(e => e.TenantId);
        modelBuilder.Entity<ProductTierEntity>().HasIndex(e => e.TenantId);
        modelBuilder.Entity<ProductFeatureEntity>().HasIndex(e => e.TenantId);
        modelBuilder.Entity<ProductConsumerEntity>().HasIndex(e => e.TenantId);

        // License related entities
        modelBuilder.Entity<ProductLicenseEntity>().HasIndex(e => e.TenantId);
        modelBuilder.Entity<ProductActivationEntity>().HasIndex(e => e.TenantId);

        // Consumer related entities
        modelBuilder.Entity<ConsumerAccountEntity>().HasIndex(e => e.TenantId);


        // Notification related entities
        modelBuilder.Entity<NotificationTemplateEntity>().HasIndex(e => e.TenantId);
        modelBuilder.Entity<NotificationHistoryEntity>().HasIndex(e => e.TenantId);

        // Settings related entities
        modelBuilder.Entity<SettingEntity>().HasIndex(e => e.TenantId); 

        // User related entities
        modelBuilder.Entity<UserProfileEntity>().HasIndex(e => e.TenantId);        modelBuilder.Entity<UserRoleEntity>().HasIndex(e => e.TenantId);
        modelBuilder.Entity<UserRoleMappingEntity>().HasIndex(e => e.TenantId);
        modelBuilder.Entity<RolePermissionEntity>().HasIndex(e => e.TenantId);

        // Seeding related entities
        modelBuilder.Entity<SeedingHistoryEntity>().HasIndex(e => e.TenantId);

        // Composite indexes for frequently queried combinations
        modelBuilder.Entity<ProductEntity>().HasIndex(e => new { e.TenantId, e.IsActive });
        modelBuilder.Entity<ConsumerAccountEntity>().HasIndex(e => new { e.TenantId, e.IsActive });
        modelBuilder.Entity<ProductLicenseEntity>().HasIndex(e => new { e.TenantId, e.Status });
        modelBuilder.Entity<UserProfileEntity>().HasIndex(e => new { e.TenantId, e.IsActive, e.IsLocked });
    }
 

    /// <summary>
    /// Override SaveChanges to update audit fields and convert DateTime values to UTC automatically
    /// </summary>
    public override int SaveChanges()
    {
        UpdateAuditFields();
        var auditEntries = CollectAuditEntries();
        ConvertDateTimesToUtc();
        var result = base.SaveChanges();
        
        // Save audit entries after the main save operation
        if (auditEntries.Any())
        {
            SaveAuditEntries(auditEntries);
        }
        
        return result;
    }

    /// <summary>
    /// Override SaveChangesAsync to update audit fields and convert DateTime values to UTC automatically
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        var auditEntries = CollectAuditEntries();
        ConvertDateTimesToUtc();
        var result = await base.SaveChangesAsync(cancellationToken);
        
        // Save audit entries after the main save operation
        if (auditEntries.Any())
        {
            await SaveAuditEntriesAsync(auditEntries, cancellationToken);
        }
        
        return result;
    }

    /// <summary>
    /// Update audit fields before saving
    /// </summary>
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<AuditEntity>();
        var currentTime = DateTime.UtcNow;
        var currentTenantId = GetCurrentTenantId();
        var currentUsername = _userContext.UserName;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.Id == Guid.Empty)
                        entry.Entity.Id = Guid.NewGuid();// Ensure Id is set for new entities
                    entry.Entity.CreatedOn = currentTime;
                    // Only set CreatedBy if it's not already set (preserve explicit assignments)
                    if (string.IsNullOrWhiteSpace(entry.Entity.CreatedBy))
                    {
                        var createdBy = string.IsNullOrEmpty(currentUsername) ? "System" : currentUsername;
                        entry.Entity.CreatedBy = createdBy; // Default to System for seeding operations
                    }
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedOn = currentTime;
                    if (entry.Entity.UpdatedBy == null)
                    {
                        var updatedBy = string.IsNullOrEmpty(currentUsername) ? "System" : currentUsername;
                        entry.Entity.UpdatedBy = updatedBy; // Default to System for seeding operations
                    }
                    entry.Property(x => x.CreatedBy).IsModified = false;
                    entry.Property(x => x.CreatedOn).IsModified = false;
                   break;
            }
        }
        // Ensure all entities have TenantId set
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                if (entry.Entity.TenantId == Guid.Empty)
                {
                    entry.Entity.TenantId = currentTenantId; // Set TenantId from user context  
                }
            }
        }
    }

    /// <summary>
    /// Get the current tenant ID from the user context
    /// </summary>
    private Guid GetCurrentTenantId()
    {
        var tenantIdString = _userContext.TenantId;
        if (tenantIdString.HasValue)
        {
            // Return the tenant ID from user context
            return tenantIdString.Value;
        }
        
        // Return empty Guid if no tenant ID is available (should not happen in production)
        // Consider throwing an exception in production environments
        return Guid.Empty;
    }

    /// <summary>
    /// Collect audit entries for all entity changes (add/update) without adding them to the context
    /// </summary>
    private List<AuditEntryEntity> CollectAuditEntries()
    {
        var auditEntries = new List<AuditEntryEntity>();
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();

        foreach (var entry in entries)
        {
            var auditEntry = CreateAuditEntry(entry);
            if (auditEntry != null)
            {
                auditEntries.Add(auditEntry);
            }
        }
        
        return auditEntries;
    }

    /// <summary>
    /// Save audit entries in a separate operation (synchronous)
    /// </summary>
    private void SaveAuditEntries(List<AuditEntryEntity> auditEntries)
    {
        try
        {
            // Add audit entries to the context
            AuditEntries.AddRange(auditEntries);
            
            // Save only the audit entries
            base.SaveChanges();
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the main operation
            Console.WriteLine($"Failed to save audit entries: {ex.Message}");
            // Clear any failed audit entries from the context
            foreach (var entry in auditEntries)
            {
                Entry(entry).State = EntityState.Detached;
            }
        }
    }

    /// <summary>
    /// Save audit entries in a separate operation (asynchronous)
    /// </summary>
    private async Task SaveAuditEntriesAsync(List<AuditEntryEntity> auditEntries, CancellationToken cancellationToken)
    {
        try
        {
            // Add audit entries to the context
            AuditEntries.AddRange(auditEntries);
            
            // Save only the audit entries
            await base.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the main operation
            Console.WriteLine($"Failed to save audit entries: {ex.Message}");
            // Clear any failed audit entries from the context
            foreach (var entry in auditEntries)
            {
                Entry(entry).State = EntityState.Detached;
            }
        }
    }

    /// <summary>
    /// Create an individual audit entry for an entity change
    /// </summary>
    private AuditEntryEntity? CreateAuditEntry(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<BaseEntity> entry)
    {
        var entityType = entry.Entity.GetType().Name;
        var entityId = entry.Entity.Id.ToString(); // Convert Guid to string for generic audit table
        var actionType = entry.State == EntityState.Added ? "CREATE" : "UPDATE";
        var currentUser = entry.State == EntityState.Added ? entry.Entity.CreatedBy : entry.Entity.UpdatedBy ?? "System";

        // Determine if this is a security audit (User/UserProfile related changes)
        var isSecurityAudit = entityType.Contains("User", StringComparison.OrdinalIgnoreCase);
        var auditCategory = isSecurityAudit ? "Security Audit" : "Standard Audit";

        var auditEntry = new AuditEntryEntity
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            ActionType = actionType,
            CreatedBy = currentUser,
            CreatedOn = DateTime.UtcNow,
            IsActive = true,
            Reason = auditCategory
        };

        // Capture old and new values for updates
        if (entry.State == EntityState.Modified)
        {
            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                if (property.IsModified && 
                    !IsAuditField(property.Metadata.Name) && 
                    !IsSensitiveField(property.Metadata.Name))
                {
                    oldValues[property.Metadata.Name] = property.OriginalValue;
                    newValues[property.Metadata.Name] = property.CurrentValue;
                }
            }

            if (oldValues.Any())
            {
                auditEntry.OldValue = System.Text.Json.JsonSerializer.Serialize(oldValues);
                auditEntry.NewValue = System.Text.Json.JsonSerializer.Serialize(newValues);
            }
        }
        else if (entry.State == EntityState.Added)
        {
            // For new entities, capture the entity data (excluding sensitive fields)
            var entityData = new Dictionary<string, object?>();
            
            foreach (var property in entry.Properties)
            {
                if (!IsAuditField(property.Metadata.Name) && 
                    !IsSensitiveField(property.Metadata.Name))
                {
                    entityData[property.Metadata.Name] = property.CurrentValue;
                }
            }

            auditEntry.NewValue = System.Text.Json.JsonSerializer.Serialize(entityData);
        }

        // Add metadata for security audits
        if (isSecurityAudit)
        {
            var metadata = new Dictionary<string, object>
            {
                ["AuditCategory"] = "Security",
                ["RequiresReview"] = true,
                ["Timestamp"] = DateTime.UtcNow,
                ["EntityType"] = entityType
            };
            auditEntry.Metadata = System.Text.Json.JsonSerializer.Serialize(metadata);
        }

        return auditEntry;
    }

    /// <summary>
    /// Check if a property is an audit field that should not be included in audit logs
    /// </summary>
    private static bool IsAuditField(string propertyName)
    {
        var auditFields = new[]
        {
            nameof(BaseEntity.CreatedBy),
            nameof(BaseEntity.CreatedOn),
            nameof(BaseEntity.UpdatedBy),
            nameof(BaseEntity.UpdatedOn),
            nameof(BaseEntity.DeletedBy),
            nameof(BaseEntity.DeletedOn),
            nameof(BaseEntity.IsActive),
            nameof(BaseEntity.IsDeleted)
        };

        return auditFields.Contains(propertyName, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Check if a property contains sensitive data that should not be logged
    /// </summary>
    private static bool IsSensitiveField(string propertyName)
    {
        var sensitiveFields = new[]
        {
            "Password", "PasswordHash", "PasswordSalt", "Token", "Secret", 
            "Key", "PrivateKey", "ConnectionString", "ApiKey", "LicenseKey"
        };

        return sensitiveFields.Any(field => 
            propertyName.Contains(field, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Convert all DateTime properties to UTC before saving to PostgreSQL
    /// This ensures PostgreSQL compatibility with 'timestamp with time zone' columns
    /// </summary>
    private void ConvertDateTimesToUtc()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            foreach (var property in entry.Properties)
            {
                if (property.Metadata.ClrType == typeof(DateTime) ||
                    property.Metadata.ClrType == typeof(DateTime?))
                {
                    var value = property.CurrentValue;

                    if (value is DateTime dateTime)
                    {
                        // Convert Local or Unspecified DateTime to UTC
                        if (dateTime.Kind == DateTimeKind.Local)
                        {
                            property.CurrentValue = dateTime.ToUniversalTime();
                        }
                        else if (dateTime.Kind == DateTimeKind.Unspecified)
                        {
                            // Assume local time and convert to UTC
                            property.CurrentValue = DateTime.SpecifyKind(dateTime, DateTimeKind.Local).ToUniversalTime();
                        }
                        // UTC DateTime values are left unchanged
                    }
                }
            }
        }
    }

    /// <summary>
    /// Configure global query filters for multi-tenancy
    /// </summary>
    private void ConfigureGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        // Product related entities
        modelBuilder.Entity<ProductEntity>().HasQueryFilter(e =>GetCurrentTenantId()==IdConstants.SystemTenantId || e.TenantId == GetCurrentTenantId());
        modelBuilder.Entity<ProductVersionEntity>().HasQueryFilter(e =>GetCurrentTenantId()==IdConstants.SystemTenantId ||  e.TenantId == GetCurrentTenantId());
        modelBuilder.Entity<ProductTierEntity>().HasQueryFilter(e =>GetCurrentTenantId()==IdConstants.SystemTenantId ||  e.TenantId == GetCurrentTenantId());
        modelBuilder.Entity<ProductFeatureEntity>().HasQueryFilter(e =>GetCurrentTenantId()==IdConstants.SystemTenantId ||  e.TenantId == GetCurrentTenantId());
        modelBuilder.Entity<ProductConsumerEntity>().HasQueryFilter(e =>GetCurrentTenantId()==IdConstants.SystemTenantId ||  e.TenantId == GetCurrentTenantId());

        // License related entities
        modelBuilder.Entity<ProductLicenseEntity>().HasQueryFilter(e =>GetCurrentTenantId()==IdConstants.SystemTenantId ||  e.TenantId == GetCurrentTenantId());

        // Consumer related entities
        modelBuilder.Entity<ConsumerAccountEntity>().HasQueryFilter(e =>GetCurrentTenantId()==IdConstants.SystemTenantId ||  e.TenantId == GetCurrentTenantId());

        // Notification related entities
        modelBuilder.Entity<NotificationTemplateEntity>().HasQueryFilter(e =>GetCurrentTenantId()==IdConstants.SystemTenantId ||  e.TenantId == GetCurrentTenantId());
        modelBuilder.Entity<NotificationHistoryEntity>().HasQueryFilter(e =>GetCurrentTenantId()==IdConstants.SystemTenantId ||  e.TenantId == GetCurrentTenantId());

        // Settings related entities
        modelBuilder.Entity<SettingEntity>().HasQueryFilter(e =>GetCurrentTenantId()==IdConstants.SystemTenantId ||  e.TenantId == GetCurrentTenantId());

        // User related entities
        modelBuilder.Entity<UserProfileEntity>().HasQueryFilter(e =>GetCurrentTenantId()==IdConstants.SystemTenantId ||  e.TenantId == GetCurrentTenantId());        modelBuilder.Entity<UserRoleEntity>().HasQueryFilter(e =>GetCurrentTenantId()==IdConstants.SystemTenantId ||  e.TenantId == GetCurrentTenantId());
        modelBuilder.Entity<UserRoleMappingEntity>().HasQueryFilter(e =>GetCurrentTenantId()==IdConstants.SystemTenantId ||  e.TenantId == GetCurrentTenantId());
        modelBuilder.Entity<RolePermissionEntity>().HasQueryFilter(e =>GetCurrentTenantId()==IdConstants.SystemTenantId ||  e.TenantId == GetCurrentTenantId());

        // Seeding related entities
        modelBuilder.Entity<SeedingHistoryEntity>().HasQueryFilter(e =>GetCurrentTenantId()==IdConstants.SystemTenantId ||  e.TenantId == GetCurrentTenantId());
    }

}
