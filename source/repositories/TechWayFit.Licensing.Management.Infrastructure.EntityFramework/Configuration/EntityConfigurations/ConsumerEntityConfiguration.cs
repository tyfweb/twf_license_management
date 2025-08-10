using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechWayFit.Licensing.Management.Infrastructure.Data.Entities.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Consumer;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration.EntityConfigurations;

/// <summary>
/// Entity configuration for Consumer-related entities
/// </summary>
public class ConsumerAccountEntityConfiguration : IEntityTypeConfiguration<ConsumerAccountEntity>
{
    public void Configure(EntityTypeBuilder<ConsumerAccountEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.CompanyName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.AccountCode).HasMaxLength(50);
        builder.Property(e => e.PrimaryContactName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.PrimaryContactEmail).HasMaxLength(255).IsRequired();
        builder.Property(e => e.PrimaryContactPhone).HasMaxLength(20).IsRequired();
        builder.Property(e => e.PrimaryContactPosition).HasMaxLength(100).IsRequired();
        builder.Property(e => e.SecondaryContactName).HasMaxLength(100);
        builder.Property(e => e.SecondaryContactEmail).HasMaxLength(255);
        builder.Property(e => e.SecondaryContactPhone).HasMaxLength(20);
        builder.Property(e => e.SecondaryContactPosition).HasMaxLength(100);
        builder.Property(e => e.AddressStreet).HasMaxLength(200).IsRequired();
        builder.Property(e => e.AddressCity).HasMaxLength(100).IsRequired();
        builder.Property(e => e.AddressState).HasMaxLength(100).IsRequired();
        builder.Property(e => e.AddressPostalCode).HasMaxLength(20).IsRequired();
        builder.Property(e => e.AddressCountry).HasMaxLength(100).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);

        // Unique constraints
        builder.HasIndex(e => e.PrimaryContactEmail).IsUnique();
        builder.HasIndex(e => e.AccountCode).IsUnique();

        // Indexes
        builder.HasIndex(e => e.CompanyName);
        builder.HasIndex(e => e.AddressCountry);
        builder.HasIndex(e => e.AddressState);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => new { e.TenantId, e.IsActive });
    }
}

public class ProductConsumerEntityConfiguration : IEntityTypeConfiguration<ProductConsumerEntity>
{
    public void Configure(EntityTypeBuilder<ProductConsumerEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.ProductId).IsRequired();
        builder.Property(e => e.ConsumerId).IsRequired();
        builder.Property(e => e.AccountManagerName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.AccountManagerEmail).HasMaxLength(255).IsRequired();
        builder.Property(e => e.AccountManagerPhone).HasMaxLength(20).IsRequired();
        builder.Property(e => e.AccountManagerPosition).HasMaxLength(100).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);

        // Foreign key relationships
        builder.HasOne(e => e.Product)
              .WithMany(p => p.ProductConsumers)
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Consumer)
              .WithMany(ca => ca.ProductConsumers)
              .HasForeignKey(e => e.ConsumerId)
              .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint - consumer can only have one active mapping to a specific product
        builder.HasIndex(e => new { e.ProductId, e.ConsumerId }).IsUnique();

        // Indexes
        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => e.ConsumerId);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.TenantId);
    }
}
