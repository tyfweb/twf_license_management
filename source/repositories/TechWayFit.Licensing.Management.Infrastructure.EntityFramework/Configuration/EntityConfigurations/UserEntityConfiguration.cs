using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.User;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration.EntityConfigurations;

/// <summary>
/// Entity configuration for User-related entities
/// </summary>
public class UserProfileEntityConfiguration : IEntityTypeConfiguration<UserProfileEntity>
{
    public void Configure(EntityTypeBuilder<UserProfileEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.UserName).HasMaxLength(50).IsRequired();
        builder.Property(e => e.PasswordHash).HasMaxLength(256).IsRequired();
        builder.Property(e => e.PasswordSalt).HasMaxLength(128).IsRequired();
        builder.Property(e => e.FullName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Email).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Department).HasMaxLength(100);
        builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);

        // Unique constraints
        builder.HasIndex(e => e.UserName).IsUnique();
        builder.HasIndex(e => e.Email).IsUnique();

        // Indexes
        builder.HasIndex(e => e.FullName);
        builder.HasIndex(e => e.Department);
        builder.HasIndex(e => e.IsLocked);
        builder.HasIndex(e => e.IsDeleted);
        builder.HasIndex(e => e.IsAdmin);
        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => new { e.TenantId, e.IsActive, e.IsLocked });
    }
}

public class UserRoleEntityConfiguration : IEntityTypeConfiguration<UserRoleEntity>
{
    public void Configure(EntityTypeBuilder<UserRoleEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.RoleName).HasMaxLength(50).IsRequired();
        builder.Property(e => e.RoleDescription).HasMaxLength(500);
        builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);

        // Unique constraint
        builder.HasIndex(e => e.RoleName).IsUnique();

        // Indexes
        builder.HasIndex(e => e.IsAdmin);
        builder.HasIndex(e => e.TenantId);
    }
}

public class UserRoleMappingEntityConfiguration : IEntityTypeConfiguration<UserRoleMappingEntity>
{
    public void Configure(EntityTypeBuilder<UserRoleMappingEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.RoleId).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);

        // Foreign key relationships
        builder.HasOne(e => e.User)
              .WithMany(u => u.UserRoles)
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Role)
              .WithMany(r => r.UserRoles)
              .HasForeignKey(e => e.RoleId)
              .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint - user can only have one active mapping to a specific role
        builder.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();

        // Indexes
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.RoleId);
        builder.HasIndex(e => e.AssignedDate);
        builder.HasIndex(e => e.ExpiryDate);
        builder.HasIndex(e => e.TenantId);
    }
}
