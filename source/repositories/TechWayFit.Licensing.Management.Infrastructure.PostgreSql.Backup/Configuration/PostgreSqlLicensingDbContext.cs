using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Infrastructure.Data.Entities.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.Notification;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.Products;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.License;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.Settings;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.User;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.Common;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.Audit;
using TechWayFit.Licensing.Management.Core.Contracts;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;

/// <summary>
/// PostgreSQL-specific Entity Framework DbContext for the licensing management system
/// 
/// MULTI-TENANT IMPLEMENTATION:
/// This DbContext implements multi-tenancy through the following mechanisms:
/// 
/// 1. TENANT ID FILTERING:
///    - All entities inherit from BaseEntity which includes TenantId property
///    - Global query filters automatically filter all queries by current user's TenantId
///    - Prevents cross-tenant data access without explicit bypass
/// 
/// 2. AUTOMATIC TENANT ID ASSIGNMENT:
///    - New entities automatically get TenantId set from current user context
///    - TenantId cannot be modified after entity creation (prevented in UpdateAuditFields)
/// 
/// 3. PERFORMANCE OPTIMIZATION:
///    - TenantId indexes on all entities for optimal query performance
///    - Composite indexes combining TenantId with frequently queried fields
/// 
/// 4. ADMINISTRATIVE BYPASS:
///    - WithoutTenantFilter methods for system-level operations
///    - Use with extreme caution for cross-tenant administrative tasks
/// 
/// SECURITY CONSIDERATIONS:
/// - All queries are automatically filtered by TenantId
/// - No raw SQL should bypass tenant filtering
/// - Admin operations must explicitly use bypass methods
/// - TenantId must be properly set in user context (claims)
/// </summary>
public partial class PostgreSqlPostgreSqlLicensingDbContext : DbContext
{
    private readonly IUserContext _userContext;
    public PostgreSqlPostgreSqlLicensingDbContext(
        DbContextOptions<PostgreSqlPostgreSqlLicensingDbContext> options, IUserContext userContext) : base(options)
    {
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
    }

    #region DbSets

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureAuditEntities(modelBuilder);
        ConfigureConsumerEntities(modelBuilder);

        // Configure entities
        ConfigureProductEntities(modelBuilder);
        ConfigureLicenseEntities(modelBuilder);

        ConfigureNotificationEntities(modelBuilder);
        ConfigureSettingsEntities(modelBuilder);
        ConfigureUserEntities(modelBuilder); 

        // Configure indexes
        ConfigureIndexes(modelBuilder);

        // Configure global query filters for multi-tenancy
        ConfigureGlobalQueryFilters(modelBuilder);
    }


    /// <summary>
    /// Configure Audit related entities
    /// </summary>
    private static void ConfigureAuditEntities(ModelBuilder modelBuilder)
    {
        // AuditEntryEntity configuration
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

            // Audit fields
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Indexes
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => e.ActionType);
        });
    }

    /// <summary>
    /// Configure Consumer related entities
    /// </summary>
    private static void ConfigureConsumerEntities(ModelBuilder modelBuilder)
    {
        // ConsumerAccountEntity configuration
        modelBuilder.Entity<ConsumerAccountEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.CompanyName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.AccountCode).HasMaxLength(50);

            entity.Property(e => e.PrimaryContactName).HasMaxLength(200);
            entity.Property(e => e.PrimaryContactEmail).HasMaxLength(255);
            entity.Property(e => e.PrimaryContactPhone).HasMaxLength(50);
            entity.Property(e => e.PrimaryContactPosition).HasMaxLength(100);

            entity.Property(e => e.SecondaryContactName).HasMaxLength(100);
            entity.Property(e => e.SecondaryContactEmail).HasMaxLength(255);
            entity.Property(e => e.SecondaryContactPhone).HasMaxLength(50);
            entity.Property(e => e.SecondaryContactPosition).HasMaxLength(100);

            entity.Property(e => e.AddressStreet).HasMaxLength(500);
            entity.Property(e => e.AddressCity).HasMaxLength(100);
            entity.Property(e => e.AddressState).HasMaxLength(100);
            entity.Property(e => e.AddressPostalCode).HasMaxLength(20);
            entity.Property(e => e.AddressCountry).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.Status).HasConversion<string>();

            // Audit fields
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Indexes
            entity.HasIndex(e => e.PrimaryContactEmail).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsActive);
        });

        modelBuilder.Entity<ProductConsumerEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.ProductId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ConsumerId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.AccountManagerName).HasMaxLength(100);
            entity.Property(e => e.AccountManagerEmail).HasMaxLength(255);
            entity.Property(e => e.AccountManagerPhone).HasMaxLength(50);
            entity.Property(e => e.AccountManagerPosition).HasMaxLength(100);

            // Audit fields
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Relationships
            entity.HasOne(e => e.Consumer)
                  .WithMany(c => c.ProductConsumers)
                  .HasForeignKey(e => e.ConsumerId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                  .WithMany(p => p.ProductConsumers)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => new { e.ConsumerId, e.ProductId }).IsUnique();
        });
    }


    /// <summary>
    /// Configure Product related entities
    /// </summary>
    private static void ConfigureProductEntities(ModelBuilder modelBuilder)
    {
        // ProductEntity configuration
        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.TenantId).IsRequired(); // Multi-tenant support
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.SupportEmail).HasMaxLength(255);
            entity.Property(e => e.SupportPhone).HasMaxLength(50);

            // Audit fields
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Indexes
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.Status);
        });

        // ProductVersionEntity configuration
        modelBuilder.Entity<ProductVersionEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.ProductId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Version).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ReleaseDate).IsRequired();
            entity.Property(e => e.EndOfLifeDate);
            entity.Property(e => e.SupportEndDate);
            entity.Property(e => e.ReleaseNotes).HasMaxLength(2000);
            entity.Property(e => e.IsCurrent).IsRequired();

            // Audit fields
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Relationships
            entity.HasOne(e => e.Product)
                  .WithMany(p => p.Versions)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => new { e.ProductId, e.Version }).IsUnique();
            entity.HasIndex(e => e.IsCurrent);
        });

        // ProductTierEntity configuration
        modelBuilder.Entity<ProductTierEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.ProductId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Price).HasMaxLength(10); // Assuming price is a string for currency formatting
            entity.Property(e => e.DisplayOrder);
            entity.Property(e => e.SupportSLAJson).HasMaxLength(1000);

            // Audit fields
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Relationships
            entity.HasOne(e => e.Product)
                  .WithMany(p => p.Tiers)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => new { e.ProductId, e.Name }).IsUnique();
            entity.HasIndex(e => e.IsActive);
        });

        // ProductFeatureEntity configuration
        modelBuilder.Entity<ProductFeatureEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.TierId).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Code).HasMaxLength(100).IsRequired();
            entity.Property(e => e.IsEnabled).IsRequired();
            entity.Property(e => e.DisplayOrder);
            entity.Property(e => e.SupportFromVersion).HasMaxLength(20);
            entity.Property(e => e.SupportToVersion).HasMaxLength(20);
            entity.Property(e => e.FeatureUsageJson).HasMaxLength(1000);

            // Audit fields
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Relationships
            entity.HasOne(e => e.Tier)
                  .WithMany(t => t.Features)
                  .HasForeignKey(e => e.TierId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => new { e.TierId, e.Code }).IsUnique();
            entity.HasIndex(e => e.IsEnabled);
            entity.HasIndex(e => e.Code);
        });
    }

    /// <summary>
    /// Configure License related entities
    /// </summary>
    private static void ConfigureLicenseEntities(ModelBuilder modelBuilder)
    {
        // ProductLicenseEntity configuration
        modelBuilder.Entity<ProductLicenseEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.ConsumerId).IsRequired();
            entity.Property(e => e.Encryption).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Signature).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LicenseKey).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.PublicKey).HasMaxLength(2000);
            entity.Property(e => e.LicenseSignature).HasMaxLength(2000);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.IssuedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.RevocationReason).HasMaxLength(500);
            entity.Property(e => e.MetadataJson).HasMaxLength(2000);

            // Audit fields
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Relationships
            entity.HasOne(e => e.Product)
                  .WithMany(p => p.Licenses)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Consumer)
                  .WithMany(c => c.Licenses)
                  .HasForeignKey(e => e.ConsumerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Features)
                  .WithMany(t => t.ProductLicenses);

            // Indexes
            entity.HasIndex(e => e.LicenseKey).IsUnique();
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.ConsumerId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ValidTo);
            entity.HasIndex(e => new { e.ProductId, e.ConsumerId });
        });
    }





    /// <summary>
    /// Configure Notification related entities
    /// </summary>
    private static void ConfigureNotificationEntities(ModelBuilder modelBuilder)
    {
        // NotificationTemplateEntity configuration
        modelBuilder.Entity<NotificationTemplateEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TemplateName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.MessageTemplate).HasMaxLength(4000).IsRequired();
            entity.Property(e => e.NotificationType).HasMaxLength(20);
            entity.Property(e => e.Subject).HasMaxLength(500);
            entity.Property(e => e.TemplateVariableJson).HasMaxLength(1000);
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.NotificationMode).HasConversion<string>();
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Audit fields
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Indexes
            entity.HasIndex(e => e.TemplateName);
            entity.HasIndex(e => e.IsActive);
        });

        // NotificationHistoryEntity configuration
        modelBuilder.Entity<NotificationHistoryEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.EntityId);
            entity.Property(e => e.EntityType).HasConversion<string>();
            entity.Property(e => e.RecipientsJson).HasMaxLength(1000);
            entity.Property(e => e.NotificationMode).HasMaxLength(20);
            entity.Property(e => e.NotificationTemplateId).HasMaxLength(50);
            entity.Property(e => e.NotificationType).HasMaxLength(20);
            entity.Property(e => e.SentDate).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.DeliveryError).HasMaxLength(2000);

            // Audit fields
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedOn).IsRequired();


            // Relationships
            entity.HasOne(e => e.Template)
                  .WithMany(t => t.NotificationHistory)
                  .HasForeignKey(e => e.NotificationTemplateId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.NotificationTemplateId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.NotificationType);
            entity.HasIndex(e => e.SentDate);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
        });
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

        // Consumer related entities
        modelBuilder.Entity<ConsumerAccountEntity>().HasIndex(e => e.TenantId);

        // Audit related entities
        modelBuilder.Entity<AuditEntryEntity>().HasIndex(e => e.TenantId);

        // Notification related entities
        modelBuilder.Entity<NotificationTemplateEntity>().HasIndex(e => e.TenantId);
        modelBuilder.Entity<NotificationHistoryEntity>().HasIndex(e => e.TenantId);

        // Settings related entities
        modelBuilder.Entity<SettingEntity>().HasIndex(e => e.TenantId);

        // User related entities
        modelBuilder.Entity<UserProfileEntity>().HasIndex(e => e.TenantId);
        modelBuilder.Entity<UserRoleEntity>().HasIndex(e => e.TenantId);
        modelBuilder.Entity<UserRoleMappingEntity>().HasIndex(e => e.TenantId);

        // Composite indexes for frequently queried combinations
        modelBuilder.Entity<ProductEntity>().HasIndex(e => new { e.TenantId, e.IsActive });
        modelBuilder.Entity<ConsumerAccountEntity>().HasIndex(e => new { e.TenantId, e.IsActive });
        modelBuilder.Entity<ProductLicenseEntity>().HasIndex(e => new { e.TenantId, e.Status });
        modelBuilder.Entity<UserProfileEntity>().HasIndex(e => new { e.TenantId, e.IsActive, e.IsLocked });
    }

    /// <summary>
    /// Configure Settings related entities
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    private static void ConfigureSettingsEntities(ModelBuilder modelBuilder)
    {
        // Configure SettingEntity
        modelBuilder.Entity<SettingEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).IsRequired();

            entity.Property(e => e.Category).HasMaxLength(100).IsRequired();

            entity.Property(e => e.Key).HasMaxLength(100).IsRequired();

            entity.Property(e => e.Value).HasMaxLength(4000);

            entity.Property(e => e.DefaultValue).HasMaxLength(4000);

            entity.Property(e => e.DataType).HasMaxLength(50).IsRequired();

            entity.Property(e => e.DisplayName).HasMaxLength(200).IsRequired();

            entity.Property(e => e.Description).HasMaxLength(1000);

            entity.Property(e => e.GroupName).HasMaxLength(100);

            entity.Property(e => e.ValidationRules).HasMaxLength(2000);

            entity.Property(e => e.PossibleValues).HasMaxLength(2000);

            // Unique constraint on Category + Key combination
            entity.HasIndex(e => new { e.Category, e.Key })
                .IsUnique();

            // Performance indexes
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => new { e.Category, e.DisplayOrder });

            // Computed column for FullKey (ignored since it's a computed property)
            entity.Ignore(e => e.FullKey);
        });
    }

    /// <summary>
    /// Override SaveChanges to update audit fields and convert DateTime values to UTC automatically
    /// </summary>
    public override int SaveChanges()
    {
        UpdateAuditFields();
        CreateAuditEntries();
        ConvertDateTimesToUtc();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to update audit fields and convert DateTime values to UTC automatically
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        CreateAuditEntries();
        ConvertDateTimesToUtc();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Update audit fields before saving
    /// </summary>
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        var currentTime = DateTime.UtcNow;
        var currentTenantId = GetCurrentTenantId();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.Id = Guid.NewGuid();// Ensure Id is set for new entities
                    entry.Entity.CreatedOn = currentTime;
                    entry.Entity.TenantId = currentTenantId; // Set tenant ID for new entities
                    if(entry.Entity.CreatedBy == null)
                        entry.Entity.CreatedBy = _userContext.UserName ?? "Anonymous"; // Default to Anonymous if not set
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedOn = currentTime;
                    if (entry.Entity.UpdatedBy == null)
                        entry.Entity.UpdatedBy = _userContext.UserName ?? "Anonymous"; // Default to Anonymous if not set
                    entry.Property(x => x.CreatedBy).IsModified = false;
                    entry.Property(x => x.CreatedOn).IsModified = false;
                    entry.Property(x => x.TenantId).IsModified = false; // Prevent tenant ID changes on updates
                    break;
            }
        }
    }

    /// <summary>
    /// Get the current tenant ID from the user context
    /// </summary>
    private Guid GetCurrentTenantId()
    {
        var tenantId = _userContext.TenantId;
        if (tenantId == Guid.Empty || tenantId == null)
        {
            // Log warning or handle accordingly
            // In production, consider throwing an exception or logging this as an error
            tenantId = Guid.Empty; // Default to empty Guid if no tenant ID is available
        }  
        // Consider throwing an exception in production environments
        return tenantId.Value;
    }

    /// <summary>
    /// Bypasses the global query filters for administrative operations
    /// Use with caution - only for system-level operations that need cross-tenant access
    /// </summary>
    /// <returns>DbContext with global filters ignored</returns>
    public DbContext IgnoreQueryFilters()
    {
        return this.IgnoreQueryFilters();
    }

    /// <summary>
    /// Execute a query with tenant filtering temporarily disabled
    /// Use for administrative operations that need cross-tenant access
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <returns>Result of the operation</returns>
    public T WithoutTenantFilter<T>(Func<T> operation)
    {
        using var scope = new TenantFilterScope();
        return operation();
    }

    /// <summary>
    /// Execute an async query with tenant filtering temporarily disabled
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="operation">Async operation to execute</param>
    /// <returns>Result of the operation</returns>
    public async Task<T> WithoutTenantFilterAsync<T>(Func<Task<T>> operation)
    {
        using var scope = new TenantFilterScope();
        return await operation();
    }

    /// <summary>
    /// Create audit entries for all entity changes (add/update)
    /// </summary>
    private void CreateAuditEntries()
    {
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();

        foreach (var entry in entries)
        {
            // Skip creating audit entries for AuditEntryEntity itself to avoid recursion
            if (entry.Entity is AuditEntryEntity)
                continue;

            var auditEntry = CreateAuditEntry(entry);
            if (auditEntry != null)
            {
                AuditEntries.Add(auditEntry);
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
    /// Configure User related entities
    /// </summary>
    private static void ConfigureUserEntities(ModelBuilder modelBuilder)
    {
        // UserProfileEntity configuration
        modelBuilder.Entity<UserProfileEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.UserName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(256).IsRequired();
            entity.Property(e => e.PasswordSalt).HasMaxLength(128).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Unique constraints
            entity.HasIndex(e => e.UserName).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();

            // Indexes
            entity.HasIndex(e => e.FullName);
            entity.HasIndex(e => e.Department);
            entity.HasIndex(e => e.IsLocked);
            entity.HasIndex(e => e.IsDeleted);
            entity.HasIndex(e => e.IsAdmin);
        });

        // UserRoleEntity configuration
        modelBuilder.Entity<UserRoleEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.RoleName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.RoleDescription).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Unique constraint
            entity.HasIndex(e => e.RoleName).IsUnique();

            // Indexes
            entity.HasIndex(e => e.IsAdmin);
        });

        // UserRoleMappingEntity configuration
        modelBuilder.Entity<UserRoleMappingEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.RoleId).IsRequired();
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Foreign key relationships
            entity.HasOne(e => e.User)
                  .WithMany(u => u.UserRoles)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                  .WithMany(r => r.UserRoles)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint - user can only have one active mapping to a specific role
            entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();

            // Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.RoleId);
            entity.HasIndex(e => e.AssignedDate);
            entity.HasIndex(e => e.ExpiryDate);
        });
    }

    /// <summary>
    /// Configure global query filters for multi-tenancy
    /// </summary>
    private void ConfigureGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        // Apply global query filter to all entities that inherit from BaseEntity
        // This ensures all queries automatically filter by TenantId
        // Note: The TenantId is evaluated at query execution time, not at model creation time
        
        // Product related entities
        modelBuilder.Entity<ProductEntity>().HasQueryFilter(e => e.TenantId == GetCurrentTenantId());
        modelBuilder.Entity<ProductVersionEntity>().HasQueryFilter(e => e.TenantId == GetCurrentTenantId());
        modelBuilder.Entity<ProductTierEntity>().HasQueryFilter(e => e.TenantId == GetCurrentTenantId());
        modelBuilder.Entity<ProductFeatureEntity>().HasQueryFilter(e => e.TenantId == GetCurrentTenantId());
        modelBuilder.Entity<ProductConsumerEntity>().HasQueryFilter(e => e.TenantId == GetCurrentTenantId());

        // License related entities
        modelBuilder.Entity<ProductLicenseEntity>().HasQueryFilter(e => e.TenantId == GetCurrentTenantId());

        // Consumer related entities
        modelBuilder.Entity<ConsumerAccountEntity>().HasQueryFilter(e => e.TenantId == GetCurrentTenantId());

        // Audit related entities
        modelBuilder.Entity<AuditEntryEntity>().HasQueryFilter(e => e.TenantId == GetCurrentTenantId());

        // Notification related entities
        modelBuilder.Entity<NotificationTemplateEntity>().HasQueryFilter(e => e.TenantId == GetCurrentTenantId());
        modelBuilder.Entity<NotificationHistoryEntity>().HasQueryFilter(e => e.TenantId == GetCurrentTenantId());

        // Settings related entities
        modelBuilder.Entity<SettingEntity>().HasQueryFilter(e => e.TenantId == GetCurrentTenantId());

        // User related entities
        modelBuilder.Entity<UserProfileEntity>().HasQueryFilter(e => e.TenantId == GetCurrentTenantId());
        modelBuilder.Entity<UserRoleEntity>().HasQueryFilter(e => e.TenantId == GetCurrentTenantId());
        modelBuilder.Entity<UserRoleMappingEntity>().HasQueryFilter(e => e.TenantId == GetCurrentTenantId());
    }

   
}
