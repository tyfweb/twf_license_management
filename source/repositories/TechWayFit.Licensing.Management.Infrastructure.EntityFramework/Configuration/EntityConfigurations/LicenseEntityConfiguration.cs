using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.License;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration.EntityConfigurations;

/// <summary>
/// Entity configuration for License-related entities
/// </summary>
public class ProductLicenseEntityConfiguration : IEntityTypeConfiguration<ProductLicenseEntity>
{
    public void Configure(EntityTypeBuilder<ProductLicenseEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.ProductId).IsRequired();
        builder.Property(e => e.ConsumerId).IsRequired();
        builder.Property(e => e.LicenseKey).HasMaxLength(2000).IsRequired();
        builder.Property(e => e.PublicKey).HasMaxLength(2000);
        builder.Property(e => e.Status).HasConversion<string>();
        builder.Property(e => e.IssuedBy).HasMaxLength(100).IsRequired();
        builder.Property(e => e.RevocationReason).HasMaxLength(500);
        builder.Property(e => e.MetadataJson).HasMaxLength(2000);

        // Audit fields
        builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);

        // Relationships
        builder.HasOne(e => e.Product)
              .WithMany(p => p.Licenses)
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Consumer)
              .WithMany(c => c.Licenses)
              .HasForeignKey(e => e.ConsumerId)
              .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => e.LicenseKey).IsUnique();
        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => e.ConsumerId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.ValidTo);
        builder.HasIndex(e => new { e.ProductId, e.ConsumerId });
    }
}

/// <summary>
/// Entity configuration for ProductActivation entity
/// </summary>
public class ProductActivationEntityConfiguration : IEntityTypeConfiguration<ProductActivationEntity>
{
    public void Configure(EntityTypeBuilder<ProductActivationEntity> builder)
    {
        // Primary key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).IsRequired();

        // Table name
        builder.ToTable("product_activations");

        // License Configuration (Immutable)
        builder.Property(e => e.LicenseId).IsRequired();
        builder.Property(e => e.FormattedProductKey).HasMaxLength(19).IsRequired();
        builder.Property(e => e.MaxActivations).IsRequired();

        // Activation Tracking (Mutable)
        builder.Property(e => e.ProductKey).HasMaxLength(19).IsRequired();
        builder.Property(e => e.MachineId).HasMaxLength(255).IsRequired();
        builder.Property(e => e.MachineName).HasMaxLength(255);
        builder.Property(e => e.MachineFingerprint).HasMaxLength(500);
        builder.Property(e => e.IpAddress).HasMaxLength(45);
        builder.Property(e => e.ActivationDate).IsRequired();
        builder.Property(e => e.ActivationEndDate);
        builder.Property(e => e.ActivationSignature).HasMaxLength(500);
        builder.Property(e => e.LastHeartbeat);
        builder.Property(e => e.Status).HasConversion<string>().IsRequired();
        builder.Property(e => e.ActivationData).IsRequired().HasDefaultValue("{}");
        builder.Property(e => e.DeactivationDate);
        builder.Property(e => e.DeactivationReason).HasMaxLength(500);
        builder.Property(e => e.DeactivatedBy).HasMaxLength(100);

        // Audit fields (inherited from AuditWorkflowEntity)
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);

        // Relationships
        builder.HasOne(e => e.License)
              .WithMany()  // ProductLicenseEntity doesn't have an Activations collection yet
              .HasForeignKey(e => e.LicenseId)
              .OnDelete(DeleteBehavior.Restrict);

        // Indexes for performance
        builder.HasIndex(e => e.LicenseId);
        builder.HasIndex(e => e.ProductKey);
        builder.HasIndex(e => e.MachineId);
        builder.HasIndex(e => e.ActivationSignature).IsUnique();
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.ActivationDate);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => new { e.ProductKey, e.MachineId });
        builder.HasIndex(e => new { e.LicenseId, e.Status });
        builder.HasIndex(e => new { e.TenantId, e.IsActive });
    }
}
