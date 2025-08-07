using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Management.Infrastructure.SqlServer.Configuration;

namespace TechWayFit.Licensing.Management.Infrastructure.SqlServer.Helpers;

/// <summary>
/// Helper class for SQL Server connection string building and validation
/// </summary>
public static class SqlServerConnectionHelper
{
    /// <summary>
    /// Builds a SQL Server connection string from individual components
    /// </summary>
    /// <param name="server">Server name or address</param>
    /// <param name="database">Database name</param>
    /// <param name="userId">User ID (optional for Windows Authentication)</param>
    /// <param name="password">Password (optional for Windows Authentication)</param>
    /// <param name="integratedSecurity">Use Windows Authentication</param>
    /// <param name="connectionTimeout">Connection timeout in seconds</param>
    /// <param name="commandTimeout">Command timeout in seconds</param>
    /// <param name="encrypt">Enable encryption</param>
    /// <param name="trustServerCertificate">Trust server certificate</param>
    /// <param name="applicationName">Application name for connection tracking</param>
    /// <returns>Built connection string</returns>
    public static string BuildConnectionString(
        string server,
        string database,
        string? userId = null,
        string? password = null,
        bool integratedSecurity = true,
        int connectionTimeout = 15,
        int commandTimeout = 30,
        bool encrypt = true,
        bool trustServerCertificate = false,
        string applicationName = "TechWayFit.Licensing.Management")
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = server,
            InitialCatalog = database,
            ConnectTimeout = connectionTimeout,
            CommandTimeout = commandTimeout,
            ApplicationName = applicationName,
            Encrypt = encrypt,
            TrustServerCertificate = trustServerCertificate
        };

        if (integratedSecurity)
        {
            builder.IntegratedSecurity = true;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required when not using integrated security", nameof(userId));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is required when not using integrated security", nameof(password));

            builder.UserID = userId;
            builder.Password = password;
        }

        return builder.ConnectionString;
    }

    /// <summary>
    /// Builds a connection string from SqlServerOptions configuration
    /// </summary>
    /// <param name="options">SQL Server configuration options</param>
    /// <param name="server">Server name or address</param>
    /// <param name="database">Database name</param>
    /// <param name="userId">User ID (optional)</param>
    /// <param name="password">Password (optional)</param>
    /// <param name="integratedSecurity">Use Windows Authentication</param>
    /// <returns>Built connection string</returns>
    public static string BuildConnectionString(
        SqlServerOptions options,
        string server,
        string database,
        string? userId = null,
        string? password = null,
        bool integratedSecurity = true)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = server,
            InitialCatalog = database,
            ConnectTimeout = options.ConnectionPool.ConnectionTimeout,
            CommandTimeout = options.CommandTimeout,
            ApplicationName = "TechWayFit.Licensing.Management",
            Encrypt = true,
            TrustServerCertificate = false,
            MaxPoolSize = options.ConnectionPool.MaxPoolSize,
            MinPoolSize = options.ConnectionPool.MinPoolSize,
            LoadBalanceTimeout = options.ConnectionPool.ConnectionLifetime
        };

        if (integratedSecurity)
        {
            builder.IntegratedSecurity = true;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required when not using integrated security", nameof(userId));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is required when not using integrated security", nameof(password));

            builder.UserID = userId;
            builder.Password = password;
        }

        return builder.ConnectionString;
    }

    /// <summary>
    /// Validates a SQL Server connection string by attempting to connect
    /// </summary>
    /// <param name="connectionString">Connection string to validate</param>
    /// <param name="logger">Logger for diagnostics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if connection is valid, false otherwise</returns>
    public static async Task<bool> ValidateConnectionAsync(
        string connectionString,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            logger?.LogError("Connection string is null or empty");
            return false;
        }

        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            logger?.LogInformation("SQL Server connection validation successful");
            return true;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "SQL Server connection validation failed");
            return false;
        }
    }

    /// <summary>
    /// Tests database connectivity and returns server information
    /// </summary>
    /// <param name="connectionString">Connection string to test</param>
    /// <param name="logger">Logger for diagnostics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Server information or null if connection fails</returns>
    public static async Task<SqlServerInfo?> GetServerInfoAsync(
        string connectionString,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 
                    @@VERSION as Version,
                    @@SERVERNAME as ServerName,
                    DB_NAME() as DatabaseName,
                    SYSTEM_USER as CurrentUser,
                    GETDATE() as ServerTime";

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                var info = new SqlServerInfo
                {
                    Version = reader["Version"]?.ToString() ?? "Unknown",
                    ServerName = reader["ServerName"]?.ToString() ?? "Unknown",
                    DatabaseName = reader["DatabaseName"]?.ToString() ?? "Unknown",
                    CurrentUser = reader["CurrentUser"]?.ToString() ?? "Unknown",
                    ServerTime = reader["ServerTime"] as DateTime? ?? DateTime.MinValue
                };

                logger?.LogInformation("Retrieved SQL Server info: {ServerName}, Version: {Version}", 
                    info.ServerName, info.Version);
                
                return info;
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to retrieve SQL Server information");
        }

        return null;
    }

    /// <summary>
    /// Checks if a database exists on the server
    /// </summary>
    /// <param name="connectionString">Connection string (can point to master or target database)</param>
    /// <param name="databaseName">Name of database to check</param>
    /// <param name="logger">Logger for diagnostics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if database exists, false otherwise</returns>
    public static async Task<bool> DatabaseExistsAsync(
        string connectionString,
        string databaseName,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Modify connection string to connect to master database
            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "master"
            };

            using var connection = new SqlConnection(builder.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = @DatabaseName";
            command.Parameters.AddWithValue("@DatabaseName", databaseName);

            var count = (int)await command.ExecuteScalarAsync(cancellationToken);
            var exists = count > 0;

            logger?.LogInformation("Database '{DatabaseName}' exists: {Exists}", databaseName, exists);
            return exists;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to check if database '{DatabaseName}' exists", databaseName);
            return false;
        }
    }
}

/// <summary>
/// Information about SQL Server instance
/// </summary>
public class SqlServerInfo
{
    /// <summary>
    /// SQL Server version information
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Server name
    /// </summary>
    public string ServerName { get; set; } = string.Empty;

    /// <summary>
    /// Database name
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// Current user context
    /// </summary>
    public string CurrentUser { get; set; } = string.Empty;

    /// <summary>
    /// Server current time
    /// </summary>
    public DateTime ServerTime { get; set; }
}
