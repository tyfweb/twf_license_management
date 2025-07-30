using Microsoft.EntityFrameworkCore; 
using TechWayFit.Licensing.Infrastructure.Data.Entities.Consumer;
using TechWayFit.Licensing.Infrastructure.Models.Entities.Audit;
using TechWayFit.Licensing.Infrastructure.Models.Entities;
using TechWayFit.Licensing.Infrastructure.Models.Entities.Notification;
using TechWayFit.Licensing.Infrastructure.Models.Entities.Products;
using TechWayFit.Licensing.Infrastructure.Models.Entities.License;
using TechWayFit.Licensing.Infrastructure.Models.Entities.Consumer;
using TechWayFit.Licensing.Infrastructure.Models.Entities.Settings;
using TechWayFit.Licensing.Infrastructure.Models.Entities.User;

namespace TechWayFit.Licensing.Infrastructure.Data.Context;

/// <summary>
/// Entity Framework DbContext for the licensing management system
/// </summary>
public class LicensingDbContext : DbContext
{
    public LicensingDbContext(DbContextOptions<LicensingDbContext> options) : base(options)
    {
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
    }

     
    /// <summary>
    /// Configure Audit related entities
    /// </summary>
    private static void ConfigureAuditEntities(ModelBuilder modelBuilder)
    {
        // AuditEntryEntity configuration
        modelBuilder.Entity<AuditEntryEntity>(entity =>
        {
            entity.HasKey(e => e.AuditEntryId);
            entity.Property(e => e.AuditEntryId).HasMaxLength(50).IsRequired();
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
            entity.HasKey(e => e.ConsumerId);
            entity.Property(e => e.ConsumerId).HasMaxLength(50).IsRequired();
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
            entity.HasKey(e => e.ProductConsumerId);
            entity.Property(e => e.ProductConsumerId).HasMaxLength(50).IsRequired();
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
            entity.HasKey(e => e.ProductId);
            entity.Property(e => e.ProductId).HasMaxLength(50).IsRequired();
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
            entity.HasKey(e => e.VersionId);
            entity.Property(e => e.VersionId).HasMaxLength(50).IsRequired();
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
            entity.HasKey(e => e.TierId);
            entity.Property(e => e.TierId).HasMaxLength(50).IsRequired();
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
            entity.HasKey(e => e.FeatureId);
            entity.Property(e => e.FeatureId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ProductId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TierId).HasMaxLength(50).IsRequired();
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
            entity.HasKey(e => e.LicenseId);
            entity.Property(e => e.LicenseId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ProductId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ConsumerId).HasMaxLength(50).IsRequired(); 
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
                  .WithMany(t => t.ProductLicenses) ;

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
            entity.HasKey(e => e.NotificationTemplateId);
            entity.Property(e => e.NotificationTemplateId).HasMaxLength(50).IsRequired();
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
            entity.HasKey(e => e.NotificationId);
            entity.Property(e => e.NotificationId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.EntityId).HasMaxLength(50);
            entity.Property(e => e.EntityType).HasConversion<string>();
            entity.Property(e => e.RecipientsJson).HasMaxLength(1000);
            entity.Property(e => e.NotificationMode).HasMaxLength(20);
            entity.Property(e => e.NotificationTemplateId).HasMaxLength(50);
            entity.Property(e => e.NotificationType).HasMaxLength(20);
            entity.Property(e => e.SentDate).IsRequired(); 
            entity.Property(e => e.DeliveryStatus).HasConversion<string>();
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
            entity.HasIndex(e => e.DeliveryStatus);
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
            entity.HasKey(e => e.SettingId);

            entity.Property(e => e.SettingId)
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Key)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Value)
                .HasMaxLength(4000);

            entity.Property(e => e.DefaultValue)
                .HasMaxLength(4000);

            entity.Property(e => e.DataType)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.DisplayName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.Property(e => e.GroupName)
                .HasMaxLength(100);

            entity.Property(e => e.ValidationRules)
                .HasMaxLength(2000);

            entity.Property(e => e.PossibleValues)
                .HasMaxLength(2000);

            entity.Property(e => e.ValueSource)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Tags)
                .HasMaxLength(500);

            entity.Property(e => e.Environment)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.IntroducedInVersion)
                .HasMaxLength(20);

            entity.Property(e => e.DeprecationMessage)
                .HasMaxLength(500);

            // Unique constraint on Category + Key combination
            entity.HasIndex(e => new { e.Category, e.Key })
                .IsUnique();

            // Performance indexes
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Environment);
            entity.HasIndex(e => new { e.Category, e.DisplayOrder });

            // Computed column for FullKey (ignored since it's a computed property)
            entity.Ignore(e => e.FullKey);
        });
    }

    /// <summary>
    /// Override SaveChanges to update audit fields automatically
    /// </summary>
    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to update audit fields automatically
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Update audit fields before saving
    /// </summary>
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseAuditEntity>();
        var currentTime = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedOn = currentTime;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedOn = currentTime;
                    entry.Property(x => x.CreatedBy).IsModified = false;
                    entry.Property(x => x.CreatedOn).IsModified = false;
                    break;
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
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).IsRequired();
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
            entity.HasKey(e => e.RoleId);
            entity.Property(e => e.RoleId).IsRequired();
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
            entity.HasKey(e => e.MappingId);
            entity.Property(e => e.MappingId).IsRequired();
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
}
