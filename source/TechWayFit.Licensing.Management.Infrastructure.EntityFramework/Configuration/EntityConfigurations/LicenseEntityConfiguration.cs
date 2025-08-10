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
        builder.Property(e => e.Encryption).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Signature).HasMaxLength(50).IsRequired();
        builder.Property(e => e.LicenseKey).HasMaxLength(2000).IsRequired();
        builder.Property(e => e.PublicKey).HasMaxLength(2000);
        builder.Property(e => e.LicenseSignature).HasMaxLength(2000);
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
