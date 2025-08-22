using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Products;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration.EntityConfigurations;

/// <summary>
/// Entity configuration for ProductKeys entity
/// </summary>
public class ProductKeysEntityConfiguration : IEntityTypeConfiguration<ProductKeysEntity>
{
    public void Configure(EntityTypeBuilder<ProductKeysEntity> builder)
    {
        // Primary key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).IsRequired();

        // Foreign key
        builder.Property(e => e.ProductId).IsRequired();

        // Key properties
        builder.Property(e => e.PrivateKeyEncrypted)
               .HasMaxLength(8000)
               .IsRequired();

        builder.Property(e => e.PublicKey)
               .HasMaxLength(4000)
               .IsRequired();

        builder.Property(e => e.KeySize)
               .IsRequired()
               .HasDefaultValue(2048);

        builder.Property(e => e.Algorithm)
               .HasMaxLength(50)
               .IsRequired()
               .HasDefaultValue("RSA");

        builder.Property(e => e.KeyGeneratedAt)
               .IsRequired();

        builder.Property(e => e.ExpiresAt);

        builder.Property(e => e.KeyVersion)
               .IsRequired()
               .HasDefaultValue(1);

        builder.Property(e => e.IsActive)
               .IsRequired()
               .HasDefaultValue(true);

        // Audit fields
        builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);

        // Relationships
        builder.HasOne(e => e.Product)
               .WithMany(p => p.Keys)
               .HasForeignKey(e => e.ProductId)
               .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.ProductId)
               .HasDatabaseName("IX_ProductKeys_ProductId");

        builder.HasIndex(e => new { e.ProductId, e.KeyVersion })
               .IsUnique()
               .HasDatabaseName("IX_ProductKeys_ProductId_KeyVersion");

        builder.HasIndex(e => e.IsActive)
               .HasDatabaseName("IX_ProductKeys_IsActive");

        builder.HasIndex(e => e.KeyGeneratedAt)
               .HasDatabaseName("IX_ProductKeys_KeyGeneratedAt");

        builder.HasIndex(e => e.ExpiresAt)
               .HasDatabaseName("IX_ProductKeys_ExpiresAt");

        // Table name
        builder.ToTable("product_keys");
    }
}
