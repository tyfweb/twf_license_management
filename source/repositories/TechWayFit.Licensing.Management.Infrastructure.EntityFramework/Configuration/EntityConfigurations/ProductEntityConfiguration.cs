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
        builder.Property(e => e.DisplayOrder);
        builder.Property(e => e.SupportSLAJson).HasMaxLength(1000);
        builder.Property(e => e.MaxUsers);
        builder.Property(e => e.MaxDevices);

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
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.Code).HasMaxLength(100).IsRequired();
        builder.Property(e => e.IsEnabled).IsRequired();
        builder.Property(e => e.DisplayOrder);
        
        // Foreign key properties for version support
        builder.Property(e => e.SupportFromVersionId);
        builder.Property(e => e.SupportToVersionId);
        
        builder.Property(e => e.FeatureUsageJson).HasMaxLength(1000);

        // Audit fields
        builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);

        // Version support relationships
        builder.HasOne(e => e.SupportFromVersion)
              .WithMany()
              .HasForeignKey(e => e.SupportFromVersionId)
              .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.SupportToVersion)
              .WithMany()
              .HasForeignKey(e => e.SupportToVersionId)
              .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(e => e.IsEnabled);
        builder.HasIndex(e => e.Code);
        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => e.SupportFromVersionId);
        builder.HasIndex(e => e.SupportToVersionId);
    }
}

public class ProductFeatureTierMappingEntityConfiguration : IEntityTypeConfiguration<ProductFeatureTierMappingEntity>
{
    public void Configure(EntityTypeBuilder<ProductFeatureTierMappingEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.ProductFeatureId).IsRequired();
        builder.Property(e => e.ProductTierId).IsRequired();
        builder.Property(e => e.IsEnabled).IsRequired();
        builder.Property(e => e.DisplayOrder);
        builder.Property(e => e.Configuration).HasMaxLength(2000);

        // Audit fields
        builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);

        // Relationships
        builder.HasOne(e => e.ProductFeature)
              .WithMany(f => f.TierMappings)
              .HasForeignKey(e => e.ProductFeatureId)
              .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ProductTier)
              .WithMany(t => t.FeatureMappings)
              .HasForeignKey(e => e.ProductTierId)
              .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => new { e.ProductFeatureId, e.ProductTierId }).IsUnique();
        builder.HasIndex(e => e.IsEnabled);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.TenantId);
    }
}

public class ProductTierPriceEntityConfiguration : IEntityTypeConfiguration<ProductTierPriceEntity>
{
    public void Configure(EntityTypeBuilder<ProductTierPriceEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.ProductId).IsRequired();
        builder.Property(e => e.TierId).IsRequired();
        builder.Property(e => e.PriceAmount).HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(e => e.Currency).HasMaxLength(3).IsRequired();
        builder.Property(e => e.PriceType).IsRequired();

        // Audit fields
        builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);

        // Relationships
        builder.HasOne(e => e.ProductTier)
              .WithMany(t => t.Prices)
              .HasForeignKey(e => e.TierId)
              .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Product)
              .WithMany()
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => new { e.TierId, e.PriceType }).IsUnique();
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.Currency);
    }
}
