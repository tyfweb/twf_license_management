using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Products;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration.EntityConfigurations;

/// <summary>
/// Entity configuration for Product-related entities
/// </summary>
public class ProductEntityConfiguration : IEntityTypeConfiguration<ProductEntity>
{
    public void Configure(EntityTypeBuilder<ProductEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TenantId).IsRequired(); // Multi-tenant support
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.SupportEmail).HasMaxLength(255);
        builder.Property(e => e.SupportPhone).HasMaxLength(50);

        // Audit fields
        builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);

        // Indexes
        builder.HasIndex(e => e.Name).IsUnique();
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => new { e.TenantId, e.IsActive });
    }
}

public class ProductVersionEntityConfiguration : IEntityTypeConfiguration<ProductVersionEntity>
{
    public void Configure(EntityTypeBuilder<ProductVersionEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.ProductId).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Version).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.ReleaseDate).IsRequired();
        builder.Property(e => e.EndOfLifeDate);
        builder.Property(e => e.SupportEndDate);
        builder.Property(e => e.ReleaseNotes).HasMaxLength(2000);
        builder.Property(e => e.IsCurrent).IsRequired();

        // Audit fields
        builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);

        // Relationships
        builder.HasOne(e => e.Product)
              .WithMany(p => p.Versions)
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => new { e.ProductId, e.Version }).IsUnique();
        builder.HasIndex(e => e.IsCurrent);
        builder.HasIndex(e => e.TenantId);
    }
}

public class ProductTierEntityConfiguration : IEntityTypeConfiguration<ProductTierEntity>
{
    public void Configure(EntityTypeBuilder<ProductTierEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.ProductId).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.Price).HasMaxLength(10);
        builder.Property(e => e.DisplayOrder);
        builder.Property(e => e.SupportSLAJson).HasMaxLength(1000);

        // Audit fields
        builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);

        // Relationships
        builder.HasOne(e => e.Product)
              .WithMany(p => p.Tiers)
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => new { e.ProductId, e.Name }).IsUnique();
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.TenantId);
    }
}

public class ProductFeatureEntityConfiguration : IEntityTypeConfiguration<ProductFeatureEntity>
{
    public void Configure(EntityTypeBuilder<ProductFeatureEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.ProductId).IsRequired();
        builder.Property(e => e.TierId).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.Code).HasMaxLength(100).IsRequired();
        builder.Property(e => e.IsEnabled).IsRequired();
        builder.Property(e => e.DisplayOrder);
        builder.Property(e => e.SupportFromVersion).HasMaxLength(20);
        builder.Property(e => e.SupportToVersion).HasMaxLength(20);
        builder.Property(e => e.FeatureUsageJson).HasMaxLength(1000);

        // Audit fields
        builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);

        // Relationships
        builder.HasOne(e => e.Tier)
              .WithMany(t => t.Features)
              .HasForeignKey(e => e.TierId)
              .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => new { e.TierId, e.Code }).IsUnique();
        builder.HasIndex(e => e.IsEnabled);
        builder.HasIndex(e => e.Code);
        builder.HasIndex(e => e.TenantId);
    }
}
