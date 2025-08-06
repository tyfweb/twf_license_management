using Microsoft.EntityFrameworkCore; 
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Infrastructure.Data.Entities.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Audit;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Notification;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.License;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Settings;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.User;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.OperationsDashboard;

namespace TechWayFit.Licensing.Management.Infrastructure.Data.Context;

/// <summary>
/// Entity Framework DbContext for the licensing management system
/// </summary>
public class LicensingDbContext : DbContext
{
    public LicensingDbContext(DbContextOptions<LicensingDbContext> options) : base(options)
    {
    }

    #region DbSets


    // Operations Dashboard related entities
    public DbSet<SystemMetricEntity> SystemMetrics { get; set; }
    public DbSet<ErrorLogSummaryEntity> ErrorLogSummaries { get; set; }
    public DbSet<PagePerformanceMetricEntity> PagePerformanceMetrics { get; set; }
    public DbSet<QueryPerformanceMetricEntity> QueryPerformanceMetrics { get; set; }
    public DbSet<SystemHealthSnapshotEntity> SystemHealthSnapshots { get; set; }

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
  
        ConfigureOperationsDashboardEntities(modelBuilder);

        // Configure indexes
        ConfigureIndexes(modelBuilder);
    }

    
    /// <summary>
    /// Configure additional indexes for performance
    /// </summary>
    private static void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // Additional performance indexes can be added here
        // These are indexes that span multiple entities or are for specific query patterns
    }

    /// <summary>
    /// Override SaveChanges to update audit fields and convert DateTime values to UTC automatically
    /// </summary>
    public override int SaveChanges()
    {
        UpdateAuditFields();
        ConvertDateTimesToUtc();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to update audit fields and convert DateTime values to UTC automatically
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        ConvertDateTimesToUtc();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Update audit fields before saving
    /// </summary>
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseDbEntity>();
        var currentTime = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedOn = currentTime;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedOn = currentTime;
                    entry.Property(x => x.CreatedBy).IsModified = false;
                    entry.Property(x => x.CreatedOn).IsModified = false;
                    break;
            }
        }
    }

    /// <summary>
    /// Convert all DateTime properties to UTC before saving to PostgreSQL
    /// This ensures PostgreSQL compatibility with 'timestamp with time zone' columns
    /// </summary>
    private void ConvertDateTimesToUtc()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            foreach (var property in entry.Properties)
            {
                if (property.Metadata.ClrType == typeof(DateTime) || 
                    property.Metadata.ClrType == typeof(DateTime?))
                {
                    var value = property.CurrentValue;
                    
                    if (value is DateTime dateTime)
                    {
                        // Convert Local or Unspecified DateTime to UTC
                        if (dateTime.Kind == DateTimeKind.Local)
                        {
                            property.CurrentValue = dateTime.ToUniversalTime();
                        }
                        else if (dateTime.Kind == DateTimeKind.Unspecified)
                        {
                            // Assume local time and convert to UTC
                            property.CurrentValue = DateTime.SpecifyKind(dateTime, DateTimeKind.Local).ToUniversalTime();
                        }
                        // UTC DateTime values are left unchanged
                    }
                }
            }
        }
    }


    /// <summary>  
    /// Configure Operations Dashboard related entities
    /// </summary>
    private static void ConfigureOperationsDashboardEntities(ModelBuilder modelBuilder)
    {
        // SystemMetricEntity configuration
        modelBuilder.Entity<SystemMetricEntity>(entity =>
        {
            entity.HasKey(e => e.MetricId);
            entity.Property(e => e.MetricId).IsRequired();
            entity.Property(e => e.MetricType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Controller).HasMaxLength(100);
            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.HttpMethod).HasMaxLength(10);
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Indexes
            entity.HasIndex(e => new { e.TimestampHour, e.MetricType });
            entity.HasIndex(e => e.MetricType);
            entity.HasIndex(e => e.TimestampHour);
            entity.HasIndex(e => new { e.Controller, e.Action });
        });

        // ErrorLogSummaryEntity configuration
        modelBuilder.Entity<ErrorLogSummaryEntity>(entity =>
        {
            entity.HasKey(e => e.ErrorSummaryId);
            entity.Property(e => e.ErrorSummaryId).IsRequired();
            entity.Property(e => e.ErrorType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ErrorSource).HasMaxLength(500);
            entity.Property(e => e.ErrorMessageHash).HasMaxLength(64).IsRequired();
            entity.Property(e => e.ErrorMessageSample).HasMaxLength(2000);
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Indexes
            entity.HasIndex(e => new { e.TimestampHour, e.ErrorType });
            entity.HasIndex(e => e.ErrorType);
            entity.HasIndex(e => e.TimestampHour);
            entity.HasIndex(e => e.ErrorMessageHash);
            entity.HasIndex(e => e.ErrorSource);
        });

        // PagePerformanceMetricEntity configuration
        modelBuilder.Entity<PagePerformanceMetricEntity>(entity =>
        {
            entity.HasKey(e => e.PerformanceId);
            entity.Property(e => e.PerformanceId).IsRequired();
            entity.Property(e => e.Controller).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
            entity.Property(e => e.RouteTemplate).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Indexes
            entity.HasIndex(e => new { e.TimestampHour, e.Controller, e.Action });
            entity.HasIndex(e => new { e.Controller, e.Action });
            entity.HasIndex(e => e.TimestampHour);
        });

        // QueryPerformanceMetricEntity configuration
        modelBuilder.Entity<QueryPerformanceMetricEntity>(entity =>
        {
            entity.HasKey(e => e.QueryMetricId);
            entity.Property(e => e.QueryMetricId).IsRequired();
            entity.Property(e => e.QueryHash).HasMaxLength(64).IsRequired();
            entity.Property(e => e.QueryType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.TableNames).HasMaxLength(500);
            entity.Property(e => e.OperationContext).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Indexes
            entity.HasIndex(e => new { e.TimestampHour, e.QueryType });
            entity.HasIndex(e => e.QueryType);
            entity.HasIndex(e => e.QueryHash);
            entity.HasIndex(e => e.TimestampHour);
            entity.HasIndex(e => e.TableNames);
        });

        // SystemHealthSnapshotEntity configuration
        modelBuilder.Entity<SystemHealthSnapshotEntity>(entity =>
        {
            entity.HasKey(e => e.SnapshotId);
            entity.Property(e => e.SnapshotId).IsRequired();
            entity.Property(e => e.OverallHealthStatus).HasMaxLength(50).IsRequired();
            entity.Property(e => e.HealthIssuesJson).HasMaxLength(4000);
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // Indexes
            entity.HasIndex(e => e.SnapshotTimestamp);
            entity.HasIndex(e => e.OverallHealthStatus);
            entity.HasIndex(e => e.ErrorRatePercent);
        });
    }
}
