using Microsoft.EntityFrameworkCore;
using System.Linq;
using TechWayFit.Licensing.Management.Core.Contracts;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Data;

/// <summary>
/// PostgreSQL-specific implementation of EfCoreLicensingDbContext
/// Inherits all table configurations and multi-tenant functionality from the base context
/// </summary>
public class PostgreSqlLicensingDbContext : EfCoreLicensingDbContext
{
    /// <summary>
    /// Initializes a new instance of PostgreSqlLicensingDbContext
    /// </summary>
    /// <param name="options">DbContext options configured for PostgreSQL</param>
    /// <param name="userContext">User context for multi-tenant operations</param>
    public PostgreSqlLicensingDbContext(
        DbContextOptions<EfCoreLicensingDbContext> options, 
        IUserContext userContext) : base(options, userContext)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Call base configuration first
        base.OnModelCreating(modelBuilder);

        // Configure PostgreSQL-specific settings
        ConfigurePostgreSqlSpecificSettings(modelBuilder);
    }

    private void ConfigurePostgreSqlSpecificSettings(ModelBuilder modelBuilder)
    {
        // Configure schema
        modelBuilder.HasDefaultSchema("licensing");

        // Apply snake_case naming convention for PostgreSQL
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            if (!string.IsNullOrEmpty(tableName))
            {
                entity.SetTableName(ConvertToSnakeCase(tableName));
            }

            foreach (var property in entity.GetProperties())
            {
                var columnName = property.GetColumnName();
                if (!string.IsNullOrEmpty(columnName))
                {
                    property.SetColumnName(ConvertToSnakeCase(columnName));
                }
            }

            foreach (var foreignKey in entity.GetForeignKeys())
            {
                var constraintName = foreignKey.GetConstraintName();
                if (!string.IsNullOrEmpty(constraintName))
                {
                    foreignKey.SetConstraintName(ConvertToSnakeCase(constraintName));
                }
            }

            foreach (var index in entity.GetIndexes())
            {
                var indexName = index.GetDatabaseName();
                if (!string.IsNullOrEmpty(indexName))
                {
                    index.SetDatabaseName(ConvertToSnakeCase(indexName));
                }
            }
        }
    }

    /// <summary>
    /// Converts PascalCase string to snake_case
    /// </summary>
    /// <param name="input">Input string in PascalCase</param>
    /// <returns>String converted to snake_case</returns>
    private static string ConvertToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return string.Concat(input.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
    }
}
