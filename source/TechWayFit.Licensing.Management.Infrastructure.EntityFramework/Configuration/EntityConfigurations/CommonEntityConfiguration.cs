using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Notification;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Settings;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Audit;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Seeding;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Tenants;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration.EntityConfigurations;

/// <summary>
/// Entity configuration for Notification-related entities
/// </summary>
public class NotificationTemplateEntityConfiguration : IEntityTypeConfiguration<NotificationTemplateEntity>
{
    public void Configure(EntityTypeBuilder<NotificationTemplateEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasMaxLength(50).IsRequired();
        builder.Property(e => e.TemplateName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.MessageTemplate).HasMaxLength(4000).IsRequired();
        builder.Property(e => e.NotificationType).HasMaxLength(20);
        builder.Property(e => e.Subject).HasMaxLength(500);
        builder.Property(e => e.TemplateVariableJson).HasMaxLength(1000);
        builder.Property(e => e.IsActive).IsRequired();
        builder.Property(e => e.NotificationMode).HasConversion<string>();
        builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);

        // Audit fields
        builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);

        // Indexes
        builder.HasIndex(e => e.TemplateName);
        builder.HasIndex(e => e.IsActive);
    }
}

public class NotificationHistoryEntityConfiguration : IEntityTypeConfiguration<NotificationHistoryEntity>
{
    public void Configure(EntityTypeBuilder<NotificationHistoryEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.EntityId);
        builder.Property(e => e.EntityType).HasConversion<string>();
        builder.Property(e => e.RecipientsJson).HasMaxLength(1000);
        builder.Property(e => e.NotificationMode).HasMaxLength(20);
        builder.Property(e => e.NotificationTemplateId).HasMaxLength(50);
        builder.Property(e => e.NotificationType).HasMaxLength(20);
        builder.Property(e => e.SentDate).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>();
        builder.Property(e => e.DeliveryError).HasMaxLength(2000);

        // Audit fields
        builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);
        builder.Property(e => e.CreatedOn).IsRequired();


        // Relationships
        builder.HasOne(e => e.Template)
                .WithMany(t => t.NotificationHistory)
                .HasForeignKey(e => e.NotificationTemplateId)
                .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(e => e.NotificationTemplateId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.NotificationType);
        builder.HasIndex(e => e.SentDate);
        builder.HasIndex(e => new { e.EntityType, e.EntityId });
    }
}

/// <summary>
/// Entity configuration for Settings entities
/// </summary>
public class SettingEntityConfiguration : IEntityTypeConfiguration<SettingEntity>
{
    public void Configure(EntityTypeBuilder<SettingEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.Category).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Key).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Value).HasMaxLength(4000);
        builder.Property(e => e.DefaultValue).HasMaxLength(4000);
        builder.Property(e => e.DataType).HasMaxLength(50).IsRequired();
        builder.Property(e => e.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.GroupName).HasMaxLength(100);
        builder.Property(e => e.ValidationRules).HasMaxLength(2000);
        builder.Property(e => e.PossibleValues).HasMaxLength(2000);
        // Unique constraint on Category + Key combination
        builder.HasIndex(e => new { e.Category, e.Key })
            .IsUnique();

        // Performance indexes
        builder.HasIndex(e => e.Category);
        builder.HasIndex(e => new { e.Category, e.DisplayOrder });

        // Computed column for FullKey (ignored since it's a computed property)
        builder.Ignore(e => e.FullKey);
    }
}

/// <summary>
/// Entity configuration for Audit entities
/// </summary>

public class AuditEntryEntityConfiguration : IEntityTypeConfiguration<AuditEntryEntity>
{
    public void Configure(EntityTypeBuilder<AuditEntryEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(e => e.EntityId).HasMaxLength(50).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Reason).HasMaxLength(1000);
        builder.Property(e => e.OldValue).HasMaxLength(4000);
        builder.Property(e => e.NewValue).HasMaxLength(4000);
        builder.Property(e => e.Metadata).HasMaxLength(1000);
        builder.Property(e => e.IpAddress).HasMaxLength(45);
        builder.Property(e => e.UserAgent).HasMaxLength(500);
        builder.Property(e => e.ActionType).HasMaxLength(20);

        // Audit fields
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);

        // Indexes
        builder.HasIndex(e => new { e.EntityType, e.EntityId });
        builder.HasIndex(e => e.ActionType);
    }
}

/// <summary>
/// Entity configuration for Seeding History entities
/// </summary>
public class SeedingHistoryEntityConfiguration : IEntityTypeConfiguration<SeedingHistoryEntity>
{
    public void Configure(EntityTypeBuilder<SeedingHistoryEntity> builder)
    {
        // Primary key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).IsRequired();

        // Table name
        builder.ToTable("seeding_history");

        // Required properties
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.SeederName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Version).HasMaxLength(50).IsRequired();
        builder.Property(e => e.ExecutedOn).IsRequired();
        builder.Property(e => e.ExecutedBy).HasMaxLength(200).IsRequired();
        builder.Property(e => e.IsSuccessful).IsRequired();

        // Optional properties
        builder.Property(e => e.ErrorMessage).HasMaxLength(2000);
        builder.Property(e => e.MetadataJson).HasMaxLength(4000).IsRequired();
        builder.Property(e => e.RecordsCreated).HasDefaultValue(0);
        builder.Property(e => e.DurationMs).HasDefaultValue(0);

        // Audit fields from BaseEntity
        builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(e => e.CreatedOn).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);
        builder.Property(e => e.UpdatedOn);
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);

        // Indexes for performance
        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => e.SeederName);
        builder.HasIndex(e => new { e.TenantId, e.SeederName });
        builder.HasIndex(e => e.ExecutedOn);
        builder.HasIndex(e => e.IsSuccessful);
    }
}


public class TenantEntityConfiguration : IEntityTypeConfiguration<TenantEntity>
{
    public void Configure(EntityTypeBuilder<TenantEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.TenantCode).HasMaxLength(50);
        builder.Property(e => e.Description).HasMaxLength(1000);

        // Audit fields
        builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);

        // Indexes
        builder.HasIndex(e => e.TenantCode).IsUnique();
    }
}