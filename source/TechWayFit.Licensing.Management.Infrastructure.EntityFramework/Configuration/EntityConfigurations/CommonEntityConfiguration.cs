using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Notification;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Settings;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Audit;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration.EntityConfigurations;

/// <summary>
/// Entity configuration for Notification-related entities
/// </summary>
public class NotificationTemplateEntityConfiguration : IEntityTypeConfiguration<NotificationTemplateEntity>
{
    public void Configure(EntityTypeBuilder<NotificationTemplateEntity> builder)
    {
        // builder.HasKey(e => e.Id);
        // builder.Property(e => e.Id).IsRequired();
        // builder.Property(e => e.TemplateName).HasMaxLength(100).IsRequired();
        // builder.Property(e => e.TemplateType).HasMaxLength(50).IsRequired();
        // builder.Property(e => e.Subject).HasMaxLength(200).IsRequired();
        // builder.Property(e => e.Body).IsRequired();
        // builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
        // builder.Property(e => e.UpdatedBy).HasMaxLength(100);

        // // Unique constraints
        // builder.HasIndex(e => e.TemplateName).IsUnique();

        // // Indexes
        // builder.HasIndex(e => e.TemplateType);
        // builder.HasIndex(e => e.IsActive);
        // builder.HasIndex(e => e.TenantId);
    }
}

public class NotificationHistoryEntityConfiguration : IEntityTypeConfiguration<NotificationHistoryEntity>
{
    public void Configure(EntityTypeBuilder<NotificationHistoryEntity> builder)
    {
        // builder.HasKey(e => e.Id);
        // builder.Property(e => e.Id).IsRequired();
        // builder.Property(e => e.TemplateId).IsRequired();
        // builder.Property(e => e.RecipientEmail).HasMaxLength(255).IsRequired();
        // builder.Property(e => e.Subject).HasMaxLength(200).IsRequired();
        // builder.Property(e => e.Body).IsRequired();
        // builder.Property(e => e.Status).HasMaxLength(50).IsRequired();
        // builder.Property(e => e.ErrorMessage).HasMaxLength(1000);
        // builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
        // builder.Property(e => e.UpdatedBy).HasMaxLength(100);

        // // Foreign key relationships
        // builder.HasOne(e => e.Template)
        //       .WithMany(t => t.NotificationHistories)
        //       .HasForeignKey(e => e.TemplateId)
        //       .OnDelete(DeleteBehavior.Restrict);

        // // Indexes
        // builder.HasIndex(e => e.TemplateId);
        // builder.HasIndex(e => e.RecipientEmail);
        // builder.HasIndex(e => e.Status);
        // builder.HasIndex(e => e.SentAt);
        // builder.HasIndex(e => e.TenantId);
        // builder.HasIndex(e => new { e.RecipientEmail, e.SentAt });
    }
}

/// <summary>
/// Entity configuration for Settings entities
/// </summary>
public class SettingEntityConfiguration : IEntityTypeConfiguration<SettingEntity>
{
    public void Configure(EntityTypeBuilder<SettingEntity> builder)
    {
        // builder.HasKey(e => e.Id);
        // builder.Property(e => e.Id).IsRequired();
        // builder.Property(e => e.SettingKey).HasMaxLength(100).IsRequired();
        // builder.Property(e => e.SettingValue).HasMaxLength(1000).IsRequired();
        // builder.Property(e => e.SettingCategory).HasMaxLength(50).IsRequired();
        // builder.Property(e => e.Description).HasMaxLength(500);
        // builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
        // builder.Property(e => e.UpdatedBy).HasMaxLength(100);

        // // Unique constraints
        // builder.HasIndex(e => new { e.SettingKey, e.TenantId }).IsUnique();

        // // Indexes
        // builder.HasIndex(e => e.SettingKey);
        // builder.HasIndex(e => e.SettingCategory);
        // builder.HasIndex(e => e.IsActive);
        // builder.HasIndex(e => e.TenantId);
    }
}

/// <summary>
/// Entity configuration for Audit entities
/// </summary>
public class AuditEntryEntityConfiguration : IEntityTypeConfiguration<AuditEntryEntity>
{
    public void Configure(EntityTypeBuilder<AuditEntryEntity> builder)
    {
        // builder.HasKey(e => e.Id);
        // builder.Property(e => e.Id).IsRequired();
        // builder.Property(e => e.EntityName).HasMaxLength(100).IsRequired();
        // builder.Property(e => e.EntityId).IsRequired();
        // builder.Property(e => e.Action).HasMaxLength(50).IsRequired();
        // builder.Property(e => e.Changes).IsRequired();
        // builder.Property(e => e.UserId).HasMaxLength(100).IsRequired();
        // builder.Property(e => e.UserName).HasMaxLength(100).IsRequired();
        // builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
        // builder.Property(e => e.UpdatedBy).HasMaxLength(100);

        // // Indexes
        // builder.HasIndex(e => e.EntityName);
        // builder.HasIndex(e => e.EntityId);
        // builder.HasIndex(e => e.Action);
        // builder.HasIndex(e => e.UserId);
        // builder.HasIndex(e => e.Timestamp);
        // builder.HasIndex(e => e.TenantId);
        // builder.HasIndex(e => new { e.EntityName, e.EntityId, e.Timestamp });
    }
}
