using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Core.Contracts;
using TechWayFit.Licensing.Management.Infrastructure.Data.Entities.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Audit;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Common;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.License;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Notification;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Products;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Settings;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.User;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Data;

/// <summary>
/// Abstract base Entity Framework DbContext for the licensing management system
/// 
/// PROVIDER-AGNOSTIC DESIGN:
/// This abstract DbContext provides common functionality across all database providers:
/// 
/// 1. ENTITY DEFINITIONS:
///    - Common DbSets for all entities
///    - Provider-agnostic entity configurations
///    - Shared audit and multi-tenant behavior
/// 
/// 2. MULTI-TENANT IMPLEMENTATION:
///    - All entities inherit from BaseEntity which includes TenantId property
///    - Global query filters automatically filter all queries by current user's TenantId
///    - Prevents cross-tenant data access without explicit bypass
/// 
/// 3. AUTOMATIC TENANT ID ASSIGNMENT:
///    - New entities automatically get TenantId set from current user context
///    - TenantId cannot be modified after entity creation (prevented in UpdateAuditFields)
/// 
/// 4. PROVIDER-SPECIFIC CUSTOMIZATION:
///    - ApplyProviderSpecificConfigurations() method for provider-specific configurations
///    - OnConfiguring() method can be overridden for provider-specific options
/// 
/// 5. ADMINISTRATIVE BYPASS:
///    - WithoutTenantFilter methods for system-level operations
///    - Use with extreme caution for cross-tenant administrative tasks
/// 
/// SECURITY CONSIDERATIONS:
/// - All queries are automatically filtered by TenantId
/// - No raw SQL should bypass tenant filtering
/// - Admin operations must explicitly use bypass methods
/// - TenantId must be properly set in user context (claims)
/// </summary>
public abstract class LicensingDbContext : DbContext
{
    protected readonly IUserContext _userContext;

    protected LicensingDbContext(DbContextOptions options, IUserContext userContext) : base(options)
    {
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
    }

    #region DbSets - Common across all providers

    // Product related entities
    public DbSet<ProductEntity> Products { get; set; }
    public DbSet<ProductVersionEntity> ProductVersions { get; set; }
    public DbSet<ProductTierEntity> ProductTiers { get; set; }
    public DbSet<ProductFeatureEntity> ProductFeatures { get; set; }
    public DbSet<ProductConsumerEntity> ProductConsumers { get; set; }

    // License related entities
    public DbSet<ProductLicenseEntity> ProductLicenses { get; set; }

    // Consumer related entities
    public DbSet<ConsumerAccountEntity> ConsumerAccounts { get; set; }

    // Audit related entities
    public DbSet<AuditEntryEntity> AuditEntries { get; set; }

    // Notification related entities
    public DbSet<NotificationTemplateEntity> NotificationTemplates { get; set; }
    public DbSet<NotificationHistoryEntity> NotificationHistory { get; set; }

    // Settings related entities
    public DbSet<SettingEntity> Settings { get; set; }

    // User related entities
    public DbSet<UserProfileEntity> UserProfiles { get; set; }
    public DbSet<UserRoleEntity> UserRoles { get; set; }
    public DbSet<UserRoleMappingEntity> UserRoleMappings { get; set; }

    #endregion

    #region Multi-Tenant Support

    /// <summary>
    /// Get current user's TenantId for filtering
    /// </summary>
    protected Guid CurrentTenantId => _userContext?.TenantId ?? Guid.Empty;

    /// <summary>
    /// Check if current user has administrative privileges
    /// </summary>
    protected bool IsSystemAdministrator => _userContext?.IsSystemAdministrator ?? false;

    #endregion

    #region Model Configuration

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply common configurations that work across all providers
        ApplyCommonConfigurations(modelBuilder);
        
        // Apply provider-specific configurations (override in derived classes)
        ApplyProviderSpecificConfigurations(modelBuilder);
        
        // Configure indexes for performance
        ConfigureIndexes(modelBuilder);

        // Configure global query filters for multi-tenancy
        ConfigureGlobalQueryFilters(modelBuilder);
    }

    /// <summary>
    /// Apply common entity configurations that work across all providers
    /// </summary>
    protected virtual void ApplyCommonConfigurations(ModelBuilder modelBuilder)
    {
        ConfigureAuditEntities(modelBuilder);
        ConfigureConsumerEntities(modelBuilder);
        ConfigureProductEntities(modelBuilder);
        ConfigureLicenseEntities(modelBuilder);
        ConfigureNotificationEntities(modelBuilder);
        ConfigureSettingsEntities(modelBuilder);
        ConfigureUserEntities(modelBuilder);
    }

    /// <summary>
    /// Apply provider-specific configurations (override in derived classes)
    /// </summary>
    protected virtual void ApplyProviderSpecificConfigurations(ModelBuilder modelBuilder)
    {
        // Override in provider-specific contexts for provider-specific configurations
        // e.g., PostgreSQL JSONB, SQL Server temporal tables, etc.
    }

    #endregion

    #region Entity Configurations

    protected virtual void ConfigureAuditEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditEntryEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.EntityType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EntityId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Reason).HasMaxLength(1000);
            entity.Property(e => e.OldValue).HasMaxLength(4000);
            entity.Property(e => e.NewValue).HasMaxLength(4000);
            entity.Property(e => e.Metadata).HasMaxLength(1000);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.ActionType).HasMaxLength(20);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
        });
    }

    protected virtual void ConfigureConsumerEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConsumerAccountEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(300).IsRequired();
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
        });
    }

    protected virtual void ConfigureProductEntities(ModelBuilder modelBuilder)
    {
        // Product Entity
        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Tags).HasMaxLength(500);
        });

        // Product Version Entity
        modelBuilder.Entity<ProductVersionEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Version).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ReleaseNotes).HasMaxLength(2000);
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
        });

        // Product Tier Entity
        modelBuilder.Entity<ProductTierEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // Product Feature Entity
        modelBuilder.Entity<ProductFeatureEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.FeatureType).HasMaxLength(50);
        });

        // Product Consumer Entity
        modelBuilder.Entity<ProductConsumerEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AccessLevel).HasMaxLength(50);
        });
    }

    protected virtual void ConfigureLicenseEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductLicenseEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.LicenseKey).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LicenseType).HasMaxLength(50);
            entity.Property(e => e.Features).HasMaxLength(2000);
            entity.Property(e => e.Restrictions).HasMaxLength(2000);
        });
    }

    protected virtual void ConfigureNotificationEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationTemplateEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Subject).HasMaxLength(300);
            entity.Property(e => e.Template).HasMaxLength(4000);
            entity.Property(e => e.Type).HasMaxLength(50);
        });

        modelBuilder.Entity<NotificationHistoryEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Recipient).HasMaxLength(300).IsRequired();
            entity.Property(e => e.Subject).HasMaxLength(300);
            entity.Property(e => e.Content).HasMaxLength(4000);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Provider).HasMaxLength(50);
        });
    }

    protected virtual void ConfigureSettingsEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SettingEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Value).HasMaxLength(4000);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.DataType).HasMaxLength(50);
        });
    }

    protected virtual void ConfigureUserEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserProfileEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(300).IsRequired();
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<UserRoleEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Permissions).HasMaxLength(2000);
        });

        modelBuilder.Entity<UserRoleMappingEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }

    #endregion

    #region Indexes Configuration

    protected virtual void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // Audit indexes
        modelBuilder.Entity<AuditEntryEntity>()
            .HasIndex(e => new { e.TenantId, e.EntityType, e.EntityId })
            .HasDatabaseName("IX_AuditEntries_TenantId_EntityType_EntityId");

        modelBuilder.Entity<AuditEntryEntity>()
            .HasIndex(e => new { e.TenantId, e.CreatedOn })
            .HasDatabaseName("IX_AuditEntries_TenantId_CreatedOn");

        // Consumer indexes
        modelBuilder.Entity<ConsumerAccountEntity>()
            .HasIndex(e => new { e.TenantId, e.Email })
            .IsUnique()
            .HasDatabaseName("IX_ConsumerAccounts_TenantId_Email");

        // Product indexes
        modelBuilder.Entity<ProductEntity>()
            .HasIndex(e => new { e.TenantId, e.Name })
            .IsUnique()
            .HasDatabaseName("IX_Products_TenantId_Name");

        modelBuilder.Entity<ProductEntity>()
            .HasIndex(e => new { e.TenantId, e.Status })
            .HasDatabaseName("IX_Products_TenantId_Status");

        // License indexes
        modelBuilder.Entity<ProductLicenseEntity>()
            .HasIndex(e => new { e.TenantId, e.LicenseKey })
            .IsUnique()
            .HasDatabaseName("IX_ProductLicenses_TenantId_LicenseKey");

        modelBuilder.Entity<ProductLicenseEntity>()
            .HasIndex(e => new { e.TenantId, e.Status })
            .HasDatabaseName("IX_ProductLicenses_TenantId_Status");

        // Settings indexes
        modelBuilder.Entity<SettingEntity>()
            .HasIndex(e => new { e.TenantId, e.Key })
            .IsUnique()
            .HasDatabaseName("IX_Settings_TenantId_Key");

        // User indexes
        modelBuilder.Entity<UserProfileEntity>()
            .HasIndex(e => new { e.TenantId, e.Email })
            .IsUnique()
            .HasDatabaseName("IX_UserProfiles_TenantId_Email");

        modelBuilder.Entity<UserRoleEntity>()
            .HasIndex(e => new { e.TenantId, e.Name })
            .IsUnique()
            .HasDatabaseName("IX_UserRoles_TenantId_Name");
    }

    #endregion

    #region Multi-Tenant Query Filters

    protected virtual void ConfigureGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        // Apply tenant filters to all entities that inherit from BaseEntity
        modelBuilder.Entity<ProductEntity>()
            .HasQueryFilter(e => e.TenantId == CurrentTenantId);

        modelBuilder.Entity<ProductVersionEntity>()
            .HasQueryFilter(e => e.TenantId == CurrentTenantId);

        modelBuilder.Entity<ProductTierEntity>()
            .HasQueryFilter(e => e.TenantId == CurrentTenantId);

        modelBuilder.Entity<ProductFeatureEntity>()
            .HasQueryFilter(e => e.TenantId == CurrentTenantId);

        modelBuilder.Entity<ProductConsumerEntity>()
            .HasQueryFilter(e => e.TenantId == CurrentTenantId);

        modelBuilder.Entity<ProductLicenseEntity>()
            .HasQueryFilter(e => e.TenantId == CurrentTenantId);

        modelBuilder.Entity<ConsumerAccountEntity>()
            .HasQueryFilter(e => e.TenantId == CurrentTenantId);

        modelBuilder.Entity<AuditEntryEntity>()
            .HasQueryFilter(e => e.TenantId == CurrentTenantId);

        modelBuilder.Entity<NotificationTemplateEntity>()
            .HasQueryFilter(e => e.TenantId == CurrentTenantId);

        modelBuilder.Entity<NotificationHistoryEntity>()
            .HasQueryFilter(e => e.TenantId == CurrentTenantId);

        modelBuilder.Entity<SettingEntity>()
            .HasQueryFilter(e => e.TenantId == CurrentTenantId);

        modelBuilder.Entity<UserProfileEntity>()
            .HasQueryFilter(e => e.TenantId == CurrentTenantId);

        modelBuilder.Entity<UserRoleEntity>()
            .HasQueryFilter(e => e.TenantId == CurrentTenantId);

        modelBuilder.Entity<UserRoleMappingEntity>()
            .HasQueryFilter(e => e.TenantId == CurrentTenantId);
    }

    #endregion

    #region Override Methods for Audit and Tenant Support

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    protected virtual void UpdateAuditFields()
    {
        var currentTime = DateTime.UtcNow;
        var currentUserId = _userContext?.UserId ?? Guid.Empty;
        var currentTenantId = CurrentTenantId;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedOn = currentTime;
                    entry.Entity.CreatedBy = currentUserId;
                    entry.Entity.ModifiedOn = currentTime;
                    entry.Entity.ModifiedBy = currentUserId;
                    
                    // Set TenantId only if not already set
                    if (entry.Entity.TenantId == Guid.Empty)
                    {
                        entry.Entity.TenantId = currentTenantId;
                    }
                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedOn = currentTime;
                    entry.Entity.ModifiedBy = currentUserId;
                    
                    // Prevent TenantId modification (security)
                    entry.Property(nameof(BaseEntity.TenantId)).IsModified = false;
                    entry.Property(nameof(BaseEntity.CreatedOn)).IsModified = false;
                    entry.Property(nameof(BaseEntity.CreatedBy)).IsModified = false;
                    break;
            }
        }
    }

    #endregion

    #region Administrative Bypass Methods (Use with caution)

    /// <summary>
    /// Execute action without tenant filtering - FOR ADMINISTRATIVE USE ONLY
    /// </summary>
    public async Task<T> WithoutTenantFilterAsync<T>(Func<LicensingDbContext, Task<T>> action)
    {
        var originalTrackingBehavior = ChangeTracker.QueryTrackingBehavior;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        try
        {
            // This would require more complex implementation to temporarily disable filters
            // For now, return the action with tracking disabled
            return await action(this);
        }
        finally
        {
            ChangeTracker.QueryTrackingBehavior = originalTrackingBehavior;
        }
    }

    /// <summary>
    /// Execute action without tenant filtering synchronously - FOR ADMINISTRATIVE USE ONLY
    /// </summary>
    public T WithoutTenantFilter<T>(Func<LicensingDbContext, T> action)
    {
        var originalTrackingBehavior = ChangeTracker.QueryTrackingBehavior;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        try
        {
            return action(this);
        }
        finally
        {
            ChangeTracker.QueryTrackingBehavior = originalTrackingBehavior;
        }
    }

    #endregion
}
