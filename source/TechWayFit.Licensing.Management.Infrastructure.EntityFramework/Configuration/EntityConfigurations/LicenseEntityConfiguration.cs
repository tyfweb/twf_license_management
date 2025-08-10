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
        // builder.HasKey(e => e.Id);
        // builder.Property(e => e.Id).IsRequired();
        // builder.Property(e => e.ProductId).IsRequired();
        // builder.Property(e => e.LicenseKey).HasMaxLength(500).IsRequired();
        // builder.Property(e => e.CustomerName).HasMaxLength(200).IsRequired();
        // builder.Property(e => e.CustomerEmail).HasMaxLength(255).IsRequired();
        // builder.Property(e => e.ProductName).HasMaxLength(100).IsRequired();
        // builder.Property(e => e.ProductVersion).HasMaxLength(50).IsRequired();
        // builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
        // builder.Property(e => e.UpdatedBy).HasMaxLength(100);

        // // Foreign key relationships
        // builder.HasOne(e => e.Product)
        //       .WithMany(p => p.ProductLicenses)
        //       .HasForeignKey(e => e.ProductId)
        //       .OnDelete(DeleteBehavior.Restrict);

        // // Unique constraints
        // builder.HasIndex(e => e.LicenseKey).IsUnique();

        // // Indexes
        // builder.HasIndex(e => e.CustomerName);
        // builder.HasIndex(e => e.CustomerEmail);
        // builder.HasIndex(e => e.ProductId);
        // builder.HasIndex(e => e.IsActive);
        // builder.HasIndex(e => e.TenantId);
        // builder.HasIndex(e => new { e.TenantId, e.IsActive });
    }
}
